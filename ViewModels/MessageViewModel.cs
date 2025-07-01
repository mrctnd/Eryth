using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class MessageViewModel
    {
        public Guid Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public Guid ReceiverId { get; set; }
        public string ReceiverUsername { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }

        public string RelativeSentTime => GetRelativeTime(SentAt);
        public string FormattedSentTime => SentAt.ToString("dd MMM yyyy 'saat' HH:mm");
        public bool IsUnread => !IsRead;
        public string TruncatedContent => Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content;

        private static string GetRelativeTime(DateTime dateTime)
        {
            var now = DateTime.UtcNow;
            
            // Entity Framework'den gelen datetime'lar genellikle Unspecified olur
            // Ama bizim SentAt değerlerimiz UTC olarak kaydediliyor, bu yüzden UTC olarak treat edelim
            var messageTime = dateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
                
            var timeSpan = now - messageTime;
              return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalMinutes < 1 => "Şimdi",
                < 1 when timeSpan.TotalMinutes < 60 => $"{(int)timeSpan.TotalMinutes}dk önce",
                < 1 => $"{(int)timeSpan.TotalHours}s önce",
                < 7 => $"{(int)timeSpan.TotalDays}g önce",
                < 30 => $"{(int)(timeSpan.TotalDays / 7)}h önce",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)}ay önce",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y önce"
            };
        }
    }

    /// ViewModel yeni mesaj oluşturma için
    public class CreateMessageViewModel
    {
        [Required(ErrorMessage = "Alıcı gereklidir")]
        public string RecipientUsername { get; set; } = string.Empty;

        // This will be populated when the form is submitted
        public Guid ReceiverId { get; set; }

        [Required(ErrorMessage = "Konu gereklidir")]
        [StringLength(200, ErrorMessage = "Konu 200 karakteri geçemez")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesaj içeriği gereklidir")]
        [StringLength(5000, ErrorMessage = "Mesaj içeriği 5000 karakteri geçemez")]
        public string Content { get; set; } = string.Empty;
    }

    public class MessageThreadViewModel
    {
        public Guid Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public Guid OtherUserId { get; set; }
        public string OtherUsername { get; set; } = string.Empty;
        public string? OtherUserAvatarUrl { get; set; }
        public DateTime LastMessageAt { get; set; }
        public string LastMessageContent { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public bool HasUnreadMessages => UnreadCount > 0;

        public string RelativeLastMessageTime => GetRelativeTime(LastMessageAt);

        private static string GetRelativeTime(DateTime dateTime)
        {
            var now = DateTime.UtcNow;
            
            // Entity Framework'den gelen datetime'lar genellikle Unspecified olur
            // Ama bizim SentAt değerlerimiz UTC olarak kaydediliyor, bu yüzden UTC olarak treat edelim
            var messageTime = dateTime.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
                
            var timeSpan = now - messageTime;
            
            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalMinutes < 1 => "Just now",
                < 1 when timeSpan.TotalMinutes < 60 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 => $"{(int)timeSpan.TotalHours}h ago",
                < 7 => $"{(int)timeSpan.TotalDays}d ago",
                < 30 => $"{(int)(timeSpan.TotalDays / 7)}w ago",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)}mo ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"
            };
        }
    }

    public class UserSearchViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }

    public class MessageConversationViewModel
    {
        public Guid Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public Guid CurrentUserId { get; set; }
        public Guid OtherUserId { get; set; }
        public string OtherUsername { get; set; } = string.Empty;
        public string OtherDisplayName { get; set; } = string.Empty;
        public string? OtherUserAvatarUrl { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new();
        public DateTime LastMessageAt { get; set; }
        public bool CanReply { get; set; } = true;
    }
}
