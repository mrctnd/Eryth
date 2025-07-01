using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{
    public class ArtistFollower
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ArtistId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ArtistId")]
        public virtual User Artist { get; set; } = null!;

        // Computed properties
        [NotMapped] 
        public bool IsSelfFollow => UserId == ArtistId;
    }
}
