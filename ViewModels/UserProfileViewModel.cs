using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class UserProfileViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Display(Name = "Avatar")]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Banner")]
        public string? BannerImageUrl { get; set; }

        [Display(Name = "Member Since")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Seen")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "Followers")]
        public int FollowerCount { get; set; }

        [Display(Name = "Following")]
        public int FollowingCount { get; set; }

        [Display(Name = "Tracks")]
        public int TrackCount { get; set; }

        [Display(Name = "Albums")]
        public int AlbumCount { get; set; }

        [Display(Name = "Playlists")]
        public int PlaylistCount { get; set; }
        [Display(Name = "Total Likes Received")]
        public int TotalLikesReceived { get; set; }

        [Display(Name = "Total Likes")]
        public int TotalLikes => TotalLikesReceived;

        [Display(Name = "Total Plays")]
        public long TotalPlays { get; set; }

        public bool IsCurrentUser { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public bool IsBlockedByCurrentUser { get; set; }
        public bool CanMessage { get; set; }
        public bool CanFollow { get; set; }

        public List<UserActivityViewModel> RecentActivity { get; set; } = new();
        public List<TrackViewModel> FeaturedTracks { get; set; } = new();
        public List<AlbumViewModel> FeaturedAlbums { get; set; } = new();
        public List<PlaylistViewModel> FeaturedPlaylists { get; set; } = new();

        public string DisplayNameOrUsername => DisplayName?.Trim() ?? Username;
        public string MemberSince => CreatedAt.ToString("MMMM yyyy");
        public string LastSeenText => GetLastSeenText();
        public int TotalContent => TrackCount + AlbumCount + PlaylistCount;
        public bool HasContent => TotalContent > 0;
        public bool HasBio => !string.IsNullOrWhiteSpace(Bio);

        public static UserProfileViewModel FromUser(User user, Guid currentUserId)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Username = user.Username?.Trim() ?? string.Empty,
                DisplayName = user.DisplayName?.Trim() ?? string.Empty,
                Email = user.Email?.Trim() ?? string.Empty,
                Bio = user.Bio?.Trim(),
                ProfileImageUrl = user.ProfileImageUrl?.Trim(),
                BannerImageUrl = user.BannerImageUrl?.Trim(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                FollowerCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                TrackCount = user.Tracks?.Count ?? 0,
                AlbumCount = user.Albums?.Count ?? 0,
                PlaylistCount = user.Playlists?.Count ?? 0
            };

            if (user.Tracks?.Any() == true)
            {
                viewModel.TotalLikesReceived += user.Tracks.Sum(t => t.Likes?.Count ?? 0);
                viewModel.TotalPlays += user.Tracks.Sum(t => t.PlayCount);
            }

            if (user.Playlists?.Any() == true)
            {
                viewModel.TotalLikesReceived += user.Playlists.Sum(p => p.Likes?.Count ?? 0);
            }

            viewModel.IsCurrentUser = currentUserId == user.Id;
            viewModel.IsFollowedByCurrentUser = user.Followers?.Any(f => f.FollowerId == currentUserId) ?? false;
            viewModel.CanFollow = !viewModel.IsCurrentUser && !viewModel.IsFollowedByCurrentUser;
            viewModel.CanMessage = !viewModel.IsCurrentUser;
            if (user.Tracks?.Any() == true)
            {
                viewModel.FeaturedTracks = user.Tracks
                    .Where(t => t.Status == TrackStatus.Active)
                    .OrderByDescending(t => t.PlayCount)
                    .Take(3)
                    .Select(t => {
                        var trackVM = TrackViewModel.FromTrack(t, true, true, true, true);
                        trackVM.ArtistName = viewModel.DisplayNameOrUsername;
                        return trackVM;
                    })
                    .ToList();
            }
            if (user.Albums?.Any() == true)
            {
                viewModel.FeaturedAlbums = user.Albums
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(3)
                    .Select(a => AlbumViewModel.FromAlbum(a, true, true))
                    .ToList();
            }
            if (user.Playlists?.Any() == true)
            {
                viewModel.FeaturedPlaylists = user.Playlists
                    .Where(p => p.Privacy == PlaylistPrivacy.Public)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Take(3)
                    .Select(p => PlaylistViewModel.FromPlaylist(p, currentUserId))
                    .ToList();
            }

            return viewModel;
        }

        private string GetLastSeenText()
        {
            if (!LastLoginAt.HasValue) return "Never";

            var timeSpan = DateTime.UtcNow - LastLoginAt.Value;
            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 => $"{(int)timeSpan.TotalHours}h ago",
                < 7 => $"{(int)timeSpan.TotalDays}d ago",
                < 30 => $"{(int)(timeSpan.TotalDays / 7)}w ago",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)}mo ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"
            };
        }
    }

    public class UserProfileEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, and hyphens")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Display(Name = "Avatar Image")]
        public IFormFile? AvatarFile { get; set; }

        [Display(Name = "Banner Image")]
        public IFormFile? BannerFile { get; set; }

        public string? CurrentProfileImageUrl { get; set; }
        public string? CurrentBannerImageUrl { get; set; }

        [Display(Name = "Remove Avatar")]
        public bool RemoveAvatar { get; set; }

        [Display(Name = "Remove Banner")]
        public bool RemoveBanner { get; set; }

        public static UserProfileEditViewModel FromUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return new UserProfileEditViewModel
            {
                Id = user.Id,
                Username = user.Username?.Trim() ?? string.Empty,
                DisplayName = user.DisplayName?.Trim(),
                Email = user.Email?.Trim() ?? string.Empty,
                Bio = user.Bio?.Trim(),
                CurrentProfileImageUrl = user.ProfileImageUrl?.Trim(),
                CurrentBannerImageUrl = user.BannerImageUrl?.Trim()
            };
        }
        public void UpdateUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.Username = Username?.Trim() ?? string.Empty;
            user.DisplayName = DisplayName?.Trim() ?? string.Empty;
            user.Email = Email?.Trim() ?? string.Empty;
            user.Bio = Bio?.Trim();
            user.UpdatedAt = DateTime.UtcNow;
        }
    }

    public class UserActivityViewModel
    {
        public int Id { get; set; }
        public UserActivityType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? TargetTitle { get; set; }
        public string? TargetUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public string RelativeTime => GetRelativeTime(CreatedAt);
        public string ActivityIcon => GetActivityIcon();
        public string ActivityColor => GetActivityColor();

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;
            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 1 => $"{(int)timeSpan.TotalHours}h ago",
                < 7 => $"{(int)timeSpan.TotalDays}d ago",
                < 30 => $"{(int)(timeSpan.TotalDays / 7)}w ago",
                < 365 => $"{(int)(timeSpan.TotalDays / 30)}mo ago",
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"
            };
        }

        private string GetActivityIcon()
        {
            return Type switch
            {
                UserActivityType.TrackUploaded => "🎵",
                UserActivityType.AlbumReleased => "💿",
                UserActivityType.PlaylistCreated => "📝",
                UserActivityType.LikedTrack => "❤️",
                UserActivityType.FollowedUser => "👤",
                UserActivityType.CommentAdded => "💬",
                _ => "📌"
            };
        }

        private string GetActivityColor()
        {
            return Type switch
            {
                UserActivityType.TrackUploaded => "text-primary",
                UserActivityType.AlbumReleased => "text-success",
                UserActivityType.PlaylistCreated => "text-info",
                UserActivityType.LikedTrack => "text-danger",
                UserActivityType.FollowedUser => "text-warning",
                UserActivityType.CommentAdded => "text-secondary",
                _ => "text-muted"
            };
        }
    }

    public class UserStatsViewModel
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public int TotalTracks { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalComments { get; set; }

        public long TotalPlays { get; set; }
        public int TotalLikes { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowing { get; set; }

        public long PlaysThisWeek { get; set; }
        public long PlaysThisMonth { get; set; }
        public long PlaysThisYear { get; set; }
        public int NewFollowersThisWeek { get; set; }
        public int NewFollowersThisMonth { get; set; }

        public TrackViewModel? MostPlayedTrack { get; set; }
        public TrackViewModel? MostLikedTrack { get; set; }
        public AlbumViewModel? TopAlbum { get; set; }
        public PlaylistViewModel? TopPlaylist { get; set; }

        public List<CountryPlayStats> TopCountries { get; set; } = new();

        public List<GenreStats> GenreBreakdown { get; set; } = new();

        public double AveragePlayCountPerTrack => TotalTracks > 0 ? (double)TotalPlays / TotalTracks : 0;
        public double AverageLikesPerTrack => TotalTracks > 0 ? (double)TotalLikes / TotalTracks : 0;
        public string TopGenre => GenreBreakdown.OrderByDescending(g => g.TrackCount).FirstOrDefault()?.Genre.ToString() ?? "Unknown";
    }

    public class CountryPlayStats
    {
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public int PlayCount { get; set; }
        public double Percentage { get; set; }
    }

    public class GenreStats
    {
        public Genre Genre { get; set; }
        public int TrackCount { get; set; }
        public int PlayCount { get; set; }
        public double Percentage { get; set; }

        public string GenreName => Genre.ToString();
    }

    public enum UserActivityType
    {
        [Display(Name = "Uploaded a track")]
        TrackUploaded,
        [Display(Name = "Released an album")]
        AlbumReleased,
        [Display(Name = "Created a playlist")]
        PlaylistCreated,
        [Display(Name = "Liked a track")]
        LikedTrack,
        [Display(Name = "Followed a user")]
        FollowedUser,
        [Display(Name = "Added a comment")]
        CommentAdded,
        [Display(Name = "Shared content")]
        SharedContent,
        [Display(Name = "Updated profile")]
        ProfileUpdated
    }
}
