using System.ComponentModel.DataAnnotations;
using Eryth.Models;
using Eryth.Models.Enums;

namespace Eryth.ViewModels
{
    public class AlbumViewModel
    {
        public Guid Id { get; set; }        [Required(ErrorMessage = "Albüm başlığı gereklidir")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Albüm başlığı 1 ile 200 karakter arasında olmalıdır")]
        [Display(Name = "Albüm Başlığı")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama 1000 karakteri geçemez")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sanatçı gereklidir")]
        public Guid ArtistId { get; set; }

        [Display(Name = "Sanatçı Adı")]
        public string ArtistName { get; set; } = string.Empty;

        [Display(Name = "Kapak Görseli")]
        [Url(ErrorMessage = "Lütfen kapak görseli için geçerli bir URL sağlayın")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Çıkış tarihi gereklidir")]
        [Display(Name = "Çıkış Tarihi")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "Ana tür gereklidir")]
        [Display(Name = "Ana Tür")]
        public Genre PrimaryGenre { get; set; }

        [StringLength(100, ErrorMessage = "Plak şirketi 100 karakteri geçemez")]
        [Display(Name = "Plak Şirketi")]
        public string? RecordLabel { get; set; }

        [StringLength(100, ErrorMessage = "Telif hakkı bilgisi 100 karakteri geçemez")]
        [Display(Name = "Telif Hakkı")]
        public string? Copyright { get; set; }

        [Display(Name = "Müstehcen İçerik")]
        public bool IsExplicit { get; set; }

        // Read-only display properties
        public long TotalPlayCount { get; set; }
        public long TotalLikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties for display
        public List<TrackViewModel> Tracks { get; set; } = new();
        public int TrackCount => Tracks.Count;
        public TimeSpan TotalDuration => TimeSpan.FromSeconds(Tracks.Sum(t => t.DurationInSeconds));

        // Security: Only show if user owns the album or is admin
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        // Factory method to create from domain model
        public static AlbumViewModel FromAlbum(Album album, bool canEdit = false, bool canDelete = false)
        {
            return new AlbumViewModel
            {
                Id = album.Id,
                Title = album.Title,
                Description = album.Description,
                ArtistId = album.ArtistId,
                ArtistName = album.Artist?.DisplayName ?? "Bilinmeyen Sanatçı",
                CoverImageUrl = album.CoverImageUrl,
                ReleaseDate = album.ReleaseDate ?? DateTime.UtcNow,
                PrimaryGenre = album.PrimaryGenre,
                RecordLabel = album.RecordLabel,
                Copyright = album.Copyright,
                IsExplicit = album.IsExplicit,
                TotalPlayCount = album.TotalPlayCount,
                TotalLikeCount = album.TotalLikeCount,
                CreatedAt = album.CreatedAt,
                UpdatedAt = album.UpdatedAt,
                Tracks = album.Tracks?.Select(t => TrackViewModel.FromTrack(t)).ToList() ?? new(),
                CanEdit = canEdit,
                CanDelete = canDelete
            };
        }

        // Method to convert to domain model for updates
        public Album ToAlbum()
        {
            return new Album
            {
                Id = Id,
                Title = Title.Trim(),
                Description = Description?.Trim(),
                ArtistId = ArtistId,
                CoverImageUrl = CoverImageUrl?.Trim(),
                ReleaseDate = ReleaseDate,
                PrimaryGenre = PrimaryGenre,
                RecordLabel = RecordLabel?.Trim(),
                Copyright = Copyright?.Trim(),
                IsExplicit = IsExplicit
            };
        }
    }    public class AlbumCreateViewModel
    {
        [Required(ErrorMessage = "Album title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Album title must be between 1 and 200 characters")]
        [Display(Name = "Album Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Cover Image File")]
        public IFormFile? CoverImage { get; set; }

        [Display(Name = "Cover Image URL (optional)")]
        [Url(ErrorMessage = "Please provide a valid URL for cover image")]
        public string? CoverImageUrl { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Primary genre is required")]
        [Display(Name = "Primary Genre")]
        public Genre PrimaryGenre { get; set; }

        [StringLength(100, ErrorMessage = "Record label cannot exceed 100 characters")]
        [Display(Name = "Record Label")]
        public string? RecordLabel { get; set; }

        [StringLength(100, ErrorMessage = "Copyright information cannot exceed 100 characters")]
        [Display(Name = "Copyright")]
        public string? Copyright { get; set; }

        [Display(Name = "Explicit Content")]
        public bool IsExplicit { get; set; }
    }

    public class AlbumListViewModel
    {
        public List<AlbumViewModel> Albums { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Filter properties
        public string? SearchTerm { get; set; }
        public Genre? FilterGenre { get; set; }
        public string? SortBy { get; set; } = "ReleaseDate";
        public bool SortDescending { get; set; } = true;
    }
}
