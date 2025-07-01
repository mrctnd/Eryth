using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.ViewModels;
using Eryth.Utilities;

namespace Eryth.Services
{
    // Kullanıcı dinleme geçmişi işlemleri için servis implementasyonu
    public class HistoryService : IHistoryService
    {
        private readonly ApplicationDbContext _context;

        public HistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserPlayHistory?> GetByIdAsync(Guid id)
        {
            return await _context.UserPlayHistories
                .Include(h => h.User)
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Include(h => h.Playlist)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<HistoryViewModel> GetHistoryViewModelAsync(Guid historyId)
        {
            var history = await GetByIdAsync(historyId);
            if (history == null)
                throw new InvalidOperationException("History entry not found");

            return HistoryViewModel.FromUserPlayHistory(history);
        }
        public async Task<IEnumerable<HistoryViewModel>> GetUserHistoryAsync(Guid userId, int page, int pageSize, DateTime? fromDate = null, DateTime? toDate = null, bool? validPlaysOnly = null)
        {
            var query = _context.UserPlayHistories
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Include(h => h.Playlist)
                .Where(h => h.UserId == userId);

            // Apply filters
            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            if (validPlaysOnly.HasValue && validPlaysOnly.Value)
                query = query.Where(h => h.PlayDurationInSeconds >= 30 || h.CompletionPercentage >= 50.0);

            var history = await query
                .OrderByDescending(h => h.PlayedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return history.Select(h => HistoryViewModel.FromUserPlayHistory(h));
        }

        public async Task<UserPlayHistory> RecordPlayAsync(PlayHistoryRecordViewModel model, Guid userId, string? ipAddress = null, string? userAgent = null)
        {
            // Validate track exists
            var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == model.TrackId);
            if (track == null)
                throw new InvalidOperationException("Track not found");

            // Validate playlist if specified
            if (model.PlaylistId.HasValue)
            {
                var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == model.PlaylistId.Value);
                if (playlist == null)
                    throw new InvalidOperationException("Playlist not found");
            }

            // Parse device info from user agent if not provided
            var deviceType = model.DeviceType;
            var deviceInfo = model.DeviceInfo;

            if (string.IsNullOrEmpty(deviceType) && !string.IsNullOrEmpty(userAgent))
            {
                deviceType = ParseDeviceTypeFromUserAgent(userAgent);
                deviceInfo = userAgent.Length > 200 ? userAgent.Substring(0, 200) : userAgent;
            }

            var playHistory = new UserPlayHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TrackId = model.TrackId,
                PlaylistId = model.PlaylistId,
                PlayDurationInSeconds = model.PlayDurationInSeconds,
                CompletionPercentage = model.CompletionPercentage,
                IsCompleted = model.IsCompleted,
                IsSkipped = model.IsSkipped,
                DeviceType = deviceType,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress?.Length > 45 ? ipAddress.Substring(0, 45) : ipAddress,
                PlayedAt = DateTime.UtcNow
            };

            _context.UserPlayHistories.Add(playHistory);

            // Update track play count if it's a valid play
            if (playHistory.IsValidPlay)
            {
                track.PlayCount++;
            }

            await _context.SaveChangesAsync();
            return playHistory;
        }

        public async Task<bool> DeleteHistoryEntryAsync(Guid historyId, Guid userId)
        {
            var history = await _context.UserPlayHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.UserId == userId);

            if (history == null)
                return false;

