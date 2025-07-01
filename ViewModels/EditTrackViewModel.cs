using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class EditTrackViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık 200 karakteri geçemez")]
        public string? Title { get; set; }
        
        public string? Description { get; set; }
        [Required(ErrorMessage = "Tür gereklidir")]
        public Eryth.Models.Enums.Genre Genre { get; set; }
        
        public string? SubGenre { get; set; }
        public string? Tags { get; set; }
        public string? Composer { get; set; }
        public string? Producer { get; set; }
        public string? Lyricist { get; set; }
        public string? Copyright { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool IsExplicit { get; set; }
        public bool AllowComments { get; set; }
        public bool AllowDownloads { get; set; }
        
        public IFormFile? NewCoverImage { get; set; }
        
        public static EditTrackViewModel FromTrack(Eryth.Models.Track track)
        {
            return new EditTrackViewModel
            {
                Id = track.Id,
                Title = track.Title,
              Description = track.Description,
                Genre = track.Genre,
                SubGenre = track.SubGenre,
                Tags = track.Tags,
                Composer = track.Composer,
                Producer = track.Producer,
                Lyricist = track.Lyricist,
                Copyright = track.Copyright,
                ReleaseDate = track.ReleaseDate,
                IsExplicit = track.IsExplicit,
                AllowComments = track.AllowComments,
                AllowDownloads = track.AllowDownloads
            };
        }
    }
}
