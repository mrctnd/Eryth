using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Müzik parçası işlemleri için servis arayüzü
    public interface ITrackService
    {
        Task<IEnumerable<Track>> GetAllTracksAsync();
        Task<IEnumerable<TrackViewModel>> GetAllTracksAsync(int page, int pageSize);
        Task<Track?> GetTrackByIdAsync(Guid id);
        Task<Track?> GetByIdAsync(Guid id);
        Task<TrackViewModel> GetTrackViewModelAsync(Guid trackId, Guid currentUserId);
        Task<Track> CreateTrackAsync(TrackViewModel model, Guid userId);
        Task<Track> CreateTrackAsync(UploadTrackViewModel model, Guid userId); Task<bool> UpdateTrackAsync(Guid trackId, TrackViewModel model, Guid userId);
        Task<bool> UpdateTrackAsync(Guid trackId, EditTrackViewModel model, Guid userId);
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> DeleteTrackAsync(Guid trackId, Guid userId);
        Task<IEnumerable<Track>> GetUserTracksAsync(Guid userId, Guid? currentUserId = null);
        Task<IEnumerable<TrackViewModel>> GetUserTracksAsync(Guid userId, Guid currentUserId, int page, int pageSize);
        Task<int> GetUserTrackCountAsync(Guid userId);
        Task<IEnumerable<Track>> SearchTracksAsync(string query);
        Task<IEnumerable<Track>> GetTracksByGenreAsync(string genre);
        Task<IEnumerable<Track>> GetPopularTracksAsync(int count = 10);
        Task<IEnumerable<Track>> GetRecentTracksAsync(int count = 10);
        Task<bool> IncrementPlayCountAsync(Guid trackId, Guid? userId = null);
        Task<TrackViewModel> IncrementPlayCountAsync(Guid trackId, Guid userId);        // New methods for HomeController
        Task<IEnumerable<TrackViewModel>> GetTrendingTracksAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<TrackViewModel>> GetDiscoverTracksAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<TrackViewModel>> SearchTracksAsync(string query, int page = 1, int pageSize = 20);        // Admin-specific methods
        Task<int> GetTotalTrackCountAsync();
        Task<int> GetNewTrackCountAsync(DateTime fromDate);
        Task<IEnumerable<TrackViewModel>> GetRecentTracksForAdminAsync(int count);
        Task<IEnumerable<TrackViewModel>> GetTracksForAdminAsync(int page, int pageSize, string? search = null, TrackStatus? status = null);
        Task<bool> UpdateTrackStatusAsync(Guid trackId, TrackStatus newStatus, Guid modifiedBy, string? reason = null);
        
        // Album track methods
        Task<bool> AddTrackToAlbumAsync(AddTrackToAlbumViewModel model, Guid userId);    }
}
