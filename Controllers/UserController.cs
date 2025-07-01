using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    // Kullanıcı profil ve takip işlemlerini yöneten controller
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IFollowService _followService;
        private readonly ITrackService _trackService;
        private readonly IPlaylistService _playlistService;
        private readonly ILikeService _likeService;

        public UserController(
            IUserService userService,
            IFollowService followService,
            ITrackService trackService,
            IPlaylistService playlistService,
            ILikeService likeService,
            IMemoryCache cache) : base(cache)
        {
            _userService = userService;
            _followService = followService;
            _trackService = trackService;
            _playlistService = playlistService;
            _likeService = likeService;
        }
        
        // Kullanıcı profil sayfası
        [AllowAnonymous]
        public async Task<IActionResult> Profile(string? username, string tab = "all")
        {
            try
            {
                // Debug için log
                System.Diagnostics.Debug.WriteLine($"Profile called with username: '{username}', tab: '{tab}'");
                System.Diagnostics.Debug.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                System.Diagnostics.Debug.WriteLine($"User.Identity.Name: '{User.Identity?.Name}'");

                // Eğer username boşsa, mevcut kullanıcının profilini göster
                if (string.IsNullOrEmpty(username))
                {
                    username = User.Identity?.Name;
                    System.Diagnostics.Debug.WriteLine($"Username was empty, using User.Identity.Name: '{username}'");

                    if (string.IsNullOrEmpty(username))
                    {
                        System.Diagnostics.Debug.WriteLine("No username found, redirecting to Home");
                        TempData["Error"] = "Profil görüntülenemiyor";
                        return RedirectToAction("Index", "Home");
                    }
                }

                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                System.Diagnostics.Debug.WriteLine($"Current user ID: {currentUserId}");

                var user = await _userService.GetUserByUsernameAsync(username);
                System.Diagnostics.Debug.WriteLine($"User found: {user != null}, Username: {user?.Username}");

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"User not found for username: '{username}'");
                    TempData["Error"] = "Kullanıcı bulunamadı";
                    return RedirectToAction("Index", "Home");
                }

                var profileViewModel = UserProfileViewModel.FromUser(user, currentUserId);
                ViewBag.ActiveTab = tab;

                System.Diagnostics.Debug.WriteLine($"Profile view model created successfully, returning view");
                return View(profileViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Profile action: {ex.Message}");
                TempData["Error"] = "Profil yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // Profil düzenleme sayfası
        [HttpGet]
        public IActionResult Settings()
        {
            // Ana ayar sayfası olduğu için AccountSettings sayfasına yönlendir
            return RedirectToAction(nameof(AccountSettings));
        }

        // Profil düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(UserSettingsViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("update-profile", 5, TimeSpan.FromMinutes(30)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla profil güncelleme denemesi. 30 dakika bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Profil resmi dosya kontrolü
            if (model.ProfileImage != null && !IsValidFileUpload(model.ProfileImage, new[] { ".jpg", ".jpeg", ".png", ".webp" }, 2 * 1024 * 1024)) // 2MB limit
            {
                ModelState.AddModelError("ProfileImage", "Geçersiz resim formatı veya boyutu. Desteklenen formatlar: JPG, PNG, WEBP (Max: 2MB)");
                return View(model);
            }

            try
            {
                model.DisplayName = SanitizeInput(model.DisplayName);
                model.Bio = SanitizeInput(model.Bio);
                model.Location = SanitizeInput(model.Location);
                model.Website = SanitizeInput(model.Website); 
                var userId = GetRequiredUserId();
                var success = await _userService.UpdateUserAsync(userId, model);

                if (success)
                {
                    // Modeli yenilemek için güncellenmiş kullanıcı verilerini al
                    var updatedUser = await _userService.GetByIdAsync(userId);
                    if (updatedUser != null)
                    {
                        model = UserSettingsViewModel.FromUser(updatedUser);
                    }

                    // Yönlendirmeyi önlemek için başarı mesajını TempData yerine ViewBag'e ekle
                    ViewBag.SuccessMessage = "Profil başarıyla güncellendi";
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Profil güncellenirken bir hata oluştu");
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Profil güncellenirken bir hata oluştu");
                return View(model);
            }
        }

        // Hesap ayarları sayfası
        [HttpGet]
        public async Task<IActionResult> AccountSettings()
        {
            try
            {
                var userId = GetRequiredUserId();
                var user = await _userService.GetByIdAsync(userId);

                if (user == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var model = UserAccountSettingsViewModel.FromUser(user);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Ayarlar yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Hesap ayarları POST işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountSettings(UserAccountSettingsViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("update-account-settings", 5, TimeSpan.FromMinutes(30)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla ayar güncelleme denemesi. 30 dakika bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetRequiredUserId();

                // Hesap silme kontrolü
                if (model.RequestAccountDeletion)
                {
                    if (string.IsNullOrEmpty(model.DeleteAccountPassword))
                    {
                        ModelState.AddModelError("DeleteAccountPassword", "Hesabınızı silmek için şifrenizi girmelisiniz");
                        return View(model);
                    }

                    var user = await _userService.GetByIdAsync(userId);

                    if (user == null || !Utilities.SecurityHelper.VerifyPassword(model.DeleteAccountPassword, user.PasswordHash))
                    {
                        ModelState.AddModelError("DeleteAccountPassword", "Şifre hatalı");
                        return View(model);
                    }                    // Hesabı sil
                    var deleteResult = await _userService.DeleteUserAccountAsync(userId);
                    if (deleteResult)
                    {
                        // Kullanıcıyı çıkış yap
                        var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
                        await authService.LogoutAsync();

                        TempData["Success"] = "Your account and all associated data have been permanently deleted. Thank you for using Eryth.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız silinirken bir hata oluştu. Lütfen tekrar deneyin veya destek ile iletişime geçin.");
                        return View(model);
                    }
                }

                // Şifre değişikliği kontrolü
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (string.IsNullOrEmpty(model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "Yeni şifre belirlemek için mevcut şifrenizi girmelisiniz");
                        return View(model);
                    }

                    var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
                    var passwordChangeResult = await authService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                    if (!passwordChangeResult)
                    {
                        ModelState.AddModelError("CurrentPassword", "Mevcut şifre hatalı");
                        return View(model);
                    }
                }
                // Profil fotoğrafı yükleme kontrolü
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    // Dosya formatı ve boyut kontrolü
                    if (!IsValidFileUpload(model.ProfileImage, new[] { ".jpg", ".jpeg", ".png", ".webp" }, 2 * 1024 * 1024)) // 2MB limit
                    {
                        ModelState.AddModelError("ProfileImage", "Geçersiz resim formatı veya boyutu. Desteklenen formatlar: JPG, PNG, WEBP (Max: 2MB)");
                        return View(model);
                    }

                    try
                    {
                        // Basit dosya yükleme - wwwroot/uploads/profiles klasörüne kaydet
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(model.ProfileImage.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ProfileImage.CopyToAsync(stream);
                        }

                        var profileImageUrl = $"/uploads/profiles/{fileName}";
                        await _userService.UploadProfileImageAsync(userId, profileImageUrl);
                    }
                    catch
                    {
                        ModelState.AddModelError("ProfileImage", "Profil fotoğrafı yüklenirken bir hata oluştu");
                        return View(model);
                    }
                }

                // Banner fotoğrafı yükleme kontrolü
                if (model.BannerImage != null && model.BannerImage.Length > 0)
                {
                    // Dosya formatı ve boyut kontrolü
                    if (!IsValidFileUpload(model.BannerImage, new[] { ".jpg", ".jpeg", ".png", ".webp" }, 5 * 1024 * 1024)) // 5MB limit
                    {
                        ModelState.AddModelError("BannerImage", "Geçersiz resim formatı veya boyutu. Desteklenen formatlar: JPG, PNG, WEBP (Max: 5MB)");
                        return View(model);
                    }

                    try
                    {
                        // Banner resmi için uploads/banners klasörüne kaydet
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "banners");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(model.BannerImage.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.BannerImage.CopyToAsync(stream);
                        }

                        var bannerImageUrl = $"/uploads/banners/{fileName}";
                        await _userService.UploadBannerImageAsync(userId, bannerImageUrl);
                    }
                    catch
                    {
                        ModelState.AddModelError("BannerImage", "Banner fotoğrafı yüklenirken bir hata oluştu");
                        return View(model);
                    }
                }

                model.DisplayName = SanitizeInput(model.DisplayName);
                model.Bio = SanitizeInput(model.Bio);
                model.Location = SanitizeInput(model.Location);
                model.Website = SanitizeInput(model.Website);
                var success = await _userService.UpdateUserAccountAsync(userId, model);

                if (success)
                {
                    // Modeli yenilemek için güncellenmiş kullanıcı verisini al
                    var updatedUser = await _userService.GetByIdAsync(userId);
                    if (updatedUser != null)
                    {
                        model = UserAccountSettingsViewModel.FromUser(updatedUser);
                    }

                    // Yönlendirmeyi önlemek için başarı mesajını TempData yerine ViewBag'e ekle
                    ViewBag.SuccessMessage = "Hesap ayarları başarıyla güncellendi";
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Ayarlar güncellenirken bir hata oluştu");
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ayarlar güncellenirken bir hata oluştu");
                return View(model);
            }
        }

        // 2FA Toggle
        [HttpPost]
        public async Task<IActionResult> ToggleTwoFactor()
        {
            try
            {
                var userId = GetRequiredUserId();
                var user = await _userService.GetByIdAsync(userId);

                if (user == null)
                {
                    return ErrorResult("Kullanıcı bulunamadı");
                }

                // 2FA'u aç
                user.IsTwoFactorEnabled = !user.IsTwoFactorEnabled;

                // 2FA açıksa, gizle
                if (user.IsTwoFactorEnabled && string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    user.TwoFactorSecret = Utilities.SecurityHelper.GenerateTwoFactorSecret();
                }

                var settingsModel = UserSettingsViewModel.FromUser(user);
                await _userService.UpdateUserAsync(userId, settingsModel);

                return SuccessResult(new { isEnabled = user.IsTwoFactorEnabled });
            }
            catch (Exception)
            {
                return ErrorResult("İşlem gerçekleştirilemedi");
            }
        }

        // Kullanıcı takip etme/etmeme işlemi
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(Guid userId)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("toggle-follow", 20, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla takip denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var currentUserId = GetRequiredUserId();

                if (currentUserId == userId)
                {
                    return ErrorResult("Kendinizi takip edemezsiniz");
                }

                var isFollowing = await _followService.IsFollowingAsync(currentUserId, userId);

                bool success;
                if (isFollowing)
                {
                    success = await _followService.UnfollowUserAsync(currentUserId, userId);
                }
                else
                {
                    success = await _followService.FollowUserAsync(currentUserId, userId);
                }

                if (success)
                {
                    var followerCount = await _followService.GetFollowerCountAsync(userId);
                    return SuccessResult(new { isFollowing = !isFollowing, followerCount });
                }

                return ErrorResult("İşlem gerçekleştirilemedi");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("İşlem gerçekleştirilemedi");
            }
        }

        // Takipçiler listesi
        public async Task<IActionResult> Followers(string username, int page = 1)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı";
                    return RedirectToAction("Index", "Home");
                }
                var (validPage, pageSize) = ValidatePagination(page);
                var followers = await _followService.GetFollowersAsync(user.Id, validPage, pageSize);

                ViewBag.Username = username;
                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = followers.Count() == pageSize;

                return View(followers);
            }
            catch (Exception)
            {
                TempData["Error"] = "Takipçiler yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Takip edilenler listesi
        public async Task<IActionResult> Following(string username, int page = 1)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı";
                    return RedirectToAction("Index", "Home");
                }
                var (validPage, pageSize) = ValidatePagination(page);
                var following = await _followService.GetFollowingAsync(user.Id, validPage, pageSize);

                ViewBag.Username = username;
                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = following.Count() == pageSize;

                return View(following);
            }
            catch (Exception)
            {
                TempData["Error"] = "Takip edilenler yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Beğenilen müzikler
        public async Task<IActionResult> LikedTracks(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var tracks = await _likeService.GetLikedTracksAsync(userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = tracks.Count() == pageSize;

                return View(tracks);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Beğenilen müzikler yüklenirken bir hata oluştu";
                return View(new List<TrackViewModel>());
            }
        }

        // Kullanıcı arama sonuçları
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("user-search", 30, TimeSpan.FromMinutes(5)))
            {
                TempData["Error"] = "Çok fazla arama denemesi. 5 dakika bekleyip tekrar deneyin.";
                return View(new List<UserViewModel>());
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<UserViewModel>());
            }

            try
            {
                query = SanitizeInput(query);

                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var (validPage, pageSize) = ValidatePagination(page);

                var users = await _userService.SearchUsersAsync(query, currentUserId, validPage, pageSize);

                ViewBag.Query = query;
                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = users.Count() == pageSize;

                return View(users);
            }
            catch (Exception)
            {
                TempData["Error"] = "Arama yapılırken bir hata oluştu";
                return View(new List<UserViewModel>());
            }
        }

        // Kullanıcı yorumları sayfası        
        public async Task<IActionResult> Comments(string? username, int page = 1)
        {
            try
            {                // Add no-cache headers to prevent caching issues
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                var currentUserId = GetCurrentUserId() ?? Guid.Empty;

                // Eğer username boşsa, mevcut kullanıcının yorumlarını göster
                if (string.IsNullOrEmpty(username))
                {
                    username = User.Identity?.Name;
                    if (string.IsNullOrEmpty(username))
                    {
                        TempData["Error"] = "Lütfen giriş yapın";
                        return RedirectToAction("Index", "Home");
                    }
                }

                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı";
                    return RedirectToAction("Index", "Home");
                }

                var (validPage, pageSize) = ValidatePagination(page, 20);
                var commentService = HttpContext.RequestServices.GetRequiredService<ICommentService>();
                var comments = await commentService.GetUserCommentsAsync(user.Id, validPage, pageSize);

                ViewBag.Username = username;
                ViewBag.DisplayName = user.DisplayName ?? username;
                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = comments.Count() == pageSize;

                return View(comments);
            }
            catch (Exception)
            {
                TempData["Error"] = "Yorumlar yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Artist bilgilerini getir
          [HttpGet]
        public async Task<IActionResult> GetArtistInfo(Guid artistId)
        {
            try
            {
                // Debug log
                System.Diagnostics.Debug.WriteLine($"GetArtistInfo called with artistId: {artistId}");
                
                if (artistId == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine("ArtistId is empty GUID");
                    return Json(new { success = false, message = "Invalid artist ID" });
                }

                var user = await _userService.GetByIdAsync(artistId);
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"User not found for artistId: {artistId}");
                    return Json(new { success = false, message = "Artist not found" });
                }

                // Artist'in track sayısını al
                var trackCount = await _trackService.GetUserTrackCountAsync(artistId);
                var followerCount = await _followService.GetFollowerCountAsync(artistId);

                var artistInfo = new
                {
                    success = true,
                    displayName = user.DisplayName ?? user.Username,
                    profileImageUrl = user.ProfileImageUrl,
                    trackCount = trackCount,
                    followerCount = followerCount
                };

                return Json(artistInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetArtistInfo: {ex.Message}");
                return Json(new { success = false, message = "Error loading artist info: " + ex.Message });
            }
        }
    }
}

