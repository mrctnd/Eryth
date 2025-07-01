using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "Notification Type")]
        public NotificationType Type { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 500 characters")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Read")]
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? TriggeredByUserId { get; set; }
        public Guid? RelatedTrackId { get; set; }
        public Guid? RelatedPlaylistId { get; set; }
        public Guid? RelatedCommentId { get; set; }
        public Guid? RelatedMessageId { get; set; }

        [Display(Name = "Triggered By")]
        public string? TriggeredByUserName { get; set; }

        public string? TriggeredByUserProfileImage { get; set; }

        [Display(Name = "Related Track")]
        public string? RelatedTrackTitle { get; set; }

        [Display(Name = "Related Playlist")]
        public string? RelatedPlaylistTitle { get; set; }

        public string? RelatedCommentContent { get; set; }

        public string TypeDisplayName => Type switch
        {
            NotificationType.Follow => "Yeni Takipçi",
            NotificationType.Like => "İçerik Beğenildi",
            NotificationType.Comment => "Yeni Yorum",
            NotificationType.PlaylistShare => "Çalma Listesi Paylaşıldı",
            NotificationType.NewRelease => "Yeni Yayın",
            NotificationType.SystemNotification => "Sistem Bildirimi",
            NotificationType.SecurityAlert => "Güvenlik Uyarısı",
            NotificationType.Message => "Yeni Mesaj",
            _ => Type.ToString()
        };

        public string Icon => Type switch
        {
            NotificationType.Follow => "👤",
            NotificationType.Like => "❤️",
            NotificationType.Comment => "💬",
            NotificationType.PlaylistShare => "🔗",
            NotificationType.NewRelease => "🎵",
            NotificationType.SystemNotification => "🔔",
            NotificationType.SecurityAlert => "⚠️",
            NotificationType.Message => "✉️",
            _ => "🔔"
        };

        public string Priority => Type switch
        {
            NotificationType.Follow => "high",
            NotificationType.Comment => "high",
            NotificationType.SecurityAlert => "high",
            NotificationType.Like => "medium",
            NotificationType.PlaylistShare => "medium",
            NotificationType.NewRelease => "low",
            NotificationType.SystemNotification => "low",
            _ => "low"
        };

        public string Title => TypeDisplayName;
        public string RelativeCreatedTime => GetRelativeTimeDisplay();
        public string ActionUrl => GetActionUrl();

        public string? StoredActionUrl { get; set; }

        public string GetRelativeTimeDisplay()
        {
            var timeSpan = DateTime.UtcNow - CreatedAt;

            return timeSpan.TotalMinutes switch
            {
                < 1 => "Şimdi",
                < 60 => $"{(int)timeSpan.TotalMinutes} dk önce",
                < 1440 => $"{(int)timeSpan.TotalHours} sa önce",
                < 10080 => $"{(int)timeSpan.TotalDays} gün önce",
                _ => CreatedAt.ToString("dd MMM yyyy")
            };
        }

        public string GetActionUrl()
        {
            if (!string.IsNullOrEmpty(StoredActionUrl))
                return StoredActionUrl;

            return Type switch
            {
                NotificationType.Follow when TriggeredByUserId.HasValue =>
                    $"/profile/{TriggeredByUserId}",
                NotificationType.Like when RelatedTrackId.HasValue =>
                    $"/track/{RelatedTrackId}",
                NotificationType.Like when RelatedPlaylistId.HasValue =>
                    $"/playlist/{RelatedPlaylistId}",
                NotificationType.Comment when RelatedTrackId.HasValue =>
                    $"/track/{RelatedTrackId}#comments",
                NotificationType.Comment when RelatedPlaylistId.HasValue =>
                    $"/playlist/{RelatedPlaylistId}#comments",
                NotificationType.PlaylistShare when RelatedPlaylistId.HasValue =>
                    $"/playlist/{RelatedPlaylistId}",
                NotificationType.NewRelease when RelatedTrackId.HasValue =>
                    $"/track/{RelatedTrackId}",
                NotificationType.NewRelease when TriggeredByUserId.HasValue =>
                    $"/profile/{TriggeredByUserId}",
                NotificationType.Message when RelatedMessageId.HasValue =>
                    $"/Message/Conversation/{RelatedMessageId}",
                NotificationType.Message =>
                    "/Message",
                _ => "/notifications"
            };
        }

        public static NotificationViewModel FromNotification(Notification notification)
        {
            return new NotificationViewModel
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Message = notification.Message ?? string.Empty,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                TriggeredByUserId = notification.TriggeredByUserId,
                RelatedTrackId = notification.RelatedTrackId,
                RelatedPlaylistId = notification.RelatedPlaylistId,
                RelatedCommentId = notification.RelatedCommentId,
                RelatedMessageId = notification.RelatedMessageId,
                StoredActionUrl = notification.ActionUrl,
                TriggeredByUserName = notification.TriggeredByUser?.DisplayName,
                TriggeredByUserProfileImage = notification.TriggeredByUser?.ProfileImageUrl,
                RelatedTrackTitle = notification.RelatedTrack?.Title,
                RelatedPlaylistTitle = notification.RelatedPlaylist?.Title,
                RelatedCommentContent = notification.RelatedComment?.Content
            };
        }

        public Notification ToNotification()
        {
            return new Notification
            {
                Id = Id,
                UserId = UserId,
                Type = Type,
                Message = Message.Trim(),
                IsRead = IsRead,
                TriggeredByUserId = TriggeredByUserId,                RelatedTrackId = RelatedTrackId,
                RelatedPlaylistId = RelatedPlaylistId,
                RelatedCommentId = RelatedCommentId,
                RelatedMessageId = RelatedMessageId,
                ActionUrl = StoredActionUrl
            };
        }
    }

    public class NotificationListViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; } = new();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public bool ShowUnreadOnly { get; set; }
        public NotificationType? FilterType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public Dictionary<string, List<NotificationViewModel>> GroupedNotifications { get; set; } = new();

        public void GroupByDate()
        {
            GroupedNotifications = Notifications
                .GroupBy(n => GetDateGroup(n.CreatedAt))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static string GetDateGroup(DateTime date)
        {
            var now = DateTime.UtcNow;
            var daysDiff = (now.Date - date.Date).Days;

            return daysDiff switch
            {
                0 => "Today",
                1 => "Yesterday",
                <= 7 => "This Week",
                <= 30 => "This Month",
                _ => "Older"
            };
        }

        public bool HasNotifications => Notifications.Any();
        public bool HasUnreadNotifications => UnreadCount > 0;

        public List<NotificationViewModel> GetPriorityNotifications()
        {
            return Notifications
                .Where(n => !n.IsRead && n.Priority == "high")
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();
        }
    }

    public class NotificationPreferencesViewModel
    {
        public Guid UserId { get; set; }

        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; } = true;

        [Display(Name = "Push Notifications")]
        public bool PushNotifications { get; set; } = true;

        [Display(Name = "SMS Notifications")]
        public bool SmsNotifications { get; set; } = false;

        // Notification type preferences
        [Display(Name = "New Followers")]
        public bool NotifyNewFollowers { get; set; } = true;

        [Display(Name = "Track Likes")]
        public bool NotifyTrackLikes { get; set; } = true;

        [Display(Name = "Playlist Likes")]
        public bool NotifyPlaylistLikes { get; set; } = true;

        [Display(Name = "Comments")]
        public bool NotifyComments { get; set; } = true;

        [Display(Name = "Playlist Additions")]
        public bool NotifyPlaylistAdditions { get; set; } = true;

        [Display(Name = "New Content from Followed Artists")]
        public bool NotifyNewContent { get; set; } = true;

        // Frequency settings
        [Display(Name = "Notification Frequency")]
        public string NotificationFrequency { get; set; } = "immediate";

        [Display(Name = "Quiet Hours Start")]
        [DataType(DataType.Time)]
        public TimeSpan? QuietHoursStart { get; set; }

        [Display(Name = "Quiet Hours End")]
        [DataType(DataType.Time)]
        public TimeSpan? QuietHoursEnd { get; set; }

        public bool HasQuietHours => QuietHoursStart.HasValue && QuietHoursEnd.HasValue;
    }
}
