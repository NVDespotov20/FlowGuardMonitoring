using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowGuardMonitoring.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMeasurementEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contaminants",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "QualityIndex",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "WaterLevel",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "pH",
                table: "Measurements");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Measurements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "Measurements");

            migrationBuilder.AddColumn<string>(
                name: "Contaminants",
                table: "Measurements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "QualityIndex",
                table: "Measurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Temperature",
                table: "Measurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WaterLevel",
                table: "Measurements",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "pH",
                table: "Measurements",
                type: "real",
                nullable: true);
        }
    }
}
