using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedRepeatAppointmentIdToAppointmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepeatAppointmentId",
                table: "Appointment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_RepeatAppointmentId",
                table: "Appointment",
                column: "RepeatAppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_RepeatAppointment_RepeatAppointmentId",
                table: "Appointment",
                column: "RepeatAppointmentId",
                principalTable: "RepeatAppointment",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_RepeatAppointment_RepeatAppointmentId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_RepeatAppointmentId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "RepeatAppointmentId",
                table: "Appointment");
        }
    }
}
