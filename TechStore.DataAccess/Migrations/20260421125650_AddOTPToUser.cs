using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOTPToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OTPCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OTPExpiry",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTPCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OTPExpiry",
                table: "AspNetUsers");
        }
    }
}
