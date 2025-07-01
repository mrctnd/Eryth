using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public bool IsCurrentUser { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int TrackCount { get; set; }
        public int AlbumCount { get; set; }
        public int PlaylistCount { get; set; }
        public long TotalPlays { get; set; }
        
        public List<TrackViewModel> FeaturedTracks { get; set; } = new();
        
        public UserRole Role { get; set; } = UserRole.User;
        public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
        public DateTime CreatedAt { get; set; }
        
        public string DisplayNameOrUsername => string.IsNullOrWhiteSpace(DisplayName) ? Username : DisplayName;

        public static UserViewModel FromUser(User user, Guid currentUserId)
        {
            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username?.Trim() ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName?.Trim(),
                Email = user.Email?.Trim(),
                Bio = user.Bio?.Trim(),
                ProfileImageUrl = user.ProfileImageUrl?.Trim(),
                BannerImageUrl = user.BannerImageUrl?.Trim(),
                JoinedDate = user.CreatedAt,
                IsFollowedByCurrentUser = user.Followers?.Any(f => f.FollowerId == currentUserId) ?? false,
                IsCurrentUser = user.Id == currentUserId,
                FollowerCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                TrackCount = user.Tracks?.Count ?? 0,
                AlbumCount = user.Albums?.Count ?? 0,
                PlaylistCount = user.Playlists?.Count ?? 0,                TotalPlays = user.Tracks?.Sum(t => t.PlayCount) ?? 0,
                Role = user.Role,
                AccountStatus = user.Status,
                CreatedAt = user.CreatedAt
            };

            if (user.Tracks?.Any() == true)
            {
                userViewModel.FeaturedTracks = user.Tracks
                    .Where(t => t.Status == TrackStatus.Active)
                    .OrderByDescending(t => t.PlayCount)
                    .Take(3)
                    .Select(t => TrackViewModel.FromTrack(t, false, false, false, false))
                    .ToList();
            }

            return userViewModel;
        }

        public static List<UserViewModel> FromUsers(IEnumerable<User> users, Guid currentUserId)
        {
            return users.Select(u => FromUser(u, currentUserId)).ToList();
        }
    }
}
