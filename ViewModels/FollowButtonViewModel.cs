using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class FollowButtonViewModel
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public string EntityType { get; set; } = string.Empty; 

        public bool IsFollowed { get; set; } = false;

        public int? FollowerCount { get; set; }

        public bool ShowCount { get; set; } = false;

        public string CssClasses { get; set; } = string.Empty;

        public static FollowButtonViewModel ForUser(Guid userId, bool isFollowed, int? followerCount = null, bool showCount = false)
        {
            return new FollowButtonViewModel
            {
                EntityId = userId,
                EntityType = "user",
                IsFollowed = isFollowed,
                FollowerCount = followerCount,
                ShowCount = showCount
            };
        }

        public static FollowButtonViewModel ForArtist(Guid artistId, bool isFollowed, int? followerCount = null, bool showCount = false)
        {
            return new FollowButtonViewModel
            {
                EntityId = artistId,
                EntityType = "artist",
                IsFollowed = isFollowed,
                FollowerCount = followerCount,
                ShowCount = showCount
            };
        }

        public static FollowButtonViewModel ForPlaylist(Guid playlistId, bool isFollowed, int? followerCount = null, bool showCount = false)
        {
            return new FollowButtonViewModel
            {
                EntityId = playlistId,
                EntityType = "playlist",
                IsFollowed = isFollowed,
                FollowerCount = followerCount,
                ShowCount = showCount
            };
        }
    }
}
