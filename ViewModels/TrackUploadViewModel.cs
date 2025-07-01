using System.ComponentModel.DataAnnotations;
using Eryth.Models;

namespace Eryth.ViewModels
{
    // Müzik yükleme için ViewModel
    public class UploadTrackViewModel
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sanatçı gereklidir")]
        [StringLength(100, ErrorMessage = "Sanatçı adı en fazla 100 karakter olabilir")]
        public string Artist { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Müzik dosyası gereklidir")]
        public IFormFile AudioFile { get; set; } = null!;

        public IFormFile? CoverImage { get; set; }
        [Required(ErrorMessage = "Tür seçimi gereklidir")]
        public string Genre { get; set; } = string.Empty;

        public string? Tags { get; set; }

        public bool IsExplicit { get; set; }

        public bool IsPublic { get; set; } = true; public Guid? AlbumId { get; set; }
    }
}
