using System.Diagnostics;
using System.Text;
using CodeSync.Events.Execution;
using CodeSync.Execution.DTOs;
using CodeSync.Execution.Models;
using CodeSync.Execution.Repositories;
using MassTransit;

namespace CodeSync.Execution.Services
{
    public class ExecutionServiceImpl : IExecutionService
    {
        private readonly IExecutionRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ExecutionServiceImpl> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        // Resource limits
        private const int MaxExecutionSeconds = 120; // Increased to 2 minutes to allow Docker to pull images on first run
        private const int MaxMemoryMb        = 256;

        public ExecutionServiceImpl(
            IExecutionRepository repo,
            IPublishEndpoint publishEndpoint,
            ILogger<ExecutionServiceImpl> logger,
            IHttpClientFactory httpClientFactory)
        {
            _repo            = repo;
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
            _httpClientFactory = httpClientFactory;
        }

        // ── Submit Execution ──────────────────────────────────────────────────

        public async Task<ExecutionJobResponse> SubmitExecutionAsync(
            int userId, SubmitExecutionRequest req)
        {
            // Validate language
            var language = await _repo.FindLanguageByNameAsync(req.Language);
            if (language is null)
                throw new InvalidOperationException(
                    $"Language '{req.Language}' is not supported.");

            // Create job
            var job = new ExecutionJob
            {
                ProjectId  = req.ProjectId,
                FileId     = req.FileId,
                UserId     = userId,
                Language   = req.Language,
                SourceCode = req.SourceCode,
                Stdin      = req.Stdin,
                Status     = "QUEUED"
            };

            await _repo.CreateAsync(job);

            _logger.LogInformation(
                "Job created JobId={JobId} Language={Language}",
                job.JobId, job.Language);

            // Execute synchronously so we can return the result
            await ExecuteJobAsync(job, language);

            return MapToResponse(job);
        }

        // ── Execute Job (runs in background) ──────────────────────────────────

        private async Task ExecuteJobAsync(
            ExecutionJob job, SupportedLanguage language)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Update status to RUNNING
                job.Status = "RUNNING";
                await _repo.UpdateAsync(job);

                _logger.LogInformation(
                    "Job running JobId={JobId}", job.JobId);

                // Write source code to temp file
                var tempDir      = Path.Combine(Path.GetTempPath(), job.JobId.ToString());
                Directory.CreateDirectory(tempDir);

                string command;
                var fileName = $"main{language.FileExtension}";
                var filePath = Path.Combine(tempDir, fileName);
                
                // Secure Docker container options
                var dockerOpts = $"--rm --memory=\"256m\" --cpus=\"1\" -v \"{tempDir}:/app\" -w /app";

                if (language.Name.ToLower() == "csharp")
                {
                    // Create .NET console app locally in temp folder
                    await RunProcessAsync("dotnet new console", tempDir, "", TimeSpan.FromSeconds(5));
                    
                    filePath = Path.Combine(tempDir, "Program.cs");
                    await System.IO.File.WriteAllTextAsync(filePath, job.SourceCode);
                    
                    // Run it inside the .NET SDK Docker container
                    command = $"docker run {dockerOpts} mcr.microsoft.com/dotnet/sdk:8.0 dotnet run";
                }
                else if (language.Name.ToLower() == "python")
                {
                    await System.IO.File.WriteAllTextAsync(filePath, job.SourceCode);
                    command = $"docker run {dockerOpts} python:3.11 python {fileName}";
                }
                else if (language.Name.ToLower() == "javascript")
                {
                    await System.IO.File.WriteAllTextAsync(filePath, job.SourceCode);
                    command = $"docker run {dockerOpts} node:20 node {fileName}";
                }
                else if (language.Name.ToLower() == "java")
                {
                    await System.IO.File.WriteAllTextAsync(filePath, job.SourceCode);
                    
                    // Use Java 11+ single-file source-code execution so users don't have to name their class Main!
                    command = $"docker run {dockerOpts} amazoncorretto:21 bash -c \"java {fileName}\"";
                }
                else
                {
                    await System.IO.File.WriteAllTextAsync(filePath, job.SourceCode);
                    command = language.RunCommand.Replace("{file}", fileName);
                }

