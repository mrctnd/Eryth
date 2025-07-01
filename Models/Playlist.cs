using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eryth.Models.Enums;

namespace Eryth.Models
{

    public class Playlist
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Url]
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [Required]
        public PlaylistPrivacy Privacy { get; set; } = PlaylistPrivacy.Public;

        public bool IsCollaborative { get; set; } = false;

        public long PlayCount { get; set; } = 0;

        public long LikeCount { get; set; } = 0;

        public long FollowCount { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        [Required]
        public Guid CreatedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Computed properties
        [NotMapped]
        public int TrackCount => PlaylistTracks?.Count ?? 0;

        [NotMapped]
        public int TotalDurationInSeconds => PlaylistTracks?.Sum(pt => pt.Track?.DurationInSeconds ?? 0) ?? 0;

        [NotMapped]
        public TimeSpan TotalDuration => TimeSpan.FromSeconds(TotalDurationInSeconds);

        [NotMapped]
        public string FormattedTotalDuration =>
            TotalDuration.Hours > 0 ?
            $"{TotalDuration.Hours:D1}:{TotalDuration.Minutes:D2}:{TotalDuration.Seconds:D2}" :
            $"{TotalDuration.Minutes:D2}:{TotalDuration.Seconds:D2}";

        [NotMapped]
        public bool IsPublic => Privacy == PlaylistPrivacy.Public;

        [NotMapped] public bool IsVisible => Privacy != PlaylistPrivacy.Private;
    }
}
