using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{
    public class PlaylistFollower
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid PlaylistId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PlaylistId")]
        public virtual Playlist Playlist { get; set; } = null!;
    }
}
