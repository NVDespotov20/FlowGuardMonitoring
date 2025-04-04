using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowGuardMonitoring.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_Sensor_Metadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Sensors",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Sensors",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Sensors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                table: "Sensors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Sensors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "ModelNumber",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Sensors");
        }
    }
}
