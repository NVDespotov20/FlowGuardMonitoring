using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowGuardMonitoring.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Fix_RawValue_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RawValue",
                table: "Measurements",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RawValue",
                table: "Measurements",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
