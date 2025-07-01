using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Takip işlemleri için servis arayüzü
    public interface IFollowService
    {
        // User following
        Task<bool> FollowUserAsync(Guid followerId, Guid followedUserId);
        Task<bool> UnfollowUserAsync(Guid followerId, Guid followedUserId);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followedUserId);
        Task<IEnumerable<UserViewModel>> GetFollowersAsync(Guid userId, int page, int pageSize);
        Task<IEnumerable<UserViewModel>> GetFollowingAsync(Guid userId, int page, int pageSize);
        Task<int> GetFollowerCountAsync(Guid userId);
        Task<int> GetFollowingCountAsync(Guid userId);

        // Artist following (artists are users who create music)
        Task<bool> ToggleArtistFollowAsync(Guid userId, Guid artistId);
        Task<bool> IsArtistFollowedAsync(Guid userId, Guid artistId);
        Task<int> GetArtistFollowerCountAsync(Guid artistId);
        Task<IEnumerable<UserViewModel>> GetArtistFollowersAsync(Guid artistId, int page, int pageSize);
        Task<IEnumerable<UserViewModel>> GetFollowedArtistsAsync(Guid userId, int page, int pageSize);

        // Playlist following
        Task<bool> TogglePlaylistFollowAsync(Guid userId, Guid playlistId);
        Task<bool> IsPlaylistFollowedAsync(Guid userId, Guid playlistId);
        Task<int> GetPlaylistFollowerCountAsync(Guid playlistId);
        Task<IEnumerable<UserViewModel>> GetPlaylistFollowersAsync(Guid playlistId, int page, int pageSize);
        Task<IEnumerable<PlaylistViewModel>> GetFollowedPlaylistsAsync(Guid userId, int page, int pageSize);
    }
}
