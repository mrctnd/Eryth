using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        [Required]
        public Guid SenderId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        public Guid? ParentMessageId { get; set; }

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; } = null!;

        [ForeignKey("ParentMessageId")]
        public virtual Message? ParentMessage { get; set; }

        // Navigation property for replies
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();

        // Helper methods
        public bool IsUnread => !IsRead;

        public string GetRelativeTime()
        {
            var timeSpan = DateTime.UtcNow - SentAt;
            
            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalMinutes < 1 => "Just now",
                < 1 when timeSpan.TotalMinutes < 60 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 => $"{(int)timeSpan.TotalHours}h ago",
                < 7 => $"{(int)timeSpan.TotalDays}d ago",
                < 30 => $"{(int)(timeSpan.TotalDays / 7)}w ago",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)}mo ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"
            };
        }

        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
            }
        }

        public void SoftDelete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
