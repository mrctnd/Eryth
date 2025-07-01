using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class TrackViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Şarkı başlığı gereklidir")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Şarkı başlığı 1 ile 200 karakter arasında olmalıdır")]
        [Display(Name = "Şarkı Başlığı")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama 1000 karakteri geçemez")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sanatçı gereklidir")]
        public Guid ArtistId { get; set; }

        [Display(Name = "Sanatçı")]
        public string ArtistName { get; set; } = string.Empty;

        public Guid? AlbumId { get; set; }

        [Display(Name = "Albüm")]
        public string? AlbumTitle { get; set; }

        [Required(ErrorMessage = "Ses dosyası gereklidir")]
        [Display(Name = "Ses Dosyası")]
        [Url(ErrorMessage = "Lütfen geçerli bir ses dosyası URL'si sağlayın")]
        public string AudioFileUrl { get; set; } = string.Empty;

        [Display(Name = "Kapak Görseli")]
        [Url(ErrorMessage = "Lütfen geçerli bir kapak görseli URL'si sağlayın")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Süre gereklidir")]
        [Range(1, int.MaxValue, ErrorMessage = "Süre 0'dan büyük olmalıdır")]
        [Display(Name = "Süre (saniye)")]
        public int DurationInSeconds { get; set; }

        [Required(ErrorMessage = "Tür gereklidir")]
        [Display(Name = "Tür")]
        public Genre Genre { get; set; }

        [Display(Name = "Alt Tür")]
        public string? SubGenre { get; set; }

        [StringLength(100, ErrorMessage = "Besteci adı 100 karakteri geçemez")]
        [Display(Name = "Besteci")]
        public string? Composer { get; set; }

        [StringLength(100, ErrorMessage = "Söz yazarı adı 100 karakteri geçemez")]
        [Display(Name = "Söz Yazarı")]
        public string? Lyricist { get; set; }

        [StringLength(100, ErrorMessage = "Yapımcı adı 100 karakteri geçemez")]
        [Display(Name = "Yapımcı")]
        public string? Producer { get; set; }

        [StringLength(100, ErrorMessage = "Telif hakkı bilgisi 100 karakteri geçemez")]
        [Display(Name = "Telif Hakkı")]
        public string? Copyright { get; set; }

        [Required(ErrorMessage = "Çıkış tarihi gereklidir")]
        [Display(Name = "Çıkış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Müstehcen İçerik")]
        public bool IsExplicit { get; set; }

        [Display(Name = "Yorumlara İzin Ver")]
        public bool AllowComments { get; set; } = true;

        [Display(Name = "İndirmeye İzin Ver")]
        public bool AllowDownloads { get; set; } = false;

        [Display(Name = "Durum")]
        public TrackStatus Status { get; set; } = TrackStatus.Active;

        public long PlayCount { get; set; }
        public long LikeCount { get; set; }
        public long CommentCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDownload { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public string FormattedDuration => TimeSpan.FromSeconds(DurationInSeconds).ToString(@"mm\:ss");
        public bool IsPublished => Status == TrackStatus.Active;
        public string Artist => ArtistName;
        public DateTime UploadDate => CreatedAt;
        public bool CanComment => AllowComments && IsPublished;
        public string RelativeUploadDate => GetRelativeTime(CreatedAt);

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
                _ => $"{(int)(timeSpan.TotalDays / 365)}y ago"            };
        }

        public static TrackViewModel FromTrack(Track track, bool canEdit = false, bool canDelete = false, bool canDownload = false, bool isLikedByCurrentUser = false)
        {
            return new TrackViewModel
            {
                Id = track.Id,
                Title = track.Title,
                Description = track.Description,
                ArtistId = track.ArtistId,
                ArtistName = track.Artist?.DisplayName ?? track.Artist?.Username ?? "Unknown Artist",
                AlbumId = track.AlbumId,
                AlbumTitle = track.Album?.Title,
                AudioFileUrl = track.AudioFileUrl,
                CoverImageUrl = track.CoverImageUrl ?? track.Album?.CoverImageUrl,
                DurationInSeconds = track.DurationInSeconds,
                Genre = track.Genre,
                SubGenre = track.SubGenre,
                Composer = track.Composer,
                Lyricist = track.Lyricist,
                Producer = track.Producer,
                Copyright = track.Copyright,
                ReleaseDate = track.ReleaseDate ?? DateTime.UtcNow,
                IsExplicit = track.IsExplicit,
                AllowComments = track.AllowComments,
                AllowDownloads = track.AllowDownloads,
                Status = track.Status,
                PlayCount = track.PlayCount,
                LikeCount = track.Likes?.Count ?? 0,
                CommentCount = track.Comments?.Count ?? 0,
                CreatedAt = track.CreatedAt,
                UpdatedAt = track.UpdatedAt,
                CanEdit = canEdit,
                CanDelete = canDelete,
                CanDownload = canDownload && track.AllowDownloads,
                IsLikedByCurrentUser = isLikedByCurrentUser
            };
        }

        public Track ToTrack()
        {
            return new Track
            {
                Id = Id,
                Title = Title.Trim(),
                Description = Description?.Trim(),
                ArtistId = ArtistId,
                AlbumId = AlbumId,
                AudioFileUrl = AudioFileUrl.Trim(),
                CoverImageUrl = CoverImageUrl?.Trim(),
                DurationInSeconds = DurationInSeconds,
                Genre = Genre,
                SubGenre = SubGenre?.Trim(),
                Composer = Composer?.Trim(),
                Lyricist = Lyricist?.Trim(),
                Producer = Producer?.Trim(),
                Copyright = Copyright?.Trim(),
                ReleaseDate = ReleaseDate,
                IsExplicit = IsExplicit,
                AllowComments = AllowComments,
                AllowDownloads = AllowDownloads,
                Status = Status
            };
        }
    }

    public class TrackCreateViewModel
    {
        [Required(ErrorMessage = "Track title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Track title must be between 1 and 200 characters")]
        [Display(Name = "Track Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public Guid? AlbumId { get; set; }

        [Required(ErrorMessage = "Audio file is required")]
        [Display(Name = "Audio File")]
        [Url(ErrorMessage = "Please provide a valid audio file URL")]
        public string AudioFileUrl { get; set; } = string.Empty;

        [Display(Name = "Cover Image")]
        [Url(ErrorMessage = "Please provide a valid cover image URL")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        [Display(Name = "Duration (seconds)")]
        public int DurationInSeconds { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [Display(Name = "Genre")]
        public Genre Genre { get; set; }

        [Display(Name = "Sub Genre")]
        public Genre? SubGenre { get; set; }

        [StringLength(100, ErrorMessage = "Composer name cannot exceed 100 characters")]
        [Display(Name = "Composer")]
        public string? Composer { get; set; }

        [StringLength(100, ErrorMessage = "Lyricist name cannot exceed 100 characters")]
        [Display(Name = "Lyricist")]
        public string? Lyricist { get; set; }

        [StringLength(100, ErrorMessage = "Producer name cannot exceed 100 characters")]
        [Display(Name = "Producer")]
        public string? Producer { get; set; }

        [StringLength(100, ErrorMessage = "Copyright information cannot exceed 100 characters")]
        [Display(Name = "Copyright")]
        public string? Copyright { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        [Display(Name = "Explicit Content")]
        public bool IsExplicit { get; set; }

        [Display(Name = "Allow Comments")]
        public bool AllowComments { get; set; } = true;

        [Display(Name = "Allow Downloads")]
        public bool AllowDownloads { get; set; } = false;

        [Display(Name = "Status")]
        public TrackStatus Status { get; set; } = TrackStatus.Draft;
    }

    public class TrackListViewModel
    {
        public List<TrackViewModel> Tracks { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public string? SearchTerm { get; set; }
        public Genre? FilterGenre { get; set; }
        public Guid? FilterArtistId { get; set; }
        public Guid? FilterAlbumId { get; set; }
        public TrackStatus? FilterStatus { get; set; }

        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;

        public string? Title { get; set; }
        public bool ShowAlbumInfo { get; set; } = true;
        public bool ShowArtistInfo { get; set; } = true;
    }
}
