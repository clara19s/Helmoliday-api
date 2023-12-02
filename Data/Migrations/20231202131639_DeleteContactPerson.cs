using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HELMoliday.Migrations
{
    public partial class DeleteContactPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ContactPerson_FirstName",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "ContactPerson_LastName",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "ContactPerson_phoneNumber",
                table: "Holidays");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "Invitations",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
        }
    }
}
