using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elevator.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateElevatorTableRemoveThreadId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManageThreadId",
                table: "DbElevators");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManageThreadId",
                table: "DbElevators",
                type: "int",
                nullable: true);
        }
    }
}
