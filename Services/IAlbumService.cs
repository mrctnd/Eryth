using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Albüm işlemleri için servis arayüzü
    public interface IAlbumService
    {
        Task<Album?> GetByIdAsync(Guid id);
        Task<AlbumViewModel> GetAlbumViewModelAsync(Guid albumId, Guid currentUserId);
        Task<IEnumerable<AlbumViewModel>> GetUserAlbumsAsync(Guid userId, Guid currentUserId, int page, int pageSize);
        Task<IEnumerable<AlbumViewModel>> GetPublicAlbumsAsync(int page, int pageSize);
        Task<IEnumerable<AlbumViewModel>> GetTrendingAlbumsAsync(int page, int pageSize);
        Task<Album> CreateAlbumAsync(AlbumCreateViewModel model, Guid userId);
        Task<bool> UpdateAlbumAsync(Guid albumId, AlbumViewModel model, Guid userId);
        Task<bool> DeleteAlbumAsync(Guid albumId, Guid userId);
        Task<bool> AddTrackToAlbumAsync(Guid albumId, Guid trackId, Guid userId);
        Task<bool> RemoveTrackFromAlbumAsync(Guid albumId, Guid trackId, Guid userId);
        Task<IEnumerable<TrackViewModel>> GetAlbumTracksAsync(Guid albumId, Guid userId);
        Task<IEnumerable<AlbumViewModel>> SearchAlbumsAsync(string query, int page, int pageSize);
        Task<bool> IsTrackInAlbumAsync(Guid albumId, Guid trackId);
        Task<bool> CanUserEditAlbumAsync(Guid albumId, Guid userId);
        Task<int> GetTotalAlbumCountAsync();
    }
}
