using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class PlaylistDetailsViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Playlist Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Privacy")]
        public PlaylistPrivacy Privacy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Total Duration")]
        public TimeSpan TotalDuration { get; set; }

        [Display(Name = "Track Count")]
        public int TrackCount { get; set; }

        [Display(Name = "Total Plays")]
        public int TotalPlays { get; set; }
        [Display(Name = "Like Count")]
        public int LikeCount { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Owner")]
        public string OwnerUsername { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }

        [Display(Name = "Owner Avatar")]
        public string? OwnerAvatarUrl { get; set; }

        public List<PlaylistTrackViewModel> Tracks { get; set; } = new();

        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAddTracks { get; set; }
        public bool CanRemoveTracks { get; set; }
        public bool CanReorder { get; set; }

        public bool IsLikedByCurrentUser { get; set; }
        public bool IsFollowingOwner { get; set; }

        [Display(Name = "Share URL")]
        public string ShareUrl { get; set; } = string.Empty;

        [Display(Name = "Embed Code")]
        public string EmbedCode { get; set; } = string.Empty;

        public string FormattedDuration => FormatDuration(TotalDuration);
        public string RelativeCreatedDate => GetRelativeTime(CreatedAt);
        public string RelativeUpdatedDate => GetRelativeTime(UpdatedAt);
        public bool IsPublic => Privacy == PlaylistPrivacy.Public;
        public bool IsEmpty => TrackCount == 0;

        public static PlaylistDetailsViewModel FromPlaylist(Playlist playlist, Guid currentUserId)
        {
            if (playlist == null) throw new ArgumentNullException(nameof(playlist));            var viewModel = new PlaylistDetailsViewModel
            {
                Id = playlist.Id,
                Name = playlist.Title?.Trim() ?? string.Empty,
                Description = playlist.Description?.Trim(),
                Privacy = playlist.Privacy,
                CreatedAt = playlist.CreatedAt,
                UpdatedAt = playlist.UpdatedAt,
                CoverImageUrl = playlist.CoverImageUrl?.Trim(),
                OwnerId = playlist.CreatedByUserId,
                OwnerUsername = playlist.CreatedByUser?.Username?.Trim() ?? string.Empty,
                OwnerAvatarUrl = playlist.CreatedByUser?.ProfileImageUrl?.Trim(),
                TrackCount = playlist.PlaylistTracks?.Count ?? 0,
                LikeCount = playlist.Likes?.Count ?? 0,
                ShareUrl = $"/playlist/{playlist.Id}",
                EmbedCode = $"<iframe src=\"/embed/playlist/{playlist.Id}\" width=\"400\" height=\"600\"></iframe>"
            };

            if (playlist.PlaylistTracks?.Any() == true)
            {
                viewModel.TotalDuration = TimeSpan.FromSeconds(
                    playlist.PlaylistTracks.Where(pt => pt.Track != null)
                                         .Sum(pt => pt.Track!.DurationInSeconds)
                );

                viewModel.TotalPlays = (int)playlist.PlaylistTracks.Where(pt => pt.Track != null)
                                                            .Sum(pt => pt.Track!.PlayCount);

                viewModel.Tracks = playlist.PlaylistTracks
                    .Where(pt => pt.Track != null)
                    .OrderBy(pt => pt.OrderIndex)
                    .Select(pt => PlaylistTrackViewModel.FromPlaylistTrack(pt, currentUserId))
                    .ToList();
            }

            var isOwner = currentUserId == playlist.CreatedByUserId;
            viewModel.CanEdit = isOwner;
            viewModel.CanDelete = isOwner;
            viewModel.CanAddTracks = isOwner || playlist.IsCollaborative;
            viewModel.CanRemoveTracks = isOwner;
            viewModel.CanReorder = isOwner;

            viewModel.IsLikedByCurrentUser = playlist.Likes?.Any(l => l.UserId == currentUserId) ?? false;
            viewModel.IsFollowingOwner = playlist.CreatedByUser?.Followers?.Any(f => f.FollowerId == currentUserId) ?? false;

            return viewModel;
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            return $"{duration.Minutes}m {duration.Seconds}s";
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

    public class PlaylistTrackViewModel
    {
        public Guid Id { get; set; }
        public Guid PlaylistId { get; set; }
        public Guid TrackId { get; set; }
        public int OrderIndex { get; set; }
        public DateTime AddedAt { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Artist")]
        public string ArtistName { get; set; } = string.Empty;

        [Display(Name = "Album")]
        public string? AlbumName { get; set; }

        [Display(Name = "Duration")]
        public int DurationInSeconds { get; set; }

        [Display(Name = "Genre")]
        public Genre Genre { get; set; }

        [Display(Name = "Cover Art")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Audio File")]
        public string AudioFileUrl { get; set; } = string.Empty;

        public bool CanRemove { get; set; }
        public bool CanReorder { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        public string FormattedDuration => TimeSpan.FromSeconds(DurationInSeconds).ToString(@"mm\:ss");
        public string RelativeAddedDate => GetRelativeTime(AddedAt);

        public static PlaylistTrackViewModel FromPlaylistTrack(PlaylistTrack playlistTrack, Guid currentUserId)
        {
            if (playlistTrack?.Track == null) throw new ArgumentNullException(nameof(playlistTrack));

            return new PlaylistTrackViewModel
            {
                Id = playlistTrack.Id,
                PlaylistId = playlistTrack.PlaylistId,
                TrackId = playlistTrack.TrackId,
                OrderIndex = playlistTrack.OrderIndex,
                AddedAt = playlistTrack.AddedAt,
                Title = playlistTrack.Track.Title?.Trim() ?? string.Empty,
                ArtistName = playlistTrack.Track.Artist?.Username?.Trim() ?? string.Empty,
                AlbumName = playlistTrack.Track.Album?.Title?.Trim(),
                DurationInSeconds = playlistTrack.Track.DurationInSeconds,
                Genre = playlistTrack.Track.Genre,
                CoverImageUrl = playlistTrack.Track.CoverImageUrl?.Trim(),
                AudioFileUrl = playlistTrack.Track.AudioFileUrl?.Trim() ?? string.Empty,
                IsLikedByCurrentUser = playlistTrack.Track.Likes?.Any(l => l.UserId == currentUserId) ?? false
            };
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

    public class AddTrackToPlaylistViewModel
    {
        [Required]
        public Guid PlaylistId { get; set; }

        [Required]
        public Guid TrackId { get; set; }

        public int? Position { get; set; }

        [Display(Name = "Playlist Name")]
        public string PlaylistName { get; set; } = string.Empty;

        [Display(Name = "Track Title")]
        public string TrackTitle { get; set; } = string.Empty;

        [Display(Name = "Insert Position")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be a positive number")]
        public int InsertPosition { get; set; } = 1;

        public bool CanAddToPlaylist { get; set; }
    }

    public class ReorderPlaylistTracksViewModel
    {
        [Required]
        public Guid PlaylistId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one track order must be specified")]
        public List<TrackOrderViewModel> TrackOrders { get; set; } = new();

        public bool CanReorder { get; set; }
    }

    public class TrackOrderViewModel
    {
        [Required]
        public Guid TrackId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be a positive number")]
        public int Order { get; set; }
    }
}
