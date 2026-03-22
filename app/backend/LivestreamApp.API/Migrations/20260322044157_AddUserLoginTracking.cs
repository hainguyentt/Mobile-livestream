using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLoginTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LockedUntil",
                table: "users",
                newName: "LockoutUntil");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCaptcha",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RequiresCaptcha",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "LockoutUntil",
                table: "users",
                newName: "LockedUntil");
        }
    }
}
