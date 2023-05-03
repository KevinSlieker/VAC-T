using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VACT.Data.Migrations
{
    /// <inheritdoc />
    public partial class QuestionModelOptionsAmountWithS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OptionAmount",
                table: "Question",
                newName: "OptionsAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OptionsAmount",
                table: "Question",
                newName: "OptionAmount");
        }
    }
}