            _context.UserPlayHistories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearUserHistoryAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserPlayHistories.Where(h => h.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            var historyEntries = await query.ToListAsync();

            if (historyEntries.Any())
            {
                _context.UserPlayHistories.RemoveRange(historyEntries);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<HistoryStatsViewModel> GetUserStatsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserPlayHistories
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Where(h => h.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            var history = await query.ToListAsync();

            var totalPlays = history.Count;
            var validPlays = history.Count(h => h.IsValidPlay);
            var totalListeningTime = TimeSpan.FromSeconds(history.Sum(h => h.PlayDurationInSeconds));
            var uniqueTracks = history.Select(h => h.TrackId).Distinct().Count();
            var uniqueArtists = history.Where(h => h.Track?.ArtistId != null)
                                     .Select(h => h.Track!.ArtistId).Distinct().Count();
            var averageCompletion = history.Any() ? history.Average(h => h.CompletionPercentage) : 0;

            // Time-based stats
            var playsByHour = history.GroupBy(h => h.PlayedAt.Hour)
                                   .ToDictionary(g => g.Key.ToString().PadLeft(2, '0'), g => g.Count());

            var playsByDay = history.GroupBy(h => h.PlayedAt.DayOfWeek.ToString())
                                   .ToDictionary(g => g.Key, g => g.Count());

            var playsByMonth = history.GroupBy(h => h.PlayedAt.ToString("yyyy-MM"))
                                     .ToDictionary(g => g.Key, g => g.Count());

            // Device stats
            var playsByDevice = history.Where(h => !string.IsNullOrEmpty(h.DeviceType))
                                      .GroupBy(h => h.DeviceType!)
                                      .ToDictionary(g => g.Key, g => g.Count());

            // Top tracks
            var topTracks = await GetTopTracksAsync(userId, 10, fromDate, toDate);

            // Top artists
            var topArtists = await GetTopArtistsAsync(userId, 10, fromDate, toDate);

            // Recent plays
            var recentPlays = await GetRecentPlaysAsync(userId, 10);

            return new HistoryStatsViewModel
            {
                TotalPlays = totalPlays,
                ValidPlays = validPlays,
                TotalListeningTime = totalListeningTime,
                AverageSessionDuration = totalPlays > 0 ? TimeSpan.FromSeconds(totalListeningTime.TotalSeconds / totalPlays) : TimeSpan.Zero,
                UniqueTracksPlayed = uniqueTracks,
                UniqueArtistsPlayed = uniqueArtists,
                AverageCompletionRate = averageCompletion,
                PlaysByHour = playsByHour,
                PlaysByDay = playsByDay,
                PlaysByMonth = playsByMonth,
                PlaysByDeviceType = playsByDevice,
                TopTracks = topTracks.ToList(),
                TopArtists = topArtists.ToList(),
                RecentPlays = recentPlays.ToList()
            };
        }

        public async Task<IEnumerable<TrackPlayStats>> GetTopTracksAsync(Guid userId, int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserPlayHistories
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Where(h => h.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            var trackStats = await query
                .GroupBy(h => new { h.TrackId, h.Track })
                .Select(g => new TrackPlayStats
                {
                    TrackId = g.Key.TrackId,
                    TrackTitle = g.Key.Track.Title,
                    ArtistName = g.Key.Track.Artist.DisplayName,
                    CoverImageUrl = g.Key.Track.CoverImageUrl,
                    PlayCount = g.Count(),
                    TotalPlayTime = TimeSpan.FromSeconds(g.Sum(h => h.PlayDurationInSeconds)),
                    AverageCompletionPercentage = g.Average(h => h.CompletionPercentage)
                })
                .OrderByDescending(ts => ts.PlayCount)
                .Take(count)
                .ToListAsync();

            return trackStats;
        }

        public async Task<IEnumerable<ArtistPlayStats>> GetTopArtistsAsync(Guid userId, int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserPlayHistories
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Where(h => h.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            var artistStats = await query
                .Where(h => h.Track.Artist != null)
                .GroupBy(h => new { h.Track.ArtistId, h.Track.Artist })
                .Select(g => new ArtistPlayStats
                {
                    ArtistId = g.Key.ArtistId,
                    ArtistName = g.Key.Artist.DisplayName,
                    ProfileImageUrl = g.Key.Artist.ProfileImageUrl,
                    PlayCount = g.Count(),
                    UniqueTracksPlayed = g.Select(h => h.TrackId).Distinct().Count(),
                    TotalPlayTime = TimeSpan.FromSeconds(g.Sum(h => h.PlayDurationInSeconds))
                })
                .OrderByDescending(ats => ats.PlayCount)
                .Take(count)
                .ToListAsync();

            return artistStats;
        }

        public async Task<IEnumerable<HistoryViewModel>> GetRecentPlaysAsync(Guid userId, int count = 20)
        {
            var recentHistory = await _context.UserPlayHistories
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Include(h => h.Playlist)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PlayedAt)
                .Take(count)
                .ToListAsync();

            return recentHistory.Select(h => HistoryViewModel.FromUserPlayHistory(h));
        }

        public async Task<int> GetTotalPlayCountAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserPlayHistories.Where(h => h.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            return await query.CountAsync();
        }

        public async Task<TimeSpan> GetTotalListeningTimeAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.UserPlayHistories.Where(h => h.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(h => h.PlayedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(h => h.PlayedAt <= toDate.Value);

            var totalSeconds = await query.SumAsync(h => h.PlayDurationInSeconds);
            return TimeSpan.FromSeconds(totalSeconds);
        }

        public async Task<IEnumerable<HistoryViewModel>> SearchHistoryAsync(Guid userId, string searchTerm, int page, int pageSize)
        {
            var sanitizedTerm = SecurityHelper.SanitizeInput(searchTerm);

            var history = await _context.UserPlayHistories
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Include(h => h.Playlist)
                .Where(h => h.UserId == userId &&
                           (h.Track.Title.Contains(sanitizedTerm) ||
                            h.Track.Artist.DisplayName.Contains(sanitizedTerm) ||
                            (h.Playlist != null && h.Playlist.Title.Contains(sanitizedTerm))))
                .OrderByDescending(h => h.PlayedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return history.Select(h => HistoryViewModel.FromUserPlayHistory(h));
        }

        public async Task<bool> IsTrackPlayedByUserAsync(Guid trackId, Guid userId)
        {
            return await _context.UserPlayHistories
                .AnyAsync(h => h.TrackId == trackId && h.UserId == userId);
        }

        public async Task<DateTime?> GetLastPlayedTimeAsync(Guid trackId, Guid userId)
        {
            var lastPlay = await _context.UserPlayHistories
                .Where(h => h.TrackId == trackId && h.UserId == userId)
                .OrderByDescending(h => h.PlayedAt)
                .FirstOrDefaultAsync();

            return lastPlay?.PlayedAt;
        }

        private static string ParseDeviceTypeFromUserAgent(string userAgent)
        {
            var ua = userAgent.ToLowerInvariant();

            if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
                return "Mobile";
            else if (ua.Contains("tablet") || ua.Contains("ipad"))
                return "Tablet";
            else if (ua.Contains("windows") || ua.Contains("mac") || ua.Contains("linux"))
                return "Desktop";
            else
                return "Unknown";
        }
    }
}
