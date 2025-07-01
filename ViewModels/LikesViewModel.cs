using System.ComponentModel.DataAnnotations;

namespace Eryth.ViewModels
{
    public class LikesViewModel
    {
        public string ActiveTab { get; set; } = "tracks";
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool HasNextPage { get; set; }

        public List<TrackViewModel> LikedTracks { get; set; } = new();
        public List<PlaylistViewModel> LikedPlaylists { get; set; } = new();

        // Computed properties
        public int TotalLikedTracks => LikedTracks.Count;
        public int TotalLikedPlaylists => LikedPlaylists.Count;
        public bool HasLikedTracks => LikedTracks.Any();
        public bool HasLikedPlaylists => LikedPlaylists.Any();
    }
}
