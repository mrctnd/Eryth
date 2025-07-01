using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{

    public class Like
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public Guid UserId { get; set; }

        public Guid? TrackId { get; set; }

        public Guid? PlaylistId { get; set; }

        public Guid? CommentId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TrackId")]
        public virtual Track? Track { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist? Playlist { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }

        // Computed properties
        [NotMapped]
        public string LikedEntityType
        {
            get
            {
                if (TrackId.HasValue) return "Track";
                if (PlaylistId.HasValue) return "Playlist";
                if (CommentId.HasValue) return "Comment";
                return "Unknown";
            }
        }

        [NotMapped]
        public Guid? LikedEntityId
        {
            get
            {
                if (TrackId.HasValue) return TrackId;
                if (PlaylistId.HasValue) return PlaylistId;
                if (CommentId.HasValue) return CommentId;
                return null;
            }
        }
    }
}
