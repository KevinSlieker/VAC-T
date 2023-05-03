using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class QuestionModelOptionsAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OptionAmount",
                table: "Question",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionAmount",
                table: "Question");
        }
    }
}
