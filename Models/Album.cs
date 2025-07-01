using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eryth.Models.Enums;

namespace Eryth.Models
{

    public class Album
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
        public Genre PrimaryGenre { get; set; } = Genre.Other;

        public DateTime? ReleaseDate { get; set; }

        [StringLength(100)]
        public string? RecordLabel { get; set; }

        [StringLength(100)]
        public string? Copyright { get; set; }

        public bool IsExplicit { get; set; } = false;

        public long TotalPlayCount { get; set; } = 0;

        public long TotalLikeCount { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        [Required]
        public Guid ArtistId { get; set; }

        // Navigation properties
        [ForeignKey("ArtistId")]
        public virtual User Artist { get; set; } = null!;

        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

        // Computed properties
        [NotMapped]
        public int TrackCount => Tracks?.Count ?? 0;

        [NotMapped]
        public int TotalDurationInSeconds => Tracks?.Sum(t => t.DurationInSeconds) ?? 0;

        [NotMapped]
        public TimeSpan TotalDuration => TimeSpan.FromSeconds(TotalDurationInSeconds);

        [NotMapped]
        public string FormattedTotalDuration =>
            TotalDuration.Hours > 0 ?
            $"{TotalDuration.Hours:D1}:{TotalDuration.Minutes:D2}:{TotalDuration.Seconds:D2}" :
            $"{TotalDuration.Minutes:D2}:{TotalDuration.Seconds:D2}"; [NotMapped]
        public bool IsReleased => ReleaseDate.HasValue && ReleaseDate.Value <= DateTime.UtcNow;
    }
}
