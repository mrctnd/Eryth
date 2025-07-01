using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eryth.Models.Enums;

namespace Eryth.Models
{
    public class Report
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // "User", "Track", "Playlist", "Comment"

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty; // "Spam", "Harassment", "Copyright", etc.

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Reviewed", "Resolved", "Dismissed"

        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High"

        [StringLength(1000)]
        public string? ModeratorNotes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Foreign Keys
        [Required]
        public Guid ReporterId { get; set; } // User who made the report

        public Guid? ReportedUserId { get; set; } // User being reported (if applicable)

        public Guid? ModeratorId { get; set; } // Admin/Moderator who handled the report

        // Content being reported (only one should be set)
        public Guid? TrackId { get; set; }
        public Guid? PlaylistId { get; set; }
        public Guid? CommentId { get; set; }

        // Navigation properties
        [ForeignKey("ReporterId")]
        public virtual User Reporter { get; set; } = null!;

        [ForeignKey("ReportedUserId")]
        public virtual User? ReportedUser { get; set; }

        [ForeignKey("ModeratorId")]
        public virtual User? Moderator { get; set; }

        [ForeignKey("TrackId")]
        public virtual Track? Track { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist? Playlist { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsPending => Status == "Pending";

        [NotMapped]
        public bool IsResolved => Status == "Resolved";

        [NotMapped]
        public bool IsDismissed => Status == "Dismissed";

        [NotMapped]
        public bool IsHighPriority => Priority == "High";

        [NotMapped]
        public string ContentTitle
        {
            get
            {
                return Type switch
                {
                    "Track" => Track?.Title ?? "Unknown Track",
                    "Playlist" => Playlist?.Title ?? "Unknown Playlist",
                    "Comment" => Comment?.Content?.Substring(0, Math.Min(50, Comment.Content.Length)) + "..." ?? "Unknown Comment",
                    "User" => ReportedUser?.Username ?? "Unknown User",
                    _ => "Unknown Content"
                };
            }
        }

        [NotMapped]
        public string RelativeCreatedTime
        {
            get
            {
                var timeAgo = DateTime.UtcNow - CreatedAt;
                return timeAgo.TotalDays >= 1 ? $"{(int)timeAgo.TotalDays} day(s) ago" :
                       timeAgo.TotalHours >= 1 ? $"{(int)timeAgo.TotalHours} hour(s) ago" :
                       $"{(int)timeAgo.TotalMinutes} minute(s) ago";
            }
        }

        [NotMapped]
        public bool HasBeenReviewed => ReviewedAt.HasValue;

        [NotMapped]
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Pending" => "orange",
                    "Reviewed" => "blue",
                    "Resolved" => "green",
                    "Dismissed" => "gray",
                    _ => "gray"
                };
            }
        }

        [NotMapped]
        public string PriorityColor
        {
            get
            {
                return Priority switch
                {
                    "High" => "red",
                    "Medium" => "yellow",
                    "Low" => "green",
                    _ => "gray"
                };
            }
        }
    }
}
