using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eryth.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_TrackId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "PreferredCulture",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_CreatedAt",
                table: "Tracks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Status",
                table: "Tracks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Status_DeletedAt",
                table: "Tracks",
                columns: new[] { "Status", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TrackId_IsDeleted",
                table: "Comments",
                columns: new[] { "TrackId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_CreatedAt",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_Status",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Tracks_Status_DeletedAt",
                table: "Tracks");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TrackId_IsDeleted",
                table: "Comments");

            migrationBuilder.AddColumn<string>(
                name: "PreferredCulture",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TrackId",
                table: "Comments",
                column: "TrackId");
        }
    }
}
