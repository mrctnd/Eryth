using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Eryth.Models.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITrackService _trackService;
        private readonly IPlaylistService _playlistService;
        private readonly ICommentService _commentService;
        private readonly INotificationService _notificationService;
        private readonly IReportService _reportService;

        public AdminController(
            IUserService userService,
            ITrackService trackService,
            IPlaylistService playlistService,
            ICommentService commentService,
            INotificationService notificationService,
            IReportService reportService,
            IMemoryCache cache) : base(cache)
        {
            _userService = userService;
            _trackService = trackService;
            _playlistService = playlistService;
            _commentService = commentService;
            _notificationService = notificationService;
            _reportService = reportService;
        }

        // Yönetim Paneli
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = GetRequiredUserId();
                var currentUser = await _userService.GetByIdAsync(currentUserId);

                if (currentUser == null || (currentUser.Role != UserRole.Admin && currentUser.Role != UserRole.Moderator))
                {
                    return Forbid();
                }

                var dashboardViewModel = new AdminDashboardViewModel
                {
                    // Kullanıcı İstatistikleri
                    TotalUsers = await _userService.GetTotalUserCountAsync(),
                    NewUsersToday = await _userService.GetNewUserCountAsync(DateTime.Today),
                    ActiveUsersThisWeek = await _userService.GetActiveUserCountAsync(DateTime.Today.AddDays(-7)),

                    // İçerik İstatistikleri
                    TotalTracks = await _trackService.GetTotalTrackCountAsync(),
                    TracksUploadedToday = await _trackService.GetNewTrackCountAsync(DateTime.Today),
                    TotalPlaylists = await _playlistService.GetTotalPlaylistCountAsync(),

                    // Moderasyon İstatistikleri
                    PendingReports = await _reportService.GetPendingReportCountAsync(),
                    SuspendedUsers = await _userService.GetSuspendedUserCountAsync(),
                    
                    // Son Etkinlik
                    RecentUsers = await _userService.GetRecentUsersAsync(10),
                    RecentTracks = await _trackService.GetRecentTracksForAdminAsync(10),
                    RecentReports = await _reportService.GetRecentReportsAsync(10),

                    CurrentUserRole = currentUser.Role
                };

                return View(dashboardViewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Kullanıcı Yönetimi
        [HttpGet]
        public async Task<IActionResult> Users(int page = 1, string? search = null, UserRole? role = null, AccountStatus? status = null)
        {
            try
            {
                var (validPage, pageSize) = ValidatePagination(page, 50);
                var users = await _userService.GetUsersForAdminAsync(validPage, pageSize, search, role, status);

                var viewModel = new AdminUserListViewModel
                {
                    Users = users.ToList(),
                    CurrentPage = validPage,
                    PageSize = pageSize,
                    SearchQuery = search,
                    SelectedRole = role,
                    SelectedStatus = status,
                    HasNextPage = users.Count() == pageSize
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Kullanıcılar yüklenirken bir hata oluştu";
                return View(new AdminUserListViewModel());
            }
        }

        // Kullanıcı Detayları
        [HttpGet]
        public async Task<IActionResult> UserDetails(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı";
                    return RedirectToAction(nameof(Users));
                }
                var viewModel = new AdminUserDetailsViewModel
                {
                    User = await _userService.GetUserViewModelAsync(id, GetRequiredUserId()),
                    UserTracks = await _trackService.GetUserTracksAsync(id, GetRequiredUserId(), 1, 10),
                    UserPlaylists = await _playlistService.GetUserPlaylistsAsync(id, GetRequiredUserId(), 1, 10),
                    UserReports = await _reportService.GetUserReportsAsync(id),
                    ReportsAgainstUser = await _reportService.GetReportsAgainstUserAsync(id),
                    LoginHistory = await _userService.GetUserLoginHistoryAsync(id, 1, 20)
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Kullanıcı detayları yüklenirken bir hata oluştu";
                return RedirectToAction(nameof(Users));
            }
        }

        // Kullanıcı Rolünü Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, UserRole newRole, string? reason = null)
        {
            try
            {
                var currentUserId = GetRequiredUserId();
                var success = await _userService.UpdateUserRoleAsync(userId, newRole, currentUserId, reason);

                if (success)
                {
                    TempData["Success"] = "Kullanıcı rolü başarıyla güncellendi";
                }
                else
                {
                    TempData["Error"] = "Rol güncellenirken bir hata oluştu";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Rol güncellenirken bir hata oluştu";
            }

            return RedirectToAction(nameof(UserDetails), new { id = userId });
        }

        // Kullanıcı Durumunu Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserStatus(Guid userId, AccountStatus newStatus, string? reason = null)
        {
            try
            {
                var currentUserId = GetRequiredUserId();
                var success = await _userService.UpdateUserStatusAsync(userId, newStatus, currentUserId, reason);

                if (success)
                {
                    TempData["Success"] = "Kullanıcı durumu başarıyla güncellendi";

                    // Durum değişikliği hakkında kullanıcıya bildirim gönder
                    if (newStatus == AccountStatus.Suspended || newStatus == AccountStatus.Banned)
                    {
                        // await _notificationService.SendModerationNotificationAsync(userId, newStatus, reason);
                    }
                }
                else
                {
                    TempData["Error"] = "Durum güncellenirken bir hata oluştu";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Durum güncellenirken bir hata oluştu";
            }

            return RedirectToAction(nameof(UserDetails), new { id = userId });
        }

        // İçerik Yönetimi
        [HttpGet]
        public async Task<IActionResult> Content(int page = 1, string type = "tracks", string? search = null, TrackStatus? status = null)
        {
            try
            {
                var (validPage, pageSize) = ValidatePagination(page, 50);

                var viewModel = new AdminContentListViewModel
                {
                    ContentType = type,
                    CurrentPage = validPage,
                    PageSize = pageSize,
                    SearchQuery = search,
                    SelectedStatus = status,
                    HasNextPage = false
                };

                switch (type.ToLower())
                {
                    case "tracks":
                        var tracks = await _trackService.GetTracksForAdminAsync(validPage, pageSize, search, status);
                        viewModel.Tracks = tracks.ToList();
                        viewModel.HasNextPage = tracks.Count() == pageSize;
                        break;
                    case "playlists":
                        var playlists = await _playlistService.GetPlaylistsForAdminAsync(validPage, pageSize, search);
                        viewModel.Playlists = playlists.ToList();
                        viewModel.HasNextPage = playlists.Count() == pageSize;
                        break;
                    case "comments":
                        // YAPILACAK ŞİMDİLİK DEVRE DIŞI
                        // var comments = await _commentService.GetCommentsForAdminAsync(validPage, pageSize, search);
                        // viewModel.Comments = comments.ToList();
                        // viewModel.HasNextPage = comments.Count() == pageSize;
                        viewModel.Comments = new List<CommentViewModel>(); // Temporary empty list
                        break;
                }

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "İçerik yüklenirken bir hata oluştu";
                return View(new AdminContentListViewModel());
            }
        }

        // Rapor Yönetimi
        [HttpGet]
        public async Task<IActionResult> Reports(int page = 1, string? status = null, string? type = null)
        {
            try
            {
                var (validPage, pageSize) = ValidatePagination(page, 50);
                var reports = await _reportService.GetReportsForAdminAsync(validPage, pageSize, status, type);

                var viewModel = new AdminReportListViewModel
                {
                    Reports = reports.ToList(),
                    CurrentPage = validPage,
                    PageSize = pageSize,
                    SelectedStatus = status,
                    SelectedType = type,
                    HasNextPage = reports.Count() == pageSize
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Raporlar yüklenirken bir hata oluştu";
                return View(new AdminReportListViewModel());
            }
        }

        // Raporu İşle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleReport(Guid reportId, string action, string? notes = null)
        {
            try
            {
                var currentUserId = GetRequiredUserId();
                var success = await _reportService.HandleReportAsync(reportId, action, currentUserId, notes);

                if (success)
                {
                    TempData["Success"] = "Rapor başarıyla işlendi";
                }
                else
                {
                    TempData["Error"] = "Rapor işlenirken bir hata oluştu";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Rapor işlenirken bir hata oluştu";
            }

            return RedirectToAction(nameof(Reports));
        }

        // Sistem Kayıtları        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Logs(int page = 1, string? level = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var (validPage, pageSize) = ValidatePagination(page, 100);

                var viewModel = new AdminLogsViewModel
                {
                    CurrentPage = validPage,
                    PageSize = pageSize,
                    SelectedLevel = level,
                    FromDate = fromDate,
                    ToDate = toDate,
                    HasNextPage = false,
                    Logs = new List<LogEntryViewModel>() // Placeholder
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Loglar yüklenirken bir hata oluştu";
                return View(new AdminLogsViewModel());
            }
        }

        // Site Ayarları
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Settings()
        {
            try
            {
                var viewModel = new AdminSettingsViewModel
                {
                    RegistrationEnabled = true,
                    MaintenanceMode = false,
                    MaxFileSize = 50 * 1024 * 1024, // 50MB
                    AllowedFileTypes = ".mp3,.wav,.flac,.aac,.ogg"
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Ayarlar yüklenirken bir hata oluştu";
                return View(new AdminSettingsViewModel());
            }
        }

        // Site Ayarlarını Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Settings(AdminSettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                TempData["Success"] = "Ayarlar başarıyla güncellendi";
                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Ayarlar güncellenirken bir hata oluştu";
                return View(model);
            }
        }

        // Analitik
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Analytics(string period = "week")
        {
            try
            {
                var viewModel = new AdminAnalyticsViewModel
                {
                    Period = period,
                    UserGrowthData = new List<AnalyticsDataPoint>(),
                    ContentGrowthData = new List<AnalyticsDataPoint>(),
                    ActivityData = new List<AnalyticsDataPoint>()
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Analitik veriler yüklenirken bir hata oluştu";
                return View(new AdminAnalyticsViewModel());
            }
        }
    }
}
