using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HELMoliday.Migrations
{
    public partial class addCategoryAndContactPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPerson_FirstName",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson_LastName",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson_phoneNumber",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPerson_FirstName",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "ContactPerson_LastName",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "ContactPerson_phoneNumber",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Activities");
        }
    }
}
