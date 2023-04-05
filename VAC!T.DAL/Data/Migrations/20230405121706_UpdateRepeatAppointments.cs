using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRepeatAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatsVar",
                table: "RepeatAppointment");

            migrationBuilder.AddColumn<int>(
                name: "RepeatsDay",
                table: "RepeatAppointment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatsRelativeWeek",
                table: "RepeatAppointment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatsWeekdays",
                table: "RepeatAppointment",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatsDay",
                table: "RepeatAppointment");

            migrationBuilder.DropColumn(
                name: "RepeatsRelativeWeek",
                table: "RepeatAppointment");

            migrationBuilder.DropColumn(
                name: "RepeatsWeekdays",
                table: "RepeatAppointment");

            migrationBuilder.AddColumn<string>(
                name: "RepeatsVar",
                table: "RepeatAppointment",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
