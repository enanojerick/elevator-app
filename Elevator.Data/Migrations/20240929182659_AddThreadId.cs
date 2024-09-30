using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elevator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManageThreadId",
                table: "DbElevatorProgress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManageThreadId",
                table: "DbElevatorProgress");
        }
    }
}
