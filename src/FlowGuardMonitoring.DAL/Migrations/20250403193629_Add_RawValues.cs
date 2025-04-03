using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowGuardMonitoring.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_RawValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RawValue",
                table: "Measurements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawValue",
                table: "Measurements");
        }
    }
}
