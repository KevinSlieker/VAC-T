using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class SollicitationAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Solicitation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitation_AppointmentId",
                table: "Solicitation",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitation_Appointment_AppointmentId",
                table: "Solicitation",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitation_Appointment_AppointmentId",
                table: "Solicitation");

            migrationBuilder.DropIndex(
                name: "IX_Solicitation_AppointmentId",
                table: "Solicitation");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Solicitation");
        }
    }
}
