using System.Text;
using CodeSync.Events.Files;
using CodeSync.File.DTOs;
using CodeSync.File.Models;
using CodeSync.File.Repositories;
using MassTransit;

namespace CodeSync.File.Services
{
    public class FileServiceImpl : IFileService
    {
        private readonly IFileRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<FileServiceImpl> _logger;

        public FileServiceImpl(
            IFileRepository repo,
            IPublishEndpoint publishEndpoint,
            ILogger<FileServiceImpl> logger)
        {
            _repo            = repo;
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
        }

        // ── Create File ───────────────────────────────────────────────────────

        public async Task<FileResponse> CreateFileAsync(
            int userId, CreateFileRequest req)
        {
            var existing = await _repo
                .FindByProjectIdAndPathAsync(req.ProjectId, req.Path);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A file already exists at path: {req.Path}");

            var file = new CodeFile
            {
                ProjectId    = req.ProjectId,
                Name         = req.Name,
                Path         = req.Path,
                Language     = DetectLanguage(req.Name, req.Language),
                Content      = req.Content,
                Size         = Encoding.UTF8.GetByteCount(req.Content),
                CreatedById  = userId,
                LastEditedBy = userId,
                IsFolder     = false
            };

            await _repo.CreateAsync(file);

            // ✅ SAGA — publish FileCreated event
            // Version-Service listens → creates initial snapshot
            await _publishEndpoint.Publish(new FileCreated(
                FileId:      file.FileId,
                ProjectId:   file.ProjectId,
                Name:        file.Name,
                Path:        file.Path,
                CreatedById: userId,
                CreatedAt:   file.CreatedAt
            ));

            _logger.LogInformation(
                "FileCreated event published for FileId={FileId}", file.FileId);

            return MapToResponse(file);
        }

        // ── Create Folder ─────────────────────────────────────────────────────

        public async Task<FileResponse> CreateFolderAsync(
            int userId, CreateFolderRequest req)
        {
            var existing = await _repo
                .FindByProjectIdAndPathAsync(req.ProjectId, req.Path);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A folder already exists at path: {req.Path}");

            var folder = new CodeFile
            {
                ProjectId    = req.ProjectId,
                Name         = req.Name,
                Path         = req.Path,
                Content      = string.Empty,
                Size         = 0,
                CreatedById  = userId,
                LastEditedBy = userId,
                IsFolder     = true
            };

            await _repo.CreateAsync(folder);
            return MapToResponse(folder);
        }

        // ── Get File ──────────────────────────────────────────────────────────

        public async Task<FileResponse> GetFileByIdAsync(int fileId)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");
            return MapToResponse(file);
        }

        // ── Get File Content ──────────────────────────────────────────────────

        public async Task<FileContentResponse> GetFileContentAsync(int fileId)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");

            if (file.IsDeleted)
                throw new InvalidOperationException("File has been deleted.");

            if (file.IsFolder)
                throw new InvalidOperationException(
                    "Cannot get content of a folder.");

