using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Kullanıcı dinleme geçmişi işlemleri için servis arayüzü
    public interface IHistoryService
    {
        Task<UserPlayHistory?> GetByIdAsync(Guid id);
        Task<HistoryViewModel> GetHistoryViewModelAsync(Guid historyId);
        Task<IEnumerable<HistoryViewModel>> GetUserHistoryAsync(Guid userId, int page, int pageSize, DateTime? fromDate = null, DateTime? toDate = null, bool? validPlaysOnly = null);
        Task<UserPlayHistory> RecordPlayAsync(PlayHistoryRecordViewModel model, Guid userId, string? ipAddress = null, string? userAgent = null);
        Task<bool> DeleteHistoryEntryAsync(Guid historyId, Guid userId);
        Task<bool> ClearUserHistoryAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<HistoryStatsViewModel> GetUserStatsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<TrackPlayStats>> GetTopTracksAsync(Guid userId, int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<ArtistPlayStats>> GetTopArtistsAsync(Guid userId, int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<HistoryViewModel>> GetRecentPlaysAsync(Guid userId, int count = 20);
        Task<int> GetTotalPlayCountAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<TimeSpan> GetTotalListeningTimeAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<HistoryViewModel>> SearchHistoryAsync(Guid userId, string searchTerm, int page, int pageSize);
        Task<bool> IsTrackPlayedByUserAsync(Guid trackId, Guid userId);
        Task<DateTime?> GetLastPlayedTimeAsync(Guid trackId, Guid userId);
    }
}