                // Run the process
                var result = await RunProcessAsync(
                    command, tempDir, job.Stdin,
                    TimeSpan.FromSeconds(MaxExecutionSeconds));

                stopwatch.Stop();

                // Update job with results
                job.Status          = result.TimedOut ? "TIMED_OUT" : "COMPLETED";
                job.Stdout          = result.Stdout;
                job.Stderr          = result.Stderr;
                job.ExitCode        = result.ExitCode;
                job.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                job.CompletedAt     = DateTime.UtcNow;

                await _repo.UpdateAsync(job);

                // Cleanup temp files
                try { Directory.Delete(tempDir, true); } catch { }

                _logger.LogInformation(
                    "Job completed JobId={JobId} Status={Status} Time={Time}ms",
                    job.JobId, job.Status, job.ExecutionTimeMs);

                // ✅ SAGA — publish CodeExecuted event
                await _publishEndpoint.Publish(new CodeExecuted(
                    JobId:           job.JobId,
                    UserId:          job.UserId,
                    ProjectId:       job.ProjectId,
                    Language:        job.Language,
                    Status:          job.Status,
                    ExecutionTimeMs: (int)job.ExecutionTimeMs,
                    CompletedAt:     job.CompletedAt.Value
                ));

                // 🚀 HTTP FALLBACK: Directly hit the Notification API since RabbitMQ is disabled
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    await client.PostAsJsonAsync("http://localhost:7007/api/notifications/send", new
                    {
                        RecipientId = job.UserId,
                        ActorId = job.UserId,
                        Type = "EXECUTION",
                        Title = $"Code Execution {job.Status}",
                        Message = $"Your {job.Language} code finished in {job.ExecutionTimeMs}ms.",
                        RelatedId = job.JobId.ToString(),
                        RelatedType = "EXECUTION"
                    });
                } catch { }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                job.Status          = "FAILED";
                job.Stderr          = ex.Message;
                job.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                job.CompletedAt     = DateTime.UtcNow;

                await _repo.UpdateAsync(job);

                _logger.LogError(ex,
                    "Job failed JobId={JobId}", job.JobId);

