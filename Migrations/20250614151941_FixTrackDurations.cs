using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eryth.Migrations
{
    /// <inheritdoc />
    public partial class FixTrackDurations : Migration
    {        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing tracks with 0 duration to have a default duration
            migrationBuilder.Sql(@"
                UPDATE Tracks 
                SET DurationInSeconds = 180 
                WHERE DurationInSeconds = 0 OR DurationInSeconds IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
