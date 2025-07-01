using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Basic Report Operations

        public async Task<Report?> GetByIdAsync(Guid id)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<Report> CreateReportAsync(CreateReportViewModel model, Guid reporterId)
        {
            var report = new Report
            {
                ReporterId = reporterId,
                Type = model.Type.ToString(),
                Reason = model.Reason,
                Description = model.Description ?? string.Empty,
                ReportedUserId = ConvertToGuid(model.TargetUserId),
                TrackId = ConvertToGuid(model.TargetTrackId),
                PlaylistId = ConvertToGuid(model.TargetPlaylistId),
                CommentId = ConvertToGuid(model.TargetCommentId),
                Status = "Pending",
                Priority = model.Priority.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<bool> HandleReportAsync(Guid reportId, string action, Guid moderatorId, string? notes = null)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            report.ModeratorId = moderatorId;
            report.ModeratorNotes = notes;

            switch (action.ToLower())
            {
                case "approve":
                case "resolve":
                    report.Status = "Resolved";
                    report.ResolvedAt = DateTime.UtcNow;
                    break;
                case "dismiss":
                case "reject":
                    report.Status = "Dismissed";
                    report.ReviewedAt = DateTime.UtcNow;
                    break;
                case "review":
                    report.Status = "Reviewed";
                    report.ReviewedAt = DateTime.UtcNow;
                    break;
                default:
                    return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateReportStatusAsync(Guid reportId, string status, Guid moderatorId, string? notes = null)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            report.Status = status;
            report.ModeratorId = moderatorId;
            report.ModeratorNotes = notes;

            if (status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                report.ResolvedAt = DateTime.UtcNow;
            }
            else if (status.Equals("Reviewed", StringComparison.OrdinalIgnoreCase))
            {
                report.ReviewedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Admin/Moderation Operations

        public async Task<IEnumerable<ReportViewModel>> GetReportsForAdminAsync(int page, int pageSize, string? status = null, string? type = null)
        {
            var query = _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(r => r.Type == type);
            }

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetUserReportsAsync(Guid userId)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.ReporterId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetReportsAgainstUserAsync(Guid userId)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.ReportedUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetRecentReportsAsync(int count)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        #endregion

        #region Statistics

        public async Task<int> GetPendingReportCountAsync()
        {
            return await _context.Reports.CountAsync(r => r.Status == "Pending");
        }

        public async Task<int> GetTotalReportCountAsync()
        {
            return await _context.Reports.CountAsync();
        }

        public async Task<int> GetReportCountByStatusAsync(string status)
        {
            return await _context.Reports.CountAsync(r => r.Status == status);
        }

        #endregion

        #region Content-Specific Reports

        public async Task<IEnumerable<ReportViewModel>> GetTrackReportsAsync(Guid trackId)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.TrackId == trackId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetPlaylistReportsAsync(Guid playlistId)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.PlaylistId == playlistId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetCommentReportsAsync(Guid commentId)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.CommentId == commentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetUserProfileReportsAsync(Guid userId)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.ReportedUserId == userId && r.Type == "User")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkHandleReportsAsync(IEnumerable<Guid> reportIds, string action, Guid moderatorId, string? notes = null)
        {
            var reports = await _context.Reports
                .Where(r => reportIds.Contains(r.Id))
                .ToListAsync();

            if (!reports.Any()) return false;

            foreach (var report in reports)
            {
                report.ModeratorId = moderatorId;
                report.ModeratorNotes = notes;

                switch (action.ToLower())
                {
                    case "approve":
                    case "resolve":
                        report.Status = "Resolved";
                        report.ResolvedAt = DateTime.UtcNow;
                        break;
                    case "dismiss":
                    case "reject":
                        report.Status = "Dismissed";
                        report.ReviewedAt = DateTime.UtcNow;
                        break;
                    case "review":
                        report.Status = "Reviewed";
                        report.ReviewedAt = DateTime.UtcNow;
                        break;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReportAsync(Guid reportId, Guid userId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            // Only allow deletion by the reporter or an admin/moderator
            if (report.ReporterId != userId && report.ModeratorId != userId)
                return false;

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Search and Filtering

        public async Task<IEnumerable<ReportViewModel>> SearchReportsAsync(string query, int page, int pageSize)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.Reason.Contains(query) || r.Description.Contains(query) ||
                           (r.Reporter != null && r.Reporter.Username.Contains(query)) ||
                           (r.ReportedUser != null && r.ReportedUser.Username.Contains(query)))
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetReportsByTypeAsync(string type, int page, int pageSize)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.Type == type)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        public async Task<IEnumerable<ReportViewModel>> GetReportsByPriorityAsync(string priority, int page, int pageSize)
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Track)
                .Include(r => r.Playlist)
                .Include(r => r.Comment)
                .Include(r => r.Moderator)
                .Where(r => r.Priority == priority)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return reports.Select(MapToViewModel);
        }

        #endregion

        #region Validation

        public async Task<bool> CanUserReportAsync(Guid reporterId, Guid targetId, string targetType)
        {
            // Check if user exists and is active
            var reporter = await _context.Users.FindAsync(reporterId);
            if (reporter == null) return false;

            // Check if target exists based on type
            switch (targetType.ToLower())
            {
                case "user":
                    return await _context.Users.AnyAsync(u => u.Id == targetId);
                case "track":
                    return await _context.Tracks.AnyAsync(t => t.Id == targetId);
                case "playlist":
                    return await _context.Playlists.AnyAsync(p => p.Id == targetId);
                case "comment":
                    return await _context.Comments.AnyAsync(c => c.Id == targetId);
                default:
                    return false;
            }
        }

        public async Task<bool> HasUserReportedAsync(Guid reporterId, Guid targetId, string targetType)
        {
            return await _context.Reports.AnyAsync(r =>
                r.ReporterId == reporterId &&
                r.Type == targetType &&
                ((targetType == "User" && r.ReportedUserId == targetId) ||
                 (targetType == "Track" && r.TrackId == targetId) ||
                 (targetType == "Playlist" && r.PlaylistId == targetId) ||
                 (targetType == "Comment" && r.CommentId == targetId)));
        }

        #endregion

        #region Helper Methods

        private static Guid? ConvertToGuid(int? intValue)
        {
            // For now, we'll need to handle the int to Guid conversion
            // This is a temporary solution - ideally the ViewModels should use Guid
            if (!intValue.HasValue) return null;

            // Since we can't directly convert int to Guid, we'll create a deterministic Guid
            // based on the int value. This is not ideal for production.
            var bytes = new byte[16];
            var intBytes = BitConverter.GetBytes(intValue.Value);
            Array.Copy(intBytes, 0, bytes, 0, 4);
            return new Guid(bytes);
        }

        private ReportViewModel MapToViewModel(Report report)
        {
            var contentInfo = GetContentInfo(report);

            return new ReportViewModel
            {
                Id = report.Id,
                Type = report.Type,
                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status,
                CreatedAt = report.CreatedAt,
                Reporter = new UserViewModel
                {
                    Id = report.ReporterId,
                    Username = report.Reporter?.Username ?? "Unknown",
                    DisplayName = report.Reporter?.DisplayName ?? report.Reporter?.Username ?? "Unknown",
                    ProfileImageUrl = report.Reporter?.ProfileImageUrl
                },
                ReportedUser = report.ReportedUser != null ? new UserViewModel
                {
                    Id = report.ReportedUser.Id,
                    Username = report.ReportedUser.Username,
                    DisplayName = report.ReportedUser.DisplayName ?? report.ReportedUser.Username,
                    ProfileImageUrl = report.ReportedUser.ProfileImageUrl
                } : null,
                ContentType = contentInfo.Type,
                ContentId = contentInfo.Id,
                ContentTitle = contentInfo.Title
            };
        }

        private (string? Type, string? Id, string? Title) GetContentInfo(Report report)
        {
            if (report.TrackId.HasValue && report.Track != null)
            {
                return ("Track", report.TrackId.ToString(), report.Track.Title);
            }
            else if (report.PlaylistId.HasValue && report.Playlist != null)
            {
                return ("Playlist", report.PlaylistId.ToString(), report.Playlist.Title);
            }
            else if (report.CommentId.HasValue && report.Comment != null)
            {
                return ("Comment", report.CommentId.ToString(), report.Comment.Content?.Substring(0, Math.Min(50, report.Comment.Content.Length)) + "...");
            }
            else if (report.ReportedUserId.HasValue && report.ReportedUser != null)
            {
                return ("User", report.ReportedUserId.ToString(), report.ReportedUser.Username);
            }

            return (null, null, null);
        }

        #endregion
    }
}
