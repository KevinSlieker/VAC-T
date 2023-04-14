using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteBehavourAppointmentWithRepeatAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_RepeatAppointment_RepeatAppointmentId",
                table: "Appointment");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_RepeatAppointment_RepeatAppointmentId",
                table: "Appointment",
                column: "RepeatAppointmentId",
                principalTable: "RepeatAppointment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_RepeatAppointment_RepeatAppointmentId",
                table: "Appointment");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_RepeatAppointment_RepeatAppointmentId",
                table: "Appointment",
                column: "RepeatAppointmentId",
                principalTable: "RepeatAppointment",
                principalColumn: "Id");
        }
    }
}
