using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{

    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public long LikeCount { get; set; } = 0;

        public bool IsEdited { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        [Required]
        public Guid UserId { get; set; }

        public Guid? TrackId { get; set; }

        public Guid? PlaylistId { get; set; }

        public Guid? ParentCommentId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TrackId")]
        public virtual Track? Track { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist? Playlist { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        // Computed properties
        [NotMapped]
        public bool IsReply => ParentCommentId.HasValue;

        [NotMapped]
        public bool HasReplies => Replies?.Any() == true;

        [NotMapped]
        public int ReplyCount => Replies?.Count ?? 0; [NotMapped]
        public bool IsVisible => !IsDeleted;
    }
}
