using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HELMoliday.Migrations
{
    public partial class RemoveUnfolding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Unfoldings");

            migrationBuilder.AddColumn<Guid>(
                name: "HolidayId",
                table: "Activities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Activities_HolidayId",
                table: "Activities",
                column: "HolidayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Holidays_HolidayId",
                table: "Activities",
                column: "HolidayId",
                principalTable: "Holidays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Holidays_HolidayId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_HolidayId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "HolidayId",
                table: "Activities");

            migrationBuilder.CreateTable(
                name: "Unfoldings",
                columns: table => new
                {
                    HolidayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unfoldings", x => new { x.HolidayId, x.ActivityId });
                    table.ForeignKey(
                        name: "FK_Unfoldings_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Unfoldings_Holidays_HolidayId",
                        column: x => x.HolidayId,
                        principalTable: "Holidays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Unfoldings_ActivityId",
                table: "Unfoldings",
                column: "ActivityId");
        }
    }
}
