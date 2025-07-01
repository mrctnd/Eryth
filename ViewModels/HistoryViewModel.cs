using System.ComponentModel.DataAnnotations;
using Eryth.Models;

namespace Eryth.ViewModels
{
    public class HistoryViewModel
    {
        public Guid Id { get; set; }
        public DateTime PlayedAt { get; set; }
        public int PlayDurationInSeconds { get; set; }
        public double CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSkipped { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceInfo { get; set; }
        public string PlaySource { get; set; } = string.Empty;

        // Track information
        public Guid TrackId { get; set; }
        public string TrackTitle { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string? TrackCoverImageUrl { get; set; }
        public int TrackDurationInSeconds { get; set; }

        // Playlist information (if played from playlist)
        public Guid? PlaylistId { get; set; }
        public string? PlaylistTitle { get; set; }

        // User information
        public Guid UserId { get; set; }
        public string UserDisplayName { get; set; } = string.Empty;

        // Computed properties
        public TimeSpan PlayDuration => TimeSpan.FromSeconds(PlayDurationInSeconds);
        public TimeSpan TrackDuration => TimeSpan.FromSeconds(TrackDurationInSeconds);
        public string FormattedPlayDuration => $"{PlayDuration.Minutes:D2}:{PlayDuration.Seconds:D2}";
        public string FormattedTrackDuration => $"{TrackDuration.Minutes:D2}:{TrackDuration.Seconds:D2}";
        public bool IsValidPlay => PlayDurationInSeconds >= 30 || CompletionPercentage >= 50.0;

        // Factory method to create from domain model
        public static HistoryViewModel FromUserPlayHistory(UserPlayHistory history)
        {
            return new HistoryViewModel
            {
                Id = history.Id,
                PlayedAt = history.PlayedAt,
                PlayDurationInSeconds = history.PlayDurationInSeconds,
                CompletionPercentage = history.CompletionPercentage,
                IsCompleted = history.IsCompleted,
                IsSkipped = history.IsSkipped,
                DeviceType = history.DeviceType,
                DeviceInfo = history.DeviceInfo,
                PlaySource = history.PlaySource,
                TrackId = history.TrackId,
                TrackTitle = history.Track?.Title ?? "Unknown Track",
                ArtistName = history.Track?.Artist?.DisplayName ?? "Unknown Artist",
                TrackCoverImageUrl = history.Track?.CoverImageUrl,
                TrackDurationInSeconds = history.Track?.DurationInSeconds ?? 0,
                PlaylistId = history.PlaylistId,
                PlaylistTitle = history.Playlist?.Title,
                UserId = history.UserId,
                UserDisplayName = history.User?.DisplayName ?? "Unknown User"
            };
        }

        public string GetRelativeTimeDisplay()
        {
            var timeSpan = DateTime.UtcNow - PlayedAt;

            return timeSpan.TotalMinutes switch
            {
                < 1 => "Just now",
                < 60 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1440 => $"{(int)timeSpan.TotalHours}h ago",
                < 10080 => $"{(int)timeSpan.TotalDays}d ago",
                _ => PlayedAt.ToString("MMM dd, yyyy")
            };
        }
    }

    public class PlayHistoryRecordViewModel
    {
        [Required]
        public Guid TrackId { get; set; }

        public Guid? PlaylistId { get; set; }

        [Range(0, int.MaxValue)]
        public int PlayDurationInSeconds { get; set; }

        [Range(0, 100)]
        public double CompletionPercentage { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsSkipped { get; set; }

        [StringLength(50)]
        public string? DeviceType { get; set; }

        [StringLength(200)]
        public string? DeviceInfo { get; set; }
    }

    public class HistoryListViewModel
    {
        public List<HistoryViewModel> History { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Filter properties
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public bool? ValidPlaysOnly { get; set; }
        public string? DeviceType { get; set; }

        // Statistics
        public int TotalPlays { get; set; }
        public int ValidPlays { get; set; }
        public TimeSpan TotalListeningTime { get; set; }
        public int UniqueTracksPlayed { get; set; }
        public int UniqueArtistsPlayed { get; set; }

        // Top items
        public List<TrackPlayStats> TopTracks { get; set; } = new();
        public List<ArtistPlayStats> TopArtists { get; set; } = new();
    }

    public class TrackPlayStats
    {
        public Guid TrackId { get; set; }
        public string TrackTitle { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public int PlayCount { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
        public double AverageCompletionPercentage { get; set; }
    }

    public class ArtistPlayStats
    {
        public Guid ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public int PlayCount { get; set; }
        public int UniqueTracksPlayed { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
    }

    public class HistoryStatsViewModel
    {
        public int TotalPlays { get; set; }
        public int ValidPlays { get; set; }
        public TimeSpan TotalListeningTime { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public int UniqueTracksPlayed { get; set; }
        public int UniqueArtistsPlayed { get; set; }
        public double AverageCompletionRate { get; set; }

        public Dictionary<string, int> PlaysByHour { get; set; } = new();
        public Dictionary<string, int> PlaysByDay { get; set; } = new();
        public Dictionary<string, int> PlaysByMonth { get; set; } = new();

        public Dictionary<string, int> PlaysByDeviceType { get; set; } = new();

        public List<TrackPlayStats> TopTracks { get; set; } = new();
        public List<ArtistPlayStats> TopArtists { get; set; } = new();

        public List<HistoryViewModel> RecentPlays { get; set; } = new();
    }
}
