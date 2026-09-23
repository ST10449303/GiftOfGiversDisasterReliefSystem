using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfGiversDisasterReliefSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "VolunteerInterests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "VolunteerInterests");
        }
    }
}
