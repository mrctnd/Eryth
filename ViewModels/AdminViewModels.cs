using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int ActiveUsersThisWeek { get; set; }

        public int TotalTracks { get; set; }
        public int TracksUploadedToday { get; set; }
        public int TotalPlaylists { get; set; }

        public int PendingReports { get; set; }
        public int SuspendedUsers { get; set; }

        public IEnumerable<UserViewModel> RecentUsers { get; set; } = new List<UserViewModel>();
        public IEnumerable<TrackViewModel> RecentTracks { get; set; } = new List<TrackViewModel>();
        public IEnumerable<ReportViewModel> RecentReports { get; set; } = new List<ReportViewModel>();

        public UserRole CurrentUserRole { get; set; }
    }

    public class AdminUserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string? SearchQuery { get; set; }
        public UserRole? SelectedRole { get; set; }
        public AccountStatus? SelectedStatus { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class AdminUserDetailsViewModel
    {
        public UserViewModel User { get; set; } = null!;
        public IEnumerable<TrackViewModel> UserTracks { get; set; } = new List<TrackViewModel>();
        public IEnumerable<PlaylistViewModel> UserPlaylists { get; set; } = new List<PlaylistViewModel>();
        public IEnumerable<ReportViewModel> UserReports { get; set; } = new List<ReportViewModel>();
        public IEnumerable<ReportViewModel> ReportsAgainstUser { get; set; } = new List<ReportViewModel>();
        public IEnumerable<LoginHistoryViewModel> LoginHistory { get; set; } = new List<LoginHistoryViewModel>();
    }
    public class AdminContentListViewModel
    {
        public string ContentType { get; set; } = "tracks";
        public List<TrackViewModel> Tracks { get; set; } = new();
        public List<PlaylistViewModel> Playlists { get; set; } = new();
        public List<CommentViewModel> Comments { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string? SearchQuery { get; set; }
        public TrackStatus? SelectedStatus { get; set; }
        public bool HasNextPage { get; set; }
    }
    public class AdminReportListViewModel
    {
        public List<ReportViewModel> Reports { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SelectedType { get; set; }
        public bool HasNextPage { get; set; }
        public int PendingReportsCount { get; set; }
        public int TotalReports { get; set; }
        public int TotalPages { get; set; }
    }
    public class AdminLogsViewModel
    {
        public List<LogEntryViewModel> Logs { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string? SelectedLevel { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool HasNextPage { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int TotalLogs { get; set; }
        public int TotalPages { get; set; }
    }
    public class AdminSettingsViewModel
    {
        public bool RegistrationEnabled { get; set; }
        public bool MaintenanceMode { get; set; }
        public long MaxFileSize { get; set; }
        public string AllowedFileTypes { get; set; } = "";
        public string SiteName { get; set; } = "Eryth";
        public string SiteDescription { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public bool EmailVerificationRequired { get; set; } = true;
        public int MaxTracksPerUser { get; set; } = 1000;
        public int MaxPlaylistsPerUser { get; set; } = 100;

        public string SiteUrl { get; set; } = "";
        public string SupportEmail { get; set; } = "";
        public bool AllowUserRegistration { get; set; } = true;
        public bool RequireEmailVerification { get; set; } = true;
        public int MinPasswordLength { get; set; } = 8;
        public int MaxLoginAttempts { get; set; } = 5;
        public int AccountLockoutDuration { get; set; } = 30;
        public int MaxUploadSizeMB { get; set; } = 50;
        public bool RequireContentModeration { get; set; } = false;
        public bool AllowComments { get; set; } = true;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnablePushNotifications { get; set; } = false;
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
        public string StorageProvider { get; set; } = "Local";
        public string StoragePath { get; set; } = "";
        public string CdnUrl { get; set; } = "";
        public bool EnableTwoFactorAuth { get; set; } = false;
        public bool EnableRecaptcha { get; set; } = false;
        public int SessionTimeout { get; set; } = 30;
        public int RateLimit { get; set; } = 60;
    }
    public class AdminAnalyticsViewModel
    {
        public string Period { get; set; } = "week";
        public List<AnalyticsDataPoint> UserGrowthData { get; set; } = new();
        public List<AnalyticsDataPoint> ContentGrowthData { get; set; } = new();
        public List<AnalyticsDataPoint> ActivityData { get; set; } = new();

        // Summary stats
        public int TotalPlaysThisPeriod { get; set; }
        public int TotalUploadsThisPeriod { get; set; }
        public int TotalRegistrationsThisPeriod { get; set; }
        public decimal RevenueThisPeriod { get; set; }

        public int TotalUsers { get; set; }
        public decimal UserGrowthPercentage { get; set; }
        public int TotalTracks { get; set; }
        public decimal TrackGrowthPercentage { get; set; }
        public long TotalPlays { get; set; }
        public decimal PlaysGrowthPercentage { get; set; }
        public long TotalPageViews { get; set; }
        public decimal PageViewsGrowthPercentage { get; set; }
        public List<TrackViewModel> TopTracks { get; set; } = new(); public List<UserViewModel> TopArtists { get; set; } = new();
        public List<LogEntryViewModel> RecentActivity { get; set; } = new();
        public decimal AverageSessionDuration { get; set; }
        public decimal BounceRate { get; set; }
        public decimal AveragePageLoadTime { get; set; }
        public decimal ServerUptime { get; set; }
    }
    public class ReportViewModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public UserViewModel Reporter { get; set; } = null!;
        public UserViewModel? ReportedUser { get; set; }
        public string? ContentType { get; set; }
        public string? ContentId { get; set; }
        public string? ContentTitle { get; set; }
        public string? ReportType { get; set; }
        public string? Priority { get; set; }
        public string? ReporterUsername { get; set; }
        public string? ReportedUsername { get; set; }
        public string? ModeratorUsername { get; set; }
    }

    public class LoginHistoryViewModel
    {
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string Location { get; set; } = "";
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
    }
    public class LogEntryViewModel
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = "";
        public string Message { get; set; } = "";
        public string Category { get; set; } = "";
        public string? Exception { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Username { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? RequestPath { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
    }
    public class AnalyticsDataPoint
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public string? Category { get; set; }
    }

    public class CreateReportViewModel
    {
        public ReportType Type { get; set; }
        public string Reason { get; set; } = "";
        public string? Description { get; set; }

        public int? TargetUserId { get; set; }
        public int? TargetTrackId { get; set; }
        public int? TargetPlaylistId { get; set; }
        public int? TargetCommentId { get; set; }

        public string? TargetUserName { get; set; }
        public string? TargetTrackTitle { get; set; }
        public string? TargetPlaylistName { get; set; }
        public string? TargetCommentText { get; set; }
        public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    }
}
