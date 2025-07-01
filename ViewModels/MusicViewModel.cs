using System.ComponentModel.DataAnnotations;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class MusicViewModel
    {
        public List<TrackViewModel> FeaturedTracks { get; set; } = new();
        public List<AlbumViewModel> FeaturedAlbums { get; set; } = new();
        public List<PlaylistViewModel> FeaturedPlaylists { get; set; } = new();
        public List<TrackViewModel> RecentlyPlayed { get; set; } = new();
        public List<TrackViewModel> TrendingTracks { get; set; } = new();
        public List<AlbumViewModel> NewReleases { get; set; } = new();
        public List<UserProfileViewModel> FeaturedArtists { get; set; } = new();

        public List<TrackViewModel> RecommendedTracks { get; set; } = new();
        public List<AlbumViewModel> RecommendedAlbums { get; set; } = new();
        public List<PlaylistViewModel> RecommendedPlaylists { get; set; } = new();

        public Dictionary<Genre, List<TrackViewModel>> TracksByGenre { get; set; } = new();
        public Dictionary<Genre, List<AlbumViewModel>> AlbumsByGenre { get; set; } = new();

        public bool IsAuthenticated { get; set; }
        public UserProfileViewModel? CurrentUser { get; set; }
        public List<PlaylistViewModel> UserPlaylists { get; set; } = new();

        public int TotalTracks { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalArtists { get; set; }
        public int TotalPlaylists { get; set; }

        public MusicPlayerViewModel? CurrentlyPlaying { get; set; }

        public bool HasFeaturedContent =>
            FeaturedTracks.Any() || FeaturedAlbums.Any() || FeaturedPlaylists.Any();

        public bool HasUserContent =>
            IsAuthenticated && (RecentlyPlayed.Any() || UserPlaylists.Any());

        public bool HasRecommendations =>
            RecommendedTracks.Any() || RecommendedAlbums.Any() || RecommendedPlaylists.Any();
    }

    public class MusicBrowseViewModel
    {
        public List<TrackViewModel> Tracks { get; set; } = new();
        public List<AlbumViewModel> Albums { get; set; } = new();
        public List<PlaylistViewModel> Playlists { get; set; } = new();
        public List<UserProfileViewModel> Artists { get; set; } = new();

        public int TotalTracks { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalArtists { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 24;
        public string ActiveTab { get; set; } = "tracks";

        public string? SearchTerm { get; set; }
        public Genre? FilterGenre { get; set; }
        public DateTime? ReleaseDateFrom { get; set; }
        public DateTime? ReleaseDateTo { get; set; }
        public bool? IsExplicit { get; set; }
        public string? ArtistName { get; set; }
        public string SortBy { get; set; } = "Popularity";
        public bool SortDescending { get; set; } = true;
        public string ViewMode { get; set; } = "grid"; 
        public bool ShowExplicitContent { get; set; } = true;

        public int GetTotalCount()
        {
            return ActiveTab switch
            {
                "tracks" => TotalTracks,
                "albums" => TotalAlbums,
                "playlists" => TotalPlaylists,
                "artists" => TotalArtists,
                _ => 0
            };
        }

        public int GetTotalPages()
        {
            var totalCount = GetTotalCount();
            return (int)Math.Ceiling((double)totalCount / PageSize);
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < GetTotalPages();

        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) ||
                                 FilterGenre.HasValue ||
                                 ReleaseDateFrom.HasValue ||
                                 ReleaseDateTo.HasValue ||
                                 IsExplicit.HasValue ||
                                 !string.IsNullOrEmpty(ArtistName);
    }

    public class MusicUploadViewModel
    {
        [Required(ErrorMessage = "Please select files to upload")]
        [Display(Name = "Music Files")]
        public List<IFormFile> AudioFiles { get; set; } = new();

        [Display(Name = "Cover Images")]
        public List<IFormFile>? CoverImages { get; set; }

        [Display(Name = "Album")]
        public Guid? AlbumId { get; set; }

        [Display(Name = "Genre")]
        public Genre? DefaultGenre { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? DefaultReleaseDate { get; set; }

        [Display(Name = "Explicit Content")]
        public bool IsExplicit { get; set; }

        [Display(Name = "Allow Comments")]
        public bool AllowComments { get; set; } = true;

        [Display(Name = "Allow Downloads")]
        public bool AllowDownloads { get; set; } = false;

        [StringLength(100, ErrorMessage = "Copyright information cannot exceed 100 characters")]
        [Display(Name = "Copyright")]
        public string? Copyright { get; set; }

        public int TotalFiles => AudioFiles?.Count ?? 0;
        public int ProcessedFiles { get; set; }
        public List<string> UploadErrors { get; set; } = new();
        public List<TrackViewModel> UploadedTracks { get; set; } = new();

        public TrackStatus DefaultStatus { get; set; } = TrackStatus.Draft;
        public bool RequireReview { get; set; }

        // Validasyon işlemi
        public bool IsValid()
        {
            return AudioFiles?.Any() == true &&
                   AudioFiles.All(f => IsValidAudioFile(f));
        }

        private static bool IsValidAudioFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a", ".aac" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var maxSize = 100 * 1024 * 1024; // 100MB

            return allowedExtensions.Contains(extension) &&
                   file.Length > 0 &&
                   file.Length <= maxSize;
        }

        public static bool IsValidImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var maxSize = 10 * 1024 * 1024; // 10MB

            return allowedExtensions.Contains(extension) &&
                   file.Length > 0 &&
                   file.Length <= maxSize;
        }
    }
}
