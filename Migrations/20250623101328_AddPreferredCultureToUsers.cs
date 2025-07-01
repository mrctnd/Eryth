using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eryth.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredCultureToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredCulture",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredCulture",
                table: "Users");
        }
    }
}
