using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elevator.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveThreadIdFromProgressToElevatorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManageThreadId",
                table: "DbElevatorProgress");

            migrationBuilder.AddColumn<int>(
                name: "ManageThreadId",
                table: "DbElevators",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManageThreadId",
                table: "DbElevators");

            migrationBuilder.AddColumn<string>(
                name: "ManageThreadId",
                table: "DbElevatorProgress",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
