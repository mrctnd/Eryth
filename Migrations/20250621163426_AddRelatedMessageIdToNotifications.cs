using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eryth.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatedMessageIdToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedMessageId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedMessageId",
                table: "Notifications",
                column: "RelatedMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Messages_RelatedMessageId",
                table: "Notifications",
                column: "RelatedMessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Messages_RelatedMessageId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedMessageId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedMessageId",
                table: "Notifications");
        }
    }
}
