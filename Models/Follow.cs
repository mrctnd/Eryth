using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eryth.Models
{

    public class Follow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public Guid FollowerId { get; set; }

        [Required]
        public Guid FollowingId { get; set; }

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; } = null!;

        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; } = null!;

        // Computed properties
        [NotMapped] public bool IsSelfFollow => FollowerId == FollowingId;
    }
}
