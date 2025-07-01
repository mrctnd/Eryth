using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eryth.Models.Enums;

namespace Eryth.Models
{

    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        [Url]
        [StringLength(255)]
        public string? ProfileImageUrl { get; set; }

        [Url]
        [StringLength(255)]
        public string? BannerImageUrl { get; set; }
        [StringLength(100)]
        public string? Location { get; set; }

        [Url]
        [StringLength(255)]
        public string? Website { get; set; }

        // Personal Information
        public int? BirthYear { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        // Settings
        [StringLength(10)]
        public string Theme { get; set; } = "dark";

        [StringLength(5)]
        public string Language { get; set; } = "tr";

        public bool IsPrivate { get; set; } = false;

        public bool EmailNotifications { get; set; } = true;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        [Required]
        public AccountStatus Status { get; set; } = AccountStatus.PendingVerification;

        [Required]
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Free;

        public bool IsEmailVerified { get; set; } = false;

        public bool IsTwoFactorEnabled { get; set; } = false;

        public string? TwoFactorSecret { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public string? LastLoginIpAddress { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? AccountLockedUntil { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<UserPlayHistory> PlayHistory { get; set; } = new List<UserPlayHistory>();

        // Computed properties
        [NotMapped]
        public bool IsAccountLocked => AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow; [NotMapped]
        public bool CanLogin => Status == AccountStatus.Active && IsEmailVerified && !IsAccountLocked;
    }
}
