using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeSync.Execution.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutionJobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceCode = table.Column<string>(type: "text", nullable: false),
                    Stdin = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "QUEUED"),
                    Stdout = table.Column<string>(type: "text", nullable: false),
                    Stderr = table.Column<string>(type: "text", nullable: false),
                    ExitCode = table.Column<int>(type: "integer", nullable: false),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsedKb = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionJobs", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "SupportedLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RunCommand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportedLanguages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SupportedLanguages",
                columns: new[] { "Id", "FileExtension", "IsEnabled", "Name", "RunCommand", "Version" },
                values: new object[,]
                {
                    { 1, ".py", true, "python", "python3 {file}", "3.11" },
                    { 2, ".js", true, "javascript", "node {file}", "Node.js 20" },
                    { 3, ".java", true, "java", "javac {file} && java Main", "21" },
                    { 4, ".cs", true, "csharp", "dotnet script {file}", ".NET 8" },
                    { 5, ".cpp", true, "cpp", "g++ {file} -o out && ./out", "GCC 13" },
                    { 6, ".c", true, "c", "gcc {file} -o out && ./out", "GCC 13" },
                    { 7, ".go", true, "go", "go run {file}", "1.21" },
                    { 8, ".rs", true, "rust", "rustc {file} -o out && ./out", "1.74" },
                    { 9, ".ts", true, "typescript", "ts-node {file}", "5.0" },
                    { 10, ".php", true, "php", "php {file}", "8.2" },
                    { 11, ".rb", true, "ruby", "ruby {file}", "3.2" },
                    { 12, ".kt", true, "kotlin", "kotlinc {file} -include-runtime -d out.jar && java -jar out.jar", "1.9" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionJobs_Language",
                table: "ExecutionJobs",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionJobs_Status",
                table: "ExecutionJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionJobs_UserId",
                table: "ExecutionJobs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecutionJobs");

            migrationBuilder.DropTable(
                name: "SupportedLanguages");
        }
    }
}
