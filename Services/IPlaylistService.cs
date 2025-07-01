using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Çalma listesi işlemleri için servis arayüzü
    public interface IPlaylistService
    {
        Task<Playlist?> GetByIdAsync(Guid id);
        Task<PlaylistViewModel> GetPlaylistViewModelAsync(Guid playlistId, Guid currentUserId);
        Task<IEnumerable<PlaylistViewModel>> GetUserPlaylistsAsync(Guid userId, Guid currentUserId, int page, int pageSize);
        Task<IEnumerable<PlaylistViewModel>> GetPublicPlaylistsAsync(int page, int pageSize);
        Task<Playlist> CreatePlaylistAsync(CreatePlaylistViewModel model, Guid userId);
        Task<bool> UpdatePlaylistAsync(Guid playlistId, PlaylistViewModel model, Guid userId);
        Task<bool> DeletePlaylistAsync(Guid playlistId, Guid userId);
        Task<bool> AddTrackToPlaylistAsync(Guid playlistId, Guid trackId, Guid userId);
        Task<bool> RemoveTrackFromPlaylistAsync(Guid playlistId, Guid trackId, Guid userId);
        Task<bool> IsTrackInPlaylistAsync(Guid playlistId, Guid trackId);
        Task<IEnumerable<TrackViewModel>> GetPlaylistTracksAsync(Guid playlistId, Guid userId);

        // Admin-specific methods
        Task<int> GetTotalPlaylistCountAsync();
        Task<int> GetNewPlaylistCountAsync(int days = 7);
        Task<IEnumerable<PlaylistViewModel>> GetRecentPlaylistsAsync(int count = 10);
        Task<IEnumerable<PlaylistViewModel>> GetPlaylistsForAdminAsync(int page = 1, int pageSize = 20, string? search = null, bool? isPublic = null);
        Task<bool> UpdatePlaylistStatusAsync(Guid playlistId, bool isActive);
        // Çalma listesi klonlama işlemi için metod
        Task<Playlist?> DuplicatePlaylistAsync(Guid playlistId, Guid userId);
    }
}
