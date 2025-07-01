using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Eryth.Utilities;

namespace Eryth.Controllers
{
    [Authorize]
    public class HistoryController : BaseController
    {
        private readonly IHistoryService _historyService;
        private readonly ITrackService _trackService;
        private readonly IUserService _userService;

        public HistoryController(
            IHistoryService historyService,
            ITrackService trackService,
            IUserService userService)
        {
            _historyService = historyService;
            _trackService = trackService;
            _userService = userService;
        }

        // GET: History
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 50, DateTime? fromDate = null, DateTime? toDate = null, bool? validPlaysOnly = null)
        {
            try
            {
                // Rate limiting: 30 requests per 5 minutes
                if (!await CheckRateLimitAsync("history_view", 30, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var userId = GetRequiredUserId();
                var history = await _historyService.GetUserHistoryAsync(userId, page, pageSize, fromDate, toDate, validPlaysOnly);
                var totalCount = await _historyService.GetTotalPlayCountAsync(userId, fromDate, toDate);

                var viewModel = new HistoryListViewModel
                {
                    History = history.ToList(),
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ValidPlaysOnly = validPlaysOnly
                };

                // Add statistics
                var stats = await _historyService.GetUserStatsAsync(userId, fromDate, toDate);
                viewModel.TotalPlays = stats.TotalPlays;
                viewModel.ValidPlays = stats.ValidPlays;
                viewModel.TotalListeningTime = stats.TotalListeningTime;
                viewModel.UniqueTracksPlayed = stats.UniqueTracksPlayed;
                viewModel.UniqueArtistsPlayed = stats.UniqueArtistsPlayed;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading history: {ex.Message}");
            }
        }

        // GET: History/Stats
        [HttpGet("Stats")]
        public async Task<IActionResult> Stats(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // Rate limiting: 20 requests per 5 minutes
                if (!await CheckRateLimitAsync("history_stats", 20, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var userId = GetRequiredUserId();
                var stats = await _historyService.GetUserStatsAsync(userId, fromDate, toDate);

                return View(stats);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading statistics: {ex.Message}");
            }
        }

        // POST: History/Record
        [HttpPost("Record")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record([FromBody] PlayHistoryRecordViewModel model)
        {
            try
            {
                // Rate limiting: 100 records per 10 minutes (for active listening)
                if (!await CheckRateLimitAsync("history_record", 100, TimeSpan.FromMinutes(10)))
                {
                    return StatusCode(429, new { success = false, message = "Too many play records. Please slow down." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Invalid play data", errors = ModelState });
                }
                var userId = GetRequiredUserId();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var history = await _historyService.RecordPlayAsync(model, userId, ipAddress, userAgent);

                return Json(new
                {
                    success = true,
                    message = "Play recorded successfully",
                    historyId = history.Id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error recording play: {ex.Message}"
                });
            }
        }

        // GET: History/TopTracks
        [HttpGet("TopTracks")]
        public async Task<IActionResult> TopTracks(int count = 20, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // Rate limiting: 20 requests per 5 minutes
                if (!await CheckRateLimitAsync("history_top_tracks", 20, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var userId = GetRequiredUserId();
                var topTracks = await _historyService.GetTopTracksAsync(userId, count, fromDate, toDate);

                return Json(new { success = true, tracks = topTracks });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: History/TopArtists
        [HttpGet("TopArtists")]
        public async Task<IActionResult> TopArtists(int count = 20, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // Rate limiting: 20 requests per 5 minutes
                if (!await CheckRateLimitAsync("history_top_artists", 20, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var userId = GetRequiredUserId();
                var topArtists = await _historyService.GetTopArtistsAsync(userId, count, fromDate, toDate);

                return Json(new { success = true, artists = topArtists });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: History/Recent
        [HttpGet("Recent")]
        public async Task<IActionResult> Recent(int count = 20)
        {
            try
            {
                // Rate limiting: 30 requests per 5 minutes
                if (!await CheckRateLimitAsync("history_recent", 30, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }
                var userId = GetRequiredUserId();
                var recentPlays = await _historyService.GetRecentPlaysAsync(userId, count);

                return Json(new { success = true, plays = recentPlays });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: History/Delete/{id}
        [HttpPost("Delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Rate limiting: 20 deletes per 10 minutes
                if (!await CheckRateLimitAsync("history_delete", 20, TimeSpan.FromMinutes(10)))
                {
                    return StatusCode(429, new { success = false, message = "Too many delete attempts. Please wait before trying again." });
                }
                var userId = GetRequiredUserId();
                var success = await _historyService.DeleteHistoryEntryAsync(id, userId);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "History entry not found or you don't have permission to delete it." });
                }

                return Json(new
                {
                    success = true,
                    message = "History entry deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error deleting history entry: {ex.Message}"
                });
            }
        }

        // POST: History/Clear
        [HttpPost("Clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // Rate limiting: 5 clear operations per hour
                if (!await CheckRateLimitAsync("history_clear", 5, TimeSpan.FromHours(1)))
                {
                    return StatusCode(429, new { success = false, message = "Too many clear attempts. Please wait before trying again." });
                }
                var userId = GetRequiredUserId();
                var success = await _historyService.ClearUserHistoryAsync(userId, fromDate, toDate);

                var message = fromDate.HasValue || toDate.HasValue
                    ? "Selected history cleared successfully"
                    : "All history cleared successfully";

                return Json(new
                {
                    success = true,
                    message = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error clearing history: {ex.Message}"
                });
            }
        }        // GET: History/SearchHistory
        [HttpGet("SearchHistory")]
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 20)
        {
            try
            {
                // Rate limiting: 30 searches per 5 minutes
                if (!await CheckRateLimitAsync("history_search", 30, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }

                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Search query is required");
                }

                // Sanitize search query
                query = SecurityHelper.SanitizeInput(query); var userId = GetRequiredUserId();
                var searchResults = await _historyService.SearchHistoryAsync(userId, query, page, pageSize);

                return Json(new
                {
                    success = true,
                    results = searchResults,
                    query = query,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: History/Export
        [HttpGet("Export")]
        public async Task<IActionResult> Export(DateTime? fromDate = null, DateTime? toDate = null, string format = "json")
        {
            try
            {
                // Rate limiting: 3 exports per hour
                if (!await CheckRateLimitAsync("history_export", 3, TimeSpan.FromHours(1)))
                {
                    return StatusCode(429, "Too many export attempts. Please wait before trying again.");
                }
                var userId = GetRequiredUserId();
                var history = await _historyService.GetUserHistoryAsync(userId, 1, 10000, fromDate, toDate); // Max 10k records

                var filename = $"eryth_history_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (format.ToLowerInvariant() == "csv")
                {
                    var csv = ConvertHistoryToCsv(history);
                    return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"{filename}.csv");
                }
                else
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(history, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"{filename}.json");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error exporting history: {ex.Message}");
            }
        }

        // GET: History/IsPlayed/{trackId}
        [HttpGet("IsPlayed/{trackId:guid}")]
        public async Task<IActionResult> IsPlayed(Guid trackId)
        {
            try
            {
                // Rate limiting: 60 requests per 5 minutes
                if (!await CheckRateLimitAsync("history_check", 60, TimeSpan.FromMinutes(5)))
                {
                    return StatusCode(429);
                }

                var userId = GetRequiredUserId();
                var isPlayed = await _historyService.IsTrackPlayedByUserAsync(trackId, userId);
                var lastPlayed = await _historyService.GetLastPlayedTimeAsync(trackId, userId);

                return Json(new
                {
                    success = true,
                    isPlayed = isPlayed,
                    lastPlayed = lastPlayed
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static string ConvertHistoryToCsv(IEnumerable<HistoryViewModel> history)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("PlayedAt,TrackTitle,ArtistName,PlayDuration,CompletionPercentage,IsCompleted,IsSkipped,DeviceType,PlaySource");

            foreach (var item in history)
            {
                csv.AppendLine($"{item.PlayedAt:yyyy-MM-dd HH:mm:ss}," +
                              $"\"{item.TrackTitle.Replace("\"", "\"\"")}\"," +
                              $"\"{item.ArtistName.Replace("\"", "\"\"")}\"," +
                              $"{item.FormattedPlayDuration}," +
                              $"{item.CompletionPercentage:F2}," +
                              $"{item.IsCompleted}," +
                              $"{item.IsSkipped}," +
                              $"{item.DeviceType ?? ""}," +
                              $"{item.PlaySource}");
            }

            return csv.ToString();
        }
    }
}
