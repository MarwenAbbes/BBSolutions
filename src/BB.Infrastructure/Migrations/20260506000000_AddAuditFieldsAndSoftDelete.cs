using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BB.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAuditFieldsAndSoftDelete : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            table: "Users");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450);

        migrationBuilder.DropColumn(name: "CreatedBy", table: "Users");
        migrationBuilder.DropColumn(name: "DeletedAt", table: "Users");
        migrationBuilder.DropColumn(name: "DeletedBy", table: "Users");
        migrationBuilder.DropColumn(name: "IsDeleted", table: "Users");
        migrationBuilder.DropColumn(name: "UpdatedAt", table: "Users");
        migrationBuilder.DropColumn(name: "UpdatedBy", table: "Users");
    }
}
