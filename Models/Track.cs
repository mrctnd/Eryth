using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eryth.Models.Enums;

namespace Eryth.Models
{

    public class Track
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Url]
        [StringLength(500)]
        public string AudioFileUrl { get; set; } = string.Empty;

        [Url]
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DurationInSeconds { get; set; }

        [Required]
        public Genre Genre { get; set; } = Genre.Other;

        [StringLength(100)]
        public string? SubGenre { get; set; }

        public bool IsExplicit { get; set; } = false;

        [Required]
        public TrackStatus Status { get; set; } = TrackStatus.Draft;

        public long PlayCount { get; set; } = 0;

        public long LikeCount { get; set; } = 0;

        public long CommentCount { get; set; } = 0;

        public bool AllowComments { get; set; } = true;

        public bool AllowDownloads { get; set; } = false;

        [StringLength(100)]
        public string? Copyright { get; set; }

        [StringLength(100)]
        public string? Producer { get; set; }

        [StringLength(100)]
        public string? Composer { get; set; }

        [StringLength(100)]
        public string? Lyricist { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        [Required]
        public Guid ArtistId { get; set; }

        public Guid? AlbumId { get; set; }

        // Navigation properties
        [ForeignKey("ArtistId")]
        public virtual User Artist { get; set; } = null!;

        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
        public virtual ICollection<UserPlayHistory> PlayHistory { get; set; } = new List<UserPlayHistory>();

        // Computed properties
        [NotMapped]
        public bool IsPublished => Status == TrackStatus.Active;

        [NotMapped]
        public TimeSpan Duration => TimeSpan.FromSeconds(DurationInSeconds); [NotMapped]
        public string FormattedDuration => $"{Duration.Minutes:D2}:{Duration.Seconds:D2}";
    }
}
