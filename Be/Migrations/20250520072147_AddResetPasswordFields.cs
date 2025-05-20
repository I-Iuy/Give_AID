using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Be.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "ACCOUNT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "ACCOUNT",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "ACCOUNT");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "ACCOUNT");
        }
    }
}
