using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    /// ViewModel çalma listesi bilgileri için
    public class PlaylistViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public string? OwnerAvatarUrl { get; set; }
        public PlaylistPrivacy Privacy { get; set; } = PlaylistPrivacy.Public;
        public bool IsCollaborative { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TrackCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string? CoverImageUrl { get; set; }
        public long PlayCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAddTracks { get; set; }
        public bool IsFollowing { get; set; }

        public string FormattedDuration => TotalDuration.TotalHours >= 1
            ? $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes}m"
            : $"{TotalDuration.Minutes}m {TotalDuration.Seconds}s";

        public string RelativeCreatedDate => GetRelativeTime(CreatedAt);
        public string RelativeUpdatedDate => GetRelativeTime(UpdatedAt); public bool IsEmpty => TrackCount == 0;
        public bool IsPublic => Privacy == PlaylistPrivacy.Public;
        public bool IsPrivate => Privacy == PlaylistPrivacy.Private;
        public string Title => Name;
        public string CreatorName => OwnerUsername;

        public static PlaylistViewModel FromPlaylist(Playlist playlist, Guid currentUserId)
        {
            var viewModel = new PlaylistViewModel
            {
                Id = playlist.Id,
                Name = playlist.Title?.Trim() ?? string.Empty,
                Description = playlist.Description?.Trim(),
                OwnerId = playlist.CreatedByUserId,
                OwnerUsername = playlist.CreatedByUser?.Username?.Trim() ?? string.Empty,
                OwnerAvatarUrl = playlist.CreatedByUser?.ProfileImageUrl?.Trim(),
                Privacy = playlist.Privacy,
                IsCollaborative = playlist.IsCollaborative,
                CreatedAt = playlist.CreatedAt,
                UpdatedAt = playlist.UpdatedAt,
                TrackCount = playlist.PlaylistTracks?.Count ?? 0,
                TotalDuration = playlist.TotalDuration,
                PlayCount = playlist.PlayCount,
                LikeCount = playlist.Likes?.Count ?? 0
            };

            viewModel.CoverImageUrl = playlist.CoverImageUrl?.Trim() ??
                playlist.PlaylistTracks?.Where(pt => pt.Track?.CoverImageUrl != null)
                                       .OrderBy(pt => pt.OrderIndex)
                                       .FirstOrDefault()?.Track?.CoverImageUrl?.Trim();

            var isOwner = currentUserId == playlist.CreatedByUserId;
            viewModel.CanEdit = isOwner || (playlist.IsCollaborative && currentUserId != Guid.Empty);
            viewModel.CanDelete = isOwner;
            viewModel.CanAddTracks = isOwner || playlist.IsCollaborative;

            var canView = isOwner ||
                         playlist.Privacy == PlaylistPrivacy.Public ||
                         playlist.Privacy == PlaylistPrivacy.UnlistedLink;

            if (canView)
            {
                viewModel.IsLikedByCurrentUser = playlist.Likes?.Any(l => l.UserId == currentUserId) ?? false;
            }

            return viewModel;
        }

        public static List<PlaylistViewModel> FromPlaylists(IEnumerable<Playlist> playlists, Guid currentUserId)
        {
            return playlists.Select(p => FromPlaylist(p, currentUserId)).ToList();
        }

        private static string GetRelativeTime(DateTime dateTime)
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
    } 
    
    public class CreatePlaylistViewModel
    {
        [Required(ErrorMessage = "Playlist name is required")]
        [StringLength(200, ErrorMessage = "Playlist name cannot exceed 200 characters")]
        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Privacy")]
        public PlaylistPrivacy Privacy { get; set; } = PlaylistPrivacy.Public;

        [Display(Name = "Allow Collaboration")]
        public bool IsCollaborative { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "Cover Image URL")]
        public string? CoverImageUrl { get; set; }
        public Playlist ToPlaylist(Guid userId)
        {
            return new Playlist
            {
                Title = Name?.Trim() ?? string.Empty,
                Description = Description?.Trim(),
                Privacy = Privacy,
                IsCollaborative = IsCollaborative,
                CoverImageUrl = CoverImageUrl,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    public class EditPlaylistViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Playlist name is required")]
        [StringLength(200, ErrorMessage = "Playlist name cannot exceed 200 characters")]
        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Privacy")]
        public PlaylistPrivacy Privacy { get; set; } = PlaylistPrivacy.Public;

        [Display(Name = "Allow Collaboration")]
        public bool IsCollaborative { get; set; }

        public static EditPlaylistViewModel FromPlaylist(Playlist playlist)
        {
            return new EditPlaylistViewModel
            {
                Id = playlist.Id,
                Name = playlist.Title?.Trim() ?? string.Empty,
                Description = playlist.Description?.Trim(),
                Privacy = playlist.Privacy,
                IsCollaborative = playlist.IsCollaborative
            };
        }

        public void UpdatePlaylist(Playlist playlist)
        {
            playlist.Title = Name?.Trim() ?? string.Empty;
            playlist.Description = Description?.Trim();
            playlist.Privacy = Privacy;
            playlist.IsCollaborative = IsCollaborative;
            playlist.UpdatedAt = DateTime.UtcNow;
        }
    }

    public class PlaylistCardViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public int TrackCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string? CoverImageUrl { get; set; }
        public PlaylistPrivacy Privacy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanEdit { get; set; }

        public string FormattedDuration => TotalDuration.TotalHours >= 1
            ? $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes}m"
            : $"{TotalDuration.Minutes}m";

        public string RelativeUpdatedDate => GetRelativeTime(UpdatedAt);
        public bool IsEmpty => TrackCount == 0;
        public string TrackCountText => TrackCount == 1 ? "1 track" : $"{TrackCount} tracks";

        public static PlaylistCardViewModel FromPlaylist(Playlist playlist, Guid currentUserId)
        {
            return new PlaylistCardViewModel
            {
                Id = playlist.Id,
                Name = playlist.Title?.Trim() ?? string.Empty,
                Description = playlist.Description?.Trim(),
                OwnerUsername = playlist.CreatedByUser?.Username?.Trim() ?? string.Empty,
                TrackCount = playlist.PlaylistTracks?.Count ?? 0,
                TotalDuration = playlist.TotalDuration,
                CoverImageUrl = playlist.CoverImageUrl?.Trim() ??
                    playlist.PlaylistTracks?.Where(pt => pt.Track?.CoverImageUrl != null)
                                           .OrderBy(pt => pt.OrderIndex)
                                           .FirstOrDefault()?.Track?.CoverImageUrl?.Trim(),
                Privacy = playlist.Privacy,
                UpdatedAt = playlist.UpdatedAt,
                CanEdit = currentUserId == playlist.CreatedByUserId
            };
        }

        public static List<PlaylistCardViewModel> FromPlaylists(IEnumerable<Playlist> playlists, Guid currentUserId)
        {
            return playlists.Select(p => FromPlaylist(p, currentUserId)).ToList();
        }

        private static string GetRelativeTime(DateTime dateTime)
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
    }
}
