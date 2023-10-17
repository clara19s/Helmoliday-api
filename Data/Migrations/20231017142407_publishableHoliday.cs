using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HELMoliday.Migrations
{
    public partial class publishableHoliday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Published",
                table: "Holidays",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Published",
                table: "Holidays");
        }
    }
}