            return new FileContentResponse
            {
                FileId    = file.FileId,
                Name      = file.Name,
                Path      = file.Path,
                Language  = file.Language,
                Content   = file.Content,
                Size      = file.Size,
                UpdatedAt = file.UpdatedAt
            };
        }

        // ── Get Files by Project ──────────────────────────────────────────────

        public async Task<IEnumerable<FileResponse>> GetFilesByProjectAsync(
            int projectId)
        {
            var files = await _repo.FindByProjectIdAsync(projectId);
            return files.Select(MapToResponse);
        }

        // ── Update File Content ───────────────────────────────────────────────

        public async Task<FileContentResponse> UpdateFileContentAsync(
            int fileId, int userId, UpdateFileContentRequest req)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");

            if (file.IsDeleted)
                throw new InvalidOperationException(
                    "Cannot edit a deleted file.");

            if (file.IsFolder)
                throw new InvalidOperationException(
                    "Cannot edit content of a folder.");

            file.Content      = req.Content;
            file.Size         = Encoding.UTF8.GetByteCount(req.Content);
            file.LastEditedBy = userId;

            await _repo.UpdateAsync(file);

            // ✅ SAGA — publish FileUpdated event
            // Version-Service listens → auto creates snapshot on save
            await _publishEndpoint.Publish(new FileUpdated(
                FileId:          file.FileId,
                ProjectId:       file.ProjectId,
                Name:            file.Name,
                UpdatedByUserId: userId,
                UpdatedAt:       file.UpdatedAt
            ));

            _logger.LogInformation(
                "FileUpdated event published for FileId={FileId}", file.FileId);

            return new FileContentResponse
            {
                FileId    = file.FileId,
                Name      = file.Name,
                Path      = file.Path,
                Language  = file.Language,
                Content   = file.Content,
                Size      = file.Size,
                UpdatedAt = file.UpdatedAt
            };
        }

        // ── Rename File ───────────────────────────────────────────────────────

        public async Task<FileResponse> RenameFileAsync(
            int fileId, int userId, RenameFileRequest req)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");

            if (file.IsDeleted)
                throw new InvalidOperationException(
                    "Cannot rename a deleted file.");

            var directory = System.IO.Path
                .GetDirectoryName(file.Path)
                ?.Replace("\\", "/") ?? string.Empty;

            var newPath = string.IsNullOrEmpty(directory)
                ? req.NewName
                : $"{directory}/{req.NewName}";

            file.Name         = req.NewName;
            file.Path         = newPath;
            file.Language     = DetectLanguage(req.NewName, file.Language);
            file.LastEditedBy = userId;

            await _repo.UpdateAsync(file);
            return MapToResponse(file);
        }

        // ── Move File ─────────────────────────────────────────────────────────

        public async Task<FileResponse> MoveFileAsync(
            int fileId, int userId, MoveFileRequest req)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");

            if (file.IsDeleted)
                throw new InvalidOperationException(
                    "Cannot move a deleted file.");

            var existing = await _repo
                .FindByProjectIdAndPathAsync(file.ProjectId, req.NewPath);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A file already exists at: {req.NewPath}");

            file.Path         = req.NewPath;
            file.Name         = req.NewPath.Split('/').Last();
            file.LastEditedBy = userId;

            await _repo.UpdateAsync(file);
            return MapToResponse(file);
        }

        // ── Soft Delete ───────────────────────────────────────────────────────

        public async Task DeleteFileAsync(int fileId, int userId)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");

            file.IsDeleted    = true;
            file.LastEditedBy = userId;

            await _repo.UpdateAsync(file);

            // ✅ SAGA — publish FileDeleted event
            // Version-Service listens → archives all snapshots for this file
            await _publishEndpoint.Publish(new FileDeleted(
                FileId:    file.FileId,
                ProjectId: file.ProjectId,
                DeletedAt: DateTime.UtcNow
            ));

            _logger.LogInformation(
                "FileDeleted event published for FileId={FileId}", file.FileId);
        }

        // ── Restore ───────────────────────────────────────────────────────────

        public async Task RestoreFileAsync(int fileId, int userId)
        {
            var file = await _repo.FindByFileIdAsync(fileId)
                ?? throw new KeyNotFoundException(
                    $"File {fileId} not found.");

            file.IsDeleted    = false;
            file.LastEditedBy = userId;

            await _repo.UpdateAsync(file);
        }

        // ── File Tree ─────────────────────────────────────────────────────────

        public async Task<List<FileTreeNode>> GetFileTreeAsync(int projectId)
        {
            var files = await _repo.FindByProjectIdAsync(projectId);
            return BuildTree(files.ToList(), string.Empty);
        }

        private List<FileTreeNode> BuildTree(
            List<CodeFile> files, string parentPath)
        {
            var nodes = new List<FileTreeNode>();

            var children = files.Where(f =>
            {
                var dir = System.IO.Path
                    .GetDirectoryName(f.Path)
                    ?.Replace("\\", "/") ?? string.Empty;
                return dir == parentPath;
            }).ToList();

            foreach (var file in children)
            {
                var node = new FileTreeNode
                {
                    FileId   = file.FileId,
                    Name     = file.Name,
                    Path     = file.Path,
                    IsFolder = file.IsFolder,
                    Language = file.Language
                };

                if (file.IsFolder)
                    node.Children = BuildTree(files, file.Path);

                nodes.Add(node);
            }

            return nodes;
        }

        // ── Search in Project ─────────────────────────────────────────────────

        public async Task<IEnumerable<SearchResultResponse>> SearchInProjectAsync(
            int projectId, string query)
        {
            var files   = await _repo.FindByProjectIdAsync(projectId);
            var results = new List<SearchResultResponse>();

            foreach (var file in files.Where(f => !f.IsFolder))
            {
                var lines = file.Content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(
                        query, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new SearchResultResponse
                        {
                            FileId      = file.FileId,
                            FileName    = file.Name,
                            FilePath    = file.Path,
                            LineNumber  = i + 1,
                            LineContent = lines[i].Trim()
                        });
                    }
                }
            }

            return results;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string DetectLanguage(string fileName, string fallback)
        {
            var ext = System.IO.Path
                .GetExtension(fileName).ToLower();
            return ext switch
            {
                ".cs"   => "csharp",
                ".py"   => "python",
                ".js"   => "javascript",
                ".ts"   => "typescript",
                ".java" => "java",
                ".go"   => "go",
                ".rs"   => "rust",
                ".cpp"  => "cpp",
                ".c"    => "c",
                ".php"  => "php",
                ".rb"   => "ruby",
                ".kt"   => "kotlin",
                ".html" => "html",
                ".css"  => "css",
                ".json" => "json",
                ".xml"  => "xml",
                ".md"   => "markdown",
                ".sql"  => "sql",
                _       => fallback
            };
        }

        private static FileResponse MapToResponse(CodeFile f) => new()
        {
            FileId       = f.FileId,
            ProjectId    = f.ProjectId,
            Name         = f.Name,
            Path         = f.Path,
            Language     = f.Language,
            Size         = f.Size,
            CreatedById  = f.CreatedById,
            LastEditedBy = f.LastEditedBy,
            IsFolder     = f.IsFolder,
            IsDeleted    = f.IsDeleted,
            CreatedAt    = f.CreatedAt,
            UpdatedAt    = f.UpdatedAt
        };
    }
}