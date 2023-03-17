using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class SolicitationAppointmentOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitation_Appointment_AppointmentId",
                table: "Solicitation");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitation_Appointment_AppointmentId",
                table: "Solicitation",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitation_Appointment_AppointmentId",
                table: "Solicitation");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitation_Appointment_AppointmentId",
                table: "Solicitation",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id");
        }
    }
}
