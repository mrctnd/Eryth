using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eryth.Migrations
{
    /// <inheritdoc />
    public partial class AddTestMusicData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert test user if not exists
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'mehmet')
                BEGIN
                    INSERT INTO Users (Id, Username, DisplayName, Email, PasswordHash, PasswordSalt, Role, Status, CreatedAt, UpdatedAt, Bio, ProfileImageUrl)
                    VALUES (
                        '12345678-1234-1234-1234-123456789012',
                        'mehmet',
                        'Mehmet',
                        'mehmet@test.com',
                        'test_hash',
                        'test_salt',
                        0,
                        0,
                        GETDATE(),
                        GETDATE(),
                        'Test kullanıcısı - müzik üreticisi',
                        '/images/profile/default-avatar.jpg'
                    )
                END
            ");            // Insert test tracks with actual audio URLs
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Tracks WHERE Title = 'Ghost Of Me Melodic Dubstep NCS - Copy')
                BEGIN
                    INSERT INTO Tracks (Id, Title, ArtistId, AudioFileUrl, CoverImageUrl, DurationInSeconds, Genre, Status, PlayCount, LikeCount, CommentCount, CreatedAt, UpdatedAt, AllowComments, AllowDownloads, ReleaseDate, IsExplicit)
                    VALUES (
                        '11111111-1111-1111-1111-111111111111',
                        'Ghost Of Me Melodic Dubstep NCS - Copy',
                        '12345678-1234-1234-1234-123456789012',
                        'https://www.soundjay.com/misc/sounds/bell-ringing-05.mp3',
                        '/images/tracks/track1.jpg',
                        182,
                        2,
                        0,
                        1250,
                        45,
                        12,
                        GETDATE(),
                        GETDATE(),
                        1,
                        0,
                        GETDATE(),
                        0
                    )
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Tracks WHERE Title = 'Hit The Ground Future House NCS - Copy')
                BEGIN
                    INSERT INTO Tracks (Id, Title, ArtistId, AudioFileUrl, CoverImageUrl, DurationInSeconds, Genre, Status, PlayCount, LikeCount, CommentCount, CreatedAt, UpdatedAt, AllowComments, AllowDownloads, ReleaseDate, IsExplicit)
                    VALUES (
                        '22222222-2222-2222-2222-222222222222',
                        'Hit The Ground Future House NCS - Copy',
                        '12345678-1234-1234-1234-123456789012',
                        'https://www.soundjay.com/misc/sounds/bell-ringing-05.mp3',
                        '/images/tracks/track2.jpg',
                        205,
                        1,
                        0,
                        890,
                        67,
                        8,
                        GETDATE(),
                        GETDATE(),
                        1,
                        1,
                        GETDATE(),
                        0
                    )
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Tracks WHERE Title = 'Electronic Dreams')
                BEGIN
                    INSERT INTO Tracks (Id, Title, ArtistId, AudioFileUrl, CoverImageUrl, DurationInSeconds, Genre, Status, PlayCount, LikeCount, CommentCount, CreatedAt, UpdatedAt, AllowComments, AllowDownloads, ReleaseDate, IsExplicit)
                    VALUES (
                        '33333333-3333-3333-3333-333333333333',
                        'Electronic Dreams',
                        '12345678-1234-1234-1234-123456789012',
                        'https://www.soundjay.com/misc/sounds/bell-ringing-05.mp3',
                        '/images/tracks/track3.jpg',
                        165,
                        2,
                        0,
                        2150,
                        89,
                        23,
                        GETDATE(),
                        GETDATE(),
                        1,
                        0,
                        GETDATE(),
                        0
                    )
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove test data
            migrationBuilder.Sql("DELETE FROM Tracks WHERE Title IN ('Ghost Of Me Melodic Dubstep NCS - Copy', 'Hit The Ground Future House NCS - Copy', 'Electronic Dreams')");
            migrationBuilder.Sql("DELETE FROM Users WHERE Username = 'mehmet'");
        }
    }
}
