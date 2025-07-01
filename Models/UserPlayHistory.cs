using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{
    public class UserPlayHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue)]
        public int PlayDurationInSeconds { get; set; } = 0;

        [Range(0, 100)]
        public double CompletionPercentage { get; set; } = 0.0;

        public bool IsCompleted { get; set; } = false;

        public bool IsSkipped { get; set; } = false;

        [StringLength(50)]
        public string? DeviceType { get; set; }

        [StringLength(200)]
        public string? DeviceInfo { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid TrackId { get; set; }

        public Guid? PlaylistId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TrackId")]
        public virtual Track Track { get; set; } = null!;

        [ForeignKey("PlaylistId")]
        public virtual Playlist? Playlist { get; set; }

        [NotMapped]
        public TimeSpan PlayDuration => TimeSpan.FromSeconds(PlayDurationInSeconds);

        [NotMapped]
        public string FormattedPlayDuration => $"{PlayDuration.Minutes:D2}:{PlayDuration.Seconds:D2}";

        [NotMapped]
        public bool IsValidPlay => PlayDurationInSeconds >= 30 || CompletionPercentage >= 50.0;

        [NotMapped]
        public string PlaySource => PlaylistId.HasValue ? "Playlist" : "Direct";
    }
}
