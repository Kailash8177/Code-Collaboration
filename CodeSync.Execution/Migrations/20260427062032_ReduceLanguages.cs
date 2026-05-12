using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodeSync.Execution.Migrations
{
    /// <inheritdoc />
    public partial class ReduceLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "SupportedLanguages",
                keyColumn: "Id",
                keyValue: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SupportedLanguages",
                columns: new[] { "Id", "FileExtension", "IsEnabled", "Name", "RunCommand", "Version" },
                values: new object[,]
                {
                    { 5, ".cpp", true, "cpp", "g++ {file} -o out && ./out", "GCC 13" },
                    { 6, ".c", true, "c", "gcc {file} -o out && ./out", "GCC 13" },
                    { 7, ".go", true, "go", "go run {file}", "1.21" },
                    { 8, ".rs", true, "rust", "rustc {file} -o out && ./out", "1.74" },
                    { 9, ".ts", true, "typescript", "ts-node {file}", "5.0" },
                    { 10, ".php", true, "php", "php {file}", "8.2" },
                    { 11, ".rb", true, "ruby", "ruby {file}", "3.2" },
                    { 12, ".kt", true, "kotlin", "kotlinc {file} -include-runtime -d out.jar && java -jar out.jar", "1.9" }
                });
        }
    }
}
