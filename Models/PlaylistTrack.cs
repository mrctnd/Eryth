using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{
    public class PlaylistTrack
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Range(0, int.MaxValue)]
        public int OrderIndex { get; set; } = 0;

        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid PlaylistId { get; set; }

        [Required]
        public Guid TrackId { get; set; }

        [Required]
        public Guid AddedByUserId { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist Playlist { get; set; } = null!;

        [ForeignKey("TrackId")]
        public virtual Track Track { get; set; } = null!;

        [ForeignKey("AddedByUserId")]
        public virtual User AddedByUser { get; set; } = null!;

        [NotMapped]
        public string RelativeAddedTime
        {
            get
            {
                var timeAgo = DateTime.UtcNow - AddedAt;
                if (timeAgo.TotalMinutes < 1) return "Just now";
                if (timeAgo.TotalHours < 1) return $"{(int)timeAgo.TotalMinutes}m ago";
                if (timeAgo.TotalDays < 1) return $"{(int)timeAgo.TotalHours}h ago";
                if (timeAgo.TotalDays < 7) return $"{(int)timeAgo.TotalDays}d ago";
                return AddedAt.ToString("MMM dd");
            }
        }

        [NotMapped]
        public bool IsFirstTrack => OrderIndex == 0;
    }
}
