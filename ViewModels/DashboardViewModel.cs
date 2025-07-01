using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    /// ViewModel hakkında kısmındaki istatistik için
    public class AboutPageViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTracks { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPlaylists { get; set; }
        public long TotalPlays { get; set; }
    }

    /// ViewModel ana sayfa
    public class DashboardViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public DashboardStatsViewModel Stats { get; set; } = new();

        public List<TrackViewModel> RecentTracks { get; set; } = new();
        public List<AlbumViewModel> RecentAlbums { get; set; } = new();
        public List<PlaylistViewModel> RecentPlaylists { get; set; } = new();
        public List<NotificationViewModel> RecentNotifications { get; set; } = new();

        public List<TrackViewModel> TrendingTracks { get; set; } = new();
        public List<AlbumViewModel> TrendingAlbums { get; set; } = new();
        public List<UserViewModel> SuggestedUsers { get; set; } = new();

        public List<TrackViewModel> RecentlyPlayed { get; set; } = new();
        public List<TrackViewModel> LikedTracks { get; set; } = new();

        public List<TrackViewModel> DraftTracks { get; set; } = new();
        public List<AlbumViewModel> DraftAlbums { get; set; } = new();

        public bool HasUnreadNotifications { get; set; }
        public bool HasPendingUploads { get; set; }
        public bool HasDrafts { get; set; }

        public bool ShowTrending { get; set; } = true;
        public bool ShowRecommendations { get; set; } = true;
        public bool ShowActivity { get; set; } = true;

        public List<PlaylistViewModel> PopularPlaylists { get; set; } = new();
        public bool IsLoggedIn { get; set; }

        public bool HasRecentActivity => RecentTracks.Any() || RecentAlbums.Any() || RecentPlaylists.Any();
        public bool HasContent => Stats.TotalTracks > 0 || Stats.TotalAlbums > 0 || Stats.TotalPlaylists > 0;
    }

    /// ViewModel dashboard istatistik için
    public class DashboardStatsViewModel
    {
        // Content counts
        public int TotalTracks { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }

        public int TotalPlays { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }

        public int PlaysThisMonth { get; set; }
        public int LikesThisMonth { get; set; }
        public int NewFollowersThisMonth { get; set; }
        public int TracksUploadedThisMonth { get; set; }

        public double PlayGrowthPercentage { get; set; }
        public double LikeGrowthPercentage { get; set; }
        public double FollowerGrowthPercentage { get; set; }

        public long StorageUsedBytes { get; set; }
        public long StorageLimitBytes { get; set; }
        public int UploadLimitRemaining { get; set; }

        public string StorageUsedFormatted => FormatFileSize(StorageUsedBytes);
        public string StorageLimitFormatted => FormatFileSize(StorageLimitBytes);
        public double StorageUsedPercentage => StorageLimitBytes > 0 ? (double)StorageUsedBytes / StorageLimitBytes * 100 : 0;
        public bool IsStorageNearLimit => StorageUsedPercentage > 80;
        public bool IsUploadLimitNearExhausted => UploadLimitRemaining <= 5;

        public string GetGrowthIcon(double percentage)
        {
            return percentage switch
            {
                > 0 => "📈",
                < 0 => "📉",
                _ => "➖"
            };
        }

        public string GetGrowthColor(double percentage)
        {
            return percentage switch
            {
                > 0 => "text-success",
                < 0 => "text-danger",
                _ => "text-muted"
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// ViewModel analitik veriler için
    public class AnalyticsViewModel
    {
        public int UserId { get; set; }
        public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime DateTo { get; set; } = DateTime.UtcNow;

        public AnalyticsOverviewViewModel Overview { get; set; } = new();

        public List<DailyAnalyticsViewModel> DailyData { get; set; } = new();
        public List<MonthlyAnalyticsViewModel> MonthlyData { get; set; } = new();

        public List<TrackAnalyticsViewModel> TopTracks { get; set; } = new();
        public List<AlbumAnalyticsViewModel> TopAlbums { get; set; } = new();
        public List<PlaylistAnalyticsViewModel> TopPlaylists { get; set; } = new();

        public List<CountryAnalyticsViewModel> TopCountries { get; set; } = new();
        public List<DeviceAnalyticsViewModel> DeviceBreakdown { get; set; } = new();
        public List<AgeGroupAnalyticsViewModel> AgeGroups { get; set; } = new();

        public List<GenreAnalyticsViewModel> GenrePerformance { get; set; } = new();

        public int CurrentListeners { get; set; }
        public List<TrackViewModel> CurrentlyPlaying { get; set; } = new();

        public bool IsLastWeek => DateFrom >= DateTime.UtcNow.AddDays(-7);
        public bool IsLastMonth => DateFrom >= DateTime.UtcNow.AddDays(-30);
        public bool IsLastYear => DateFrom >= DateTime.UtcNow.AddDays(-365);
        public string DateRangeText => $"{DateFrom:MMM dd} - {DateTo:MMM dd, yyyy}";
    }

    public class AnalyticsOverviewViewModel
    {
        public int TotalPlays { get; set; }
        public int UniqueListeners { get; set; }
        public TimeSpan TotalListeningTime { get; set; }
        public int NewFollowers { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }

        public double LikeRate { get; set; }
        public double CommentRate { get; set; }
        public double ShareRate { get; set; }
        public double FollowRate { get; set; }

        public double PlaysChange { get; set; }
        public double ListenersChange { get; set; }
        public double FollowersChange { get; set; }

        public string TotalListeningTimeFormatted => FormatDuration(TotalListeningTime);
        public double AverageListeningTime => UniqueListeners > 0 ? TotalListeningTime.TotalMinutes / UniqueListeners : 0;

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{(int)duration.TotalMinutes}m";
        }
    }

    public class DailyAnalyticsViewModel
    {
        public DateTime Date { get; set; }
        public int Plays { get; set; }
        public int UniqueListeners { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }
        public int NewFollowers { get; set; }

        public string DateFormatted => Date.ToString("MMM dd");
        public string DateShort => Date.ToString("dd");
    }

    public class MonthlyAnalyticsViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Plays { get; set; }
        public int UniqueListeners { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int NewFollowers { get; set; }

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
        public string MonthShort => new DateTime(Year, Month, 1).ToString("MMM");
    }

    public class TrackAnalyticsViewModel
    {
        public int TrackId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverArtUrl { get; set; }
        public int Plays { get; set; }
        public int UniqueListeners { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }
        public TimeSpan TotalListeningTime { get; set; }
        public double CompletionRate { get; set; }
        public double LikeRate { get; set; }

        public string TotalListeningTimeFormatted => FormatDuration(TotalListeningTime);
        public string CompletionRateFormatted => $"{CompletionRate:F1}%";
        public string LikeRateFormatted => $"{LikeRate:F1}%";

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{(int)duration.TotalMinutes}m";
        }
    }

    public class AlbumAnalyticsViewModel
    {
        public int AlbumId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverArtUrl { get; set; }
        public int TotalPlays { get; set; }
        public int UniqueListeners { get; set; }
        public int Likes { get; set; }
        public int TrackCount { get; set; }
        public double AverageCompletionRate { get; set; }

        public double PlaysPerTrack => TrackCount > 0 ? (double)TotalPlays / TrackCount : 0;
        public string CompletionRateFormatted => $"{AverageCompletionRate:F1}%";
    }

    public class PlaylistAnalyticsViewModel
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalPlays { get; set; }
        public int Likes { get; set; }
        public int Followers { get; set; }
        public int TrackCount { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }

        public string AverageSessionDurationFormatted => FormatDuration(AverageSessionDuration);

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{(int)duration.TotalMinutes}m";
        }
    }

    public class CountryAnalyticsViewModel
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public int Plays { get; set; }
        public int UniqueListeners { get; set; }
        public double Percentage { get; set; }

        public string PercentageFormatted => $"{Percentage:F1}%";
    }

    public class DeviceAnalyticsViewModel
    {
        public string DeviceType { get; set; } = string.Empty;
        public int Plays { get; set; }
        public double Percentage { get; set; }

        public string PercentageFormatted => $"{Percentage:F1}%";
        public string DeviceIcon => GetDeviceIcon();

        private string GetDeviceIcon()
        {
            return DeviceType.ToLowerInvariant() switch
            {
                "mobile" => "📱",
                "tablet" => "📱",
                "desktop" => "💻",
                "smart speaker" => "🔊",
                "car" => "🚗",
                _ => "🖥️"
            };
        }
    }
    /// <summary>
    public class AgeGroupAnalyticsViewModel
    {
        public string AgeGroup { get; set; } = string.Empty;
        public int UniqueListeners { get; set; }
        public double Percentage { get; set; }

        public string PercentageFormatted => $"{Percentage:F1}%";
    }

    public class GenreAnalyticsViewModel
    {
        public Genre Genre { get; set; }
        public int TrackCount { get; set; }
        public int TotalPlays { get; set; }
        public int TotalLikes { get; set; }
        public double AverageRating { get; set; }

        public string GenreName => Genre.ToString();
        public double PlaysPerTrack => TrackCount > 0 ? (double)TotalPlays / TrackCount : 0;
        public double LikesPerTrack => TrackCount > 0 ? (double)TotalLikes / TrackCount : 0;
    }

    public class QuickActionsViewModel
    {
        public List<QuickActionItem> Actions { get; set; } = new();

        public static QuickActionsViewModel GetDefault()
        {
            return new QuickActionsViewModel
            {
                Actions = new List<QuickActionItem>
                {
                    new() { Title = "Upload Track", Icon = "🎵", Url = "/upload/track", Color = "primary" },
                    new() { Title = "Create Album", Icon = "💿", Url = "/album/create", Color = "success" },
                    new() { Title = "New Playlist", Icon = "📝", Url = "/playlist/create", Color = "info" },
                    new() { Title = "View Analytics", Icon = "📊", Url = "/analytics", Color = "warning" },
                    new() { Title = "Manage Profile", Icon = "👤", Url = "/profile/edit", Color = "secondary" },
                    new() { Title = "Settings", Icon = "⚙️", Url = "/settings", Color = "dark" }
                }
            };
        }
    }

    public class QuickActionItem
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? BadgeText { get; set; }
        public string? BadgeColor { get; set; }
    }

    public class AnnouncementViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public AnnouncementType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsDismissible { get; set; } = true;
        public bool IsRead { get; set; }

        public string TypeIcon => Type switch
        {
            AnnouncementType.News => "📰",
            AnnouncementType.Feature => "✨",
            AnnouncementType.Maintenance => "🔧",
            AnnouncementType.Warning => "⚠️",
            AnnouncementType.Promotion => "🎉",
            _ => "ℹ️"
        };

        public string TypeColor => Type switch
        {
            AnnouncementType.News => "primary",
            AnnouncementType.Feature => "success",
            AnnouncementType.Maintenance => "warning",
            AnnouncementType.Warning => "danger",
            AnnouncementType.Promotion => "info",
            _ => "secondary"
        };

        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        public string RelativeTime => GetRelativeTime(CreatedAt);

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 => $"{(int)timeSpan.TotalHours}h ago",
                < 7 => $"{(int)timeSpan.TotalDays}d ago",
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }
    }

    public enum AnnouncementType
    {
        [Display(Name = "News")]
        News,
        [Display(Name = "New Feature")]
        Feature,
        [Display(Name = "Maintenance")]
        Maintenance,
        [Display(Name = "Warning")]
        Warning,
        [Display(Name = "Promotion")]
        Promotion
    }
}
