using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eryth.Models.Enums;

namespace Eryth.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Message { get; set; }

        public bool IsRead { get; set; } = false;

        [Url]
        [StringLength(500)]
        public string? ActionUrl { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public string? Metadata { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public Guid? TriggeredByUserId { get; set; }

        public Guid? RelatedTrackId { get; set; }

        public Guid? RelatedPlaylistId { get; set; }        public Guid? RelatedCommentId { get; set; }

        public Guid? RelatedMessageId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TriggeredByUserId")]
        public virtual User? TriggeredByUser { get; set; }

        [ForeignKey("RelatedTrackId")]
        public virtual Track? RelatedTrack { get; set; }

        [ForeignKey("RelatedPlaylistId")]
        public virtual Playlist? RelatedPlaylist { get; set; }        [ForeignKey("RelatedCommentId")]
        public virtual Comment? RelatedComment { get; set; }

        [ForeignKey("RelatedMessageId")]
        public virtual Message? RelatedMessage { get; set; }

        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

        [NotMapped]
        public bool IsActive => !IsExpired;

        [NotMapped]
        public string RelativeCreatedTime
        {
            get
            {
                var timeAgo = DateTime.UtcNow - CreatedAt;
                if (timeAgo.TotalMinutes < 1) return "Just now";
                if (timeAgo.TotalHours < 1) return $"{(int)timeAgo.TotalMinutes}m ago";
                if (timeAgo.TotalDays < 1) return $"{(int)timeAgo.TotalHours}h ago";
                if (timeAgo.TotalDays < 7) return $"{(int)timeAgo.TotalDays}d ago";
                return CreatedAt.ToString("MMM dd");
            }
        }
    }
}
