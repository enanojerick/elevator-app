using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elevator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbElevatorProgress",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false),
                    CurrentDirection = table.Column<int>(type: "int", nullable: false),
                    CurrentFloorsQueued = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbElevatorProgress", x => x.ProgressId);
                });

            migrationBuilder.CreateTable(
                name: "DbElevators",
                columns: table => new
                {
                    CarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentFloor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbElevators", x => x.CarId);
                });

            migrationBuilder.CreateTable(
                name: "DbElevatorsRequest",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    RequestedDirection = table.Column<int>(type: "int", nullable: false),
                    RequestedFromFloor = table.Column<int>(type: "int", nullable: false),
                    RequestedFloors = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbElevatorsRequest", x => x.RequestId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbElevatorProgress");

            migrationBuilder.DropTable(
                name: "DbElevators");

            migrationBuilder.DropTable(
                name: "DbElevatorsRequest");
        }
    }
}
