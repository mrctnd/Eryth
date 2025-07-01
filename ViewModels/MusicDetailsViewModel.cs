using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class MusicDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public Guid ArtistId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool IsExplicit { get; set; }

        public string ContentType { get; set; } = string.Empty;
        public bool IsTrack => ContentType == "Track";
        public bool IsAlbum => ContentType == "Album";

        public string? AudioFileUrl { get; set; }
        public int? DurationInSeconds { get; set; }
        public string? FormattedDuration => DurationInSeconds.HasValue ?
            TimeSpan.FromSeconds(DurationInSeconds.Value).ToString(@"mm\:ss") : null;
        public bool? AllowComments { get; set; }
        public bool? AllowDownloads { get; set; }

        public string? RecordLabel { get; set; }
        public List<TrackViewModel>? Tracks { get; set; }
        public int? TrackCount => Tracks?.Count;
        public TimeSpan? TotalDuration => Tracks?.Any() == true ?
            TimeSpan.FromSeconds(Tracks.Sum(t => t.DurationInSeconds)) : null;

        public long PlayCount { get; set; }
        public long LikeCount { get; set; }
        public long CommentCount { get; set; }

        public bool IsLikedByCurrentUser { get; set; }
        public bool IsFollowingArtist { get; set; }

        // Security and permissions
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanComment { get; set; }
        public bool CanDownload { get; set; }
        public bool CanShare { get; set; } = true;

        // Related content
        public List<CommentViewModel> RecentComments { get; set; } = new();
        public List<AlbumViewModel> RelatedAlbums { get; set; } = new();
        public List<TrackViewModel> RelatedTracks { get; set; } = new();
        public List<UserProfileViewModel> RecentLikes { get; set; } = new();

        public UserProfileViewModel? ArtistProfile { get; set; }

        public string? ReturnUrl { get; set; }

        public string GetShareUrl(string baseUrl)
        {
            var type = IsTrack ? "track" : "album";
            return $"{baseUrl.TrimEnd('/')}/{type}/{Id}";
        }

        public string GetShareTitle()
        {
            return IsTrack ? $"{Title} by {ArtistName}" : $"{Title} - Album by {ArtistName}";
        }

        public string GetShareDescription()
        {
            if (!string.IsNullOrEmpty(Description))
                return Description.Length > 150 ? Description[..147] + "..." : Description;

            return IsTrack ?
                $"Listen to {Title} by {ArtistName} on Eryth" :
                $"Discover {Title} album by {ArtistName} with {TrackCount} tracks on Eryth";
        }

        public static MusicDetailsViewModel FromTrack(TrackViewModel track, UserProfileViewModel? artistProfile = null)
        {
            return new MusicDetailsViewModel
            {
                Id = track.Id,
                Title = track.Title,
                Description = track.Description,
                CoverImageUrl = track.CoverImageUrl,
                ArtistName = track.ArtistName,
                ArtistId = track.ArtistId,
                ReleaseDate = track.ReleaseDate,
                IsExplicit = track.IsExplicit,
                ContentType = "Track",
                AudioFileUrl = track.AudioFileUrl,
                DurationInSeconds = track.DurationInSeconds,
                AllowComments = track.AllowComments,
                AllowDownloads = track.AllowDownloads,
                PlayCount = track.PlayCount,
                LikeCount = track.LikeCount,
                CommentCount = track.CommentCount,
                IsLikedByCurrentUser = track.IsLikedByCurrentUser,
                CanEdit = track.CanEdit,
                CanDelete = track.CanDelete,
                CanComment = track.CanComment,
                CanDownload = track.CanDownload,
                ArtistProfile = artistProfile
            };
        }

        public static MusicDetailsViewModel FromAlbum(AlbumViewModel album, UserProfileViewModel? artistProfile = null)
        {
            return new MusicDetailsViewModel
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                CoverImageUrl = album.CoverImageUrl,
                ArtistName = album.ArtistName,
                ArtistId = album.ArtistId,
                ReleaseDate = album.ReleaseDate,
                IsExplicit = album.IsExplicit,
                ContentType = "Album",
                RecordLabel = album.RecordLabel,
                Tracks = album.Tracks,
                PlayCount = album.TotalPlayCount,
                LikeCount = album.TotalLikeCount,
                CommentCount = 0,
                CanEdit = album.CanEdit,
                CanDelete = album.CanDelete,
                CanComment = false,
                ArtistProfile = artistProfile
            };
        }
    }

    public class MusicPlayerViewModel
    {
        public Guid TrackId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string AudioFileUrl { get; set; } = string.Empty;
        public int DurationInSeconds { get; set; }
        public string FormattedDuration => TimeSpan.FromSeconds(DurationInSeconds).ToString(@"mm\:ss");

        public Guid? PlaylistId { get; set; }
        public string? PlaylistTitle { get; set; }
        public List<Guid>? PlaylistTrackIds { get; set; }
        public int? CurrentTrackIndex { get; set; }

        public bool HasNext => PlaylistTrackIds != null && CurrentTrackIndex < PlaylistTrackIds.Count - 1;
        public bool HasPrevious => PlaylistTrackIds != null && CurrentTrackIndex > 0;

        public bool IsLiked { get; set; }
        public bool AllowDownload { get; set; }
        public int Volume { get; set; } = 50;
        public bool IsMuted { get; set; }
        public bool IsShuffleEnabled { get; set; }
        public string RepeatMode { get; set; } = "off";

        public bool CanDownload { get; set; }
        public bool CanShare { get; set; } = true;
    }
}
