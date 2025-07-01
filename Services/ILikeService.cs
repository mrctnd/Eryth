using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Beğeni işlemleri için servis arayüzü
    public interface ILikeService
    {
        Task<bool> LikeTrackAsync(Guid trackId, Guid userId);
        Task<bool> UnlikeTrackAsync(Guid trackId, Guid userId);
        Task<bool> LikePlaylistAsync(Guid playlistId, Guid userId);
        Task<bool> UnlikePlaylistAsync(Guid playlistId, Guid userId);
        Task<bool> IsTrackLikedAsync(Guid trackId, Guid userId);
        Task<bool> IsPlaylistLikedAsync(Guid playlistId, Guid userId);
        Task<bool> IsLikedByUserAsync(Guid entityId, Guid userId, string entityType);
        Task<int> GetTrackLikeCountAsync(Guid trackId);
        Task<int> GetPlaylistLikeCountAsync(Guid playlistId);
        Task<IEnumerable<TrackViewModel>> GetLikedTracksAsync(Guid userId, int page, int pageSize);
        Task<IEnumerable<PlaylistViewModel>> GetLikedPlaylistsAsync(Guid userId, int page, int pageSize);
    }
}
