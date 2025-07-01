using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class AddTrackToAlbumViewModel
    {
        [Required(ErrorMessage = "Track title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int DurationInSeconds { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        public Eryth.Models.Enums.Genre Genre { get; set; }

        [Required(ErrorMessage = "Album ID is required")]
        public Guid AlbumId { get; set; }

        public IFormFile? AudioFile { get; set; }
        
        public string? Description { get; set; }
        
        public bool IsExplicit { get; set; }
        
        public bool AllowComments { get; set; } = true;
        
        public bool AllowDownloads { get; set; } = false;
    }
}
