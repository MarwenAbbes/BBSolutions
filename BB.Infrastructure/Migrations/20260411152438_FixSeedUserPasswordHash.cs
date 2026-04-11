using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedUserPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$e77yzVgdvLzdGPVzCYwym..DjHcJYObz4NCah3hSCnxm3EH4bGYR2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "123qwe");
        }
    }
}
