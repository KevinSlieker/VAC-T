using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_AspNetUsers_CandidateId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_AspNetUsers_EmployerId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Solicitation_AppointmentId",
                table: "Solicitation");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_CandidateId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_EmployerId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "Available",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "CandidateId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "EmployerId",
                table: "Appointment");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Appointment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SolicitationId",
                table: "Appointment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitation_AppointmentId",
                table: "Solicitation",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_CompanyId",
                table: "Appointment",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Company_CompanyId",
                table: "Appointment",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Company_CompanyId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Solicitation_AppointmentId",
                table: "Solicitation");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_CompanyId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "SolicitationId",
                table: "Appointment");

            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "Appointment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CandidateId",
                table: "Appointment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerId",
                table: "Appointment",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitation_AppointmentId",
                table: "Solicitation",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_CandidateId",
                table: "Appointment",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_EmployerId",
                table: "Appointment",
                column: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_AspNetUsers_CandidateId",
                table: "Appointment",
                column: "CandidateId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_AspNetUsers_EmployerId",
                table: "Appointment",
                column: "EmployerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
