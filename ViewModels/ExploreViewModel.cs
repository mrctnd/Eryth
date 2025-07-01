namespace Eryth.ViewModels
{
    public class ExploreViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public List<TrackViewModel> Tracks { get; set; } = new List<TrackViewModel>();
        public List<PlaylistViewModel> Playlists { get; set; } = new List<PlaylistViewModel>();
        public List<AlbumViewModel> Albums { get; set; } = new List<AlbumViewModel>();
        
        public int CurrentPage { get; set; } = 1;
        public bool HasNextPage { get; set; } = false;
        
        public bool HasUsers => Users?.Any() == true;
        public bool HasTracks => Tracks?.Any() == true;
        public bool HasPlaylists => Playlists?.Any() == true;
        public bool HasAlbums => Albums?.Any() == true;
        public bool HasAnyContent => HasUsers || HasTracks || HasPlaylists || HasAlbums;
    }
}
