using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Report/moderation management service interface
    public interface IReportService
    {
        // Basic report operations
        Task<Report?> GetByIdAsync(Guid id);
        Task<Report> CreateReportAsync(CreateReportViewModel model, Guid reporterId);
        Task<bool> HandleReportAsync(Guid reportId, string action, Guid moderatorId, string? notes = null);
        Task<bool> UpdateReportStatusAsync(Guid reportId, string status, Guid moderatorId, string? notes = null);

        // Admin/Moderation operations
        Task<IEnumerable<ReportViewModel>> GetReportsForAdminAsync(int page, int pageSize, string? status = null, string? type = null);
        Task<IEnumerable<ReportViewModel>> GetUserReportsAsync(Guid userId);
        Task<IEnumerable<ReportViewModel>> GetReportsAgainstUserAsync(Guid userId);
        Task<IEnumerable<ReportViewModel>> GetRecentReportsAsync(int count);

        // Statistics
        Task<int> GetPendingReportCountAsync();
        Task<int> GetTotalReportCountAsync();
        Task<int> GetReportCountByStatusAsync(string status);

        // Content-specific reports
        Task<IEnumerable<ReportViewModel>> GetTrackReportsAsync(Guid trackId);
        Task<IEnumerable<ReportViewModel>> GetPlaylistReportsAsync(Guid playlistId);
        Task<IEnumerable<ReportViewModel>> GetCommentReportsAsync(Guid commentId);
        Task<IEnumerable<ReportViewModel>> GetUserProfileReportsAsync(Guid userId);

        // Bulk operations
        Task<bool> BulkHandleReportsAsync(IEnumerable<Guid> reportIds, string action, Guid moderatorId, string? notes = null);
        Task<bool> DeleteReportAsync(Guid reportId, Guid userId);

        // Search and filtering
        Task<IEnumerable<ReportViewModel>> SearchReportsAsync(string query, int page, int pageSize);
        Task<IEnumerable<ReportViewModel>> GetReportsByTypeAsync(string type, int page, int pageSize);
        Task<IEnumerable<ReportViewModel>> GetReportsByPriorityAsync(string priority, int page, int pageSize);

        // Validation
        Task<bool> CanUserReportAsync(Guid reporterId, Guid targetId, string targetType);
        Task<bool> HasUserReportedAsync(Guid reporterId, Guid targetId, string targetType);
    }
}