                // ✅ SAGA — publish ExecutionFailed event
                await _publishEndpoint.Publish(new ExecutionFailed(
                    JobId:        job.JobId,
                    UserId:       job.UserId,
                    ProjectId:    job.ProjectId,
                    Language:     job.Language,
                    ErrorMessage: ex.Message,
                    FailedAt:     DateTime.UtcNow
                ));
            }
        }

        // ── Run Process ───────────────────────────────────────────────────────

        private async Task<ProcessResult> RunProcessAsync(
            string command,
            string workingDir,
            string stdin,
            TimeSpan timeout)
        {
            // Split command into executable and arguments
            var parts      = command.Split(' ', 2);
            var executable = parts[0];
            var arguments  = parts.Length > 1 ? parts[1] : string.Empty;

            var startInfo = new ProcessStartInfo
            {
                FileName               = executable,
                Arguments              = arguments,
                WorkingDirectory       = workingDir,
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var process = new Process { StartInfo = startInfo };
            var stdoutBuilder  = new StringBuilder();
            var stderrBuilder  = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                    stdoutBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                    stderrBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Write stdin if provided
            if (!string.IsNullOrEmpty(stdin))
            {
                await process.StandardInput.WriteAsync(stdin);
                process.StandardInput.Close();
            }

            // Wait with timeout
            var completed = await process.WaitForExitAsync(
                new CancellationTokenSource(timeout).Token
            ).ContinueWith(t => !t.IsCanceled);

            if (!completed)
            {
                try { process.Kill(true); } catch { }
                return new ProcessResult
                {
                    Stdout   = stdoutBuilder.ToString(),
                    Stderr   = $"Execution timed out after {timeout.TotalSeconds} seconds. (Docker might be pulling the image for the first time)",
                    ExitCode = -1,
                    TimedOut = true
                };
            }

            return new ProcessResult
            {
                Stdout   = stdoutBuilder.ToString(),
                Stderr   = stderrBuilder.ToString(),
                ExitCode = process.ExitCode,
                TimedOut = false
            };
        }

        // ── Get Job ───────────────────────────────────────────────────────────

        public async Task<ExecutionJobResponse> GetJobByIdAsync(Guid jobId)
        {
            var job = await _repo.FindByJobIdAsync(jobId)
                ?? throw new KeyNotFoundException(
                    $"Job {jobId} not found.");
            return MapToResponse(job);
        }

        // ── Get Executions ────────────────────────────────────────────────────

        public async Task<IEnumerable<ExecutionJobResponse>>
            GetExecutionsByUserAsync(int userId)
        {
            var jobs = await _repo.FindByUserIdAsync(userId);
            return jobs.Select(MapToResponse);
        }

        public async Task<IEnumerable<ExecutionJobResponse>>
            GetExecutionsByProjectAsync(int projectId)
        {
            var jobs = await _repo.FindByProjectIdAsync(projectId);
            return jobs.Select(MapToResponse);
        }

        // ── Cancel Execution ──────────────────────────────────────────────────

        public async Task CancelExecutionAsync(Guid jobId, int userId)
        {
            var job = await _repo.FindByJobIdAsync(jobId)
                ?? throw new KeyNotFoundException(
                    $"Job {jobId} not found.");

            if (job.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only cancel your own jobs.");

            if (job.Status != "QUEUED" && job.Status != "RUNNING")
                throw new InvalidOperationException(
                    $"Cannot cancel job with status {job.Status}.");

            job.Status      = "CANCELLED";
            job.CompletedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(job);
        }

        // ── Get Result ────────────────────────────────────────────────────────

        public async Task<ExecutionJobResponse> GetExecutionResultAsync(Guid jobId)
        {
            var job = await _repo.FindByJobIdAsync(jobId)
                ?? throw new KeyNotFoundException(
                    $"Job {jobId} not found.");
            return MapToResponse(job);
        }

        // ── Supported Languages ───────────────────────────────────────────────

        public async Task<IEnumerable<SupportedLanguageResponse>>
            GetSupportedLanguagesAsync()
        {
            var languages = await _repo.GetSupportedLanguagesAsync();
            return languages.Select(l => new SupportedLanguageResponse
            {
                Id            = l.Id,
                Name          = l.Name,
                Version       = l.Version,
                FileExtension = l.FileExtension,
                IsEnabled     = l.IsEnabled
            });
        }

        // ── Stats ─────────────────────────────────────────────────────────────

        public async Task<ExecutionStatsResponse> GetExecutionStatsAsync(
            int userId)
        {
            var jobs = await _repo.FindByUserIdAsync(userId);
            var list = jobs.ToList();

            return new ExecutionStatsResponse
            {
                TotalJobs     = list.Count,
                CompletedJobs = list.Count(j => j.Status == "COMPLETED"),
                FailedJobs    = list.Count(j => j.Status == "FAILED"),
                CancelledJobs = list.Count(j => j.Status == "CANCELLED"),
                JobsByLanguage = list
                    .GroupBy(j => j.Language)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static ExecutionJobResponse MapToResponse(ExecutionJob j) => new()
        {
            JobId           = j.JobId,
            ProjectId       = j.ProjectId,
            FileId          = j.FileId,
            UserId          = j.UserId,
            Language        = j.Language,
            Status          = j.Status,
            Stdout          = j.Stdout,
            Stderr          = j.Stderr,
            ExitCode        = j.ExitCode,
            ExecutionTimeMs = j.ExecutionTimeMs,
            MemoryUsedKb    = j.MemoryUsedKb,
            CreatedAt       = j.CreatedAt,
            CompletedAt     = j.CompletedAt
        };

        private record ProcessResult
        {
            public string Stdout   { get; init; } = string.Empty;
            public string Stderr   { get; init; } = string.Empty;
            public int    ExitCode { get; init; }
            public bool   TimedOut { get; init; }
        }
    }
}