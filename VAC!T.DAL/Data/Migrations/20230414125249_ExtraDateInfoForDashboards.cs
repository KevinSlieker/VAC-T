using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtraDateInfoForDashboards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateAppointmentSelected",
                table: "Solicitation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSelectedIsTrue",
                table: "Solicitation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Closed",
                table: "JobOffer",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAppointmentSelected",
                table: "Solicitation");

            migrationBuilder.DropColumn(
                name: "DateSelectedIsTrue",
                table: "Solicitation");

            migrationBuilder.DropColumn(
                name: "Closed",
                table: "JobOffer");
        }
    }
}
