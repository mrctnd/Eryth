using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class LibraryViewModel
    {
        public string ActiveFilter { get; set; } = "All";
        public List<TrackViewModel> UserTracks { get; set; } = new List<TrackViewModel>();
        public List<AlbumViewModel> UserAlbums { get; set; } = new List<AlbumViewModel>();
        public List<PlaylistViewModel> UserPlaylists { get; set; } = new List<PlaylistViewModel>();
        
        public int TotalTracks { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalItems { get; set; }

        public bool HasAnyContent => TotalItems > 0;
    }
}
