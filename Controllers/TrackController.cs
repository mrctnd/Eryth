using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Eryth.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace Eryth.Controllers
{
    // Müzik parçası işlemlerini yöneten controller
    [Authorize]
    public class TrackController : BaseController
    {
        private readonly ITrackService _trackService;
        private readonly ILikeService _likeService;
        private readonly ILogger<TrackController> _logger;

        public TrackController(ITrackService trackService, ILikeService likeService, IMemoryCache cache, ILogger<TrackController> logger) : base(cache)
        {
            _trackService = trackService;
            _likeService = likeService;
            _logger = logger;
        }

        // Müzik detay sayfası
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var track = await _trackService.GetTrackViewModelAsync(id, currentUserId);

                if (track == null)
                {
                    TempData["Error"] = "Müzik bulunamadı";
                    return RedirectToAction("Index", "Home");
                }
                return View(track);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track details for {TrackId}", id);
                TempData["Error"] = "Müzik yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Müzik yükleme sayfası
        [HttpGet]
        public IActionResult Upload()
        {
            return View(new UploadTrackViewModel());
        }

        // Müzik yükleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadTrackViewModel model)
        {
            // Rate limiting kontrolü - günde 10 müzik yükleme limiti
            if (IsRateLimited("upload-track", 10, TimeSpan.FromDays(1)))
            {
                ModelState.AddModelError(string.Empty, "Günlük müzik yükleme limitinizi aştınız. Yarın tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Dosya güvenlik kontrolü
            if (model.AudioFile != null && !IsValidFileUpload(model.AudioFile, AppConstants.AllowedAudioExtensions, AppConstants.MaxAudioFileSizeBytes))
            {
                ModelState.AddModelError("AudioFile", "Geçersiz dosya formatı veya boyutu. Desteklenen formatlar: MP3, WAV, FLAC, M4A (Max: 50MB)");
                return View(model);
            }

            if (model.CoverImage != null && !IsValidFileUpload(model.CoverImage, AppConstants.AllowedImageExtensions, AppConstants.MaxImageFileSizeBytes))
            {
                ModelState.AddModelError("CoverImage", "Geçersiz resim formatı veya boyutu. Desteklenen formatlar: JPG, PNG, WEBP (Max: 5MB)");
                return View(model);
            }

            try
            {
                model.Title = SanitizeInput(model.Title);
                model.Description = SanitizeInput(model.Description);
                model.Tags = SanitizeInput(model.Tags);

                var userId = GetRequiredUserId();
                var track = await _trackService.CreateTrackAsync(model, userId);

                TempData["Success"] = "Müzik başarıyla yüklendi";
                return RedirectToAction(nameof(Details), new { id = track.Id });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading track");
                ModelState.AddModelError(string.Empty, "Müzik yüklenirken bir hata oluştu");
                return View(model);
            }
        }

        // Müzik düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = GetRequiredUserId();
                var track = await _trackService.GetByIdAsync(id);

                if (track == null || track.ArtistId != userId)
                {
                    TempData["Error"] = "Müzik bulunamadı veya düzenleme yetkiniz yok";
                    return RedirectToAction("Index", "Home");
                }

                var model = EditTrackViewModel.FromTrack(track);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track edit page for {TrackId}", id);
                TempData["Error"] = "Müzik yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Müzik düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTrackViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("edit-track", 20, TimeSpan.FromHours(1)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla düzenleme denemesi. 1 saat bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.Title = SanitizeInput(model.Title);
                model.Description = SanitizeInput(model.Description);
                model.Tags = SanitizeInput(model.Tags);

                var userId = GetRequiredUserId();
                var success = await _trackService.UpdateTrackAsync(model.Id, model, userId);

                if (success)
                {
                    TempData["Success"] = "Müzik başarıyla güncellendi";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                TempData["Error"] = "Müzik güncellenirken bir hata oluştu";
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing track {TrackId}", model.Id);
                ModelState.AddModelError(string.Empty, "Müzik güncellenirken bir hata oluştu");
                return View(model);
            }
        }

        // Müzik silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("delete-track", 10, TimeSpan.FromHours(1)))
            {
                TempData["Error"] = "Çok fazla silme denemesi. 1 saat bekleyip tekrar deneyin.";
                return RedirectToAction(nameof(MyTracks));
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _trackService.DeleteTrackAsync(id, userId);

                if (success)
                {
                    TempData["Success"] = "Müzik başarıyla silindi";
                    return RedirectToAction(nameof(MyTracks));
                }

                TempData["Error"] = "Müzik silinirken bir hata oluştu";
                return RedirectToAction(nameof(MyTracks));
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Yetkiniz bulunmuyor";
                return RedirectToAction(nameof(MyTracks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting track {TrackId}", id);
                TempData["Error"] = "Müzik silinirken bir hata oluştu";
                return RedirectToAction(nameof(MyTracks));
            }
        }

        // Müzik beğenme/beğenmeme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("toggle-like", 50, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla beğeni denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var isLiked = await _likeService.IsTrackLikedAsync(id, userId);

                bool success;
                if (isLiked)
                {
                    success = await _likeService.UnlikeTrackAsync(id, userId);
                }
                else
                {
                    success = await _likeService.LikeTrackAsync(id, userId);
                }

                if (success)
                {
                    var likeCount = await _likeService.GetTrackLikeCountAsync(id);
                    return SuccessResult(data: new { isLiked = !isLiked, likeCount });
                }

                return ErrorResult("İşlem gerçekleştirilemedi");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for track {TrackId}", id);
                return ErrorResult("İşlem gerçekleştirilemedi");
            }
        }

        // Müzik dinleme sayısını artırma
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> IncrementPlayCount(Guid id)
        {
            // Rate limiting kontrolü - aynı müziği çok hızlı dinlemeyi önle
            if (IsRateLimited($"play-count-{id}", 1, TimeSpan.FromMinutes(1)))
            {
                return SuccessResult(); // Sessizce başarılı döndür, spam'i önle
            }

            try
            {
                var userId = GetCurrentUserId();
                await _trackService.IncrementPlayCountAsync(id, userId);
                return SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing play count for {TrackId}", id);
                return ErrorResult("Play count güncellenemedi");
            }
        }

        // Kullanıcının müzikleri
        public async Task<IActionResult> MyTracks(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var tracks = await _trackService.GetUserTracksAsync(userId, userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = tracks.Count() >= pageSize;

                return View(tracks);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user tracks");
                TempData["Error"] = "Müzikler yüklenirken bir hata oluştu";
                return View(new List<TrackViewModel>());
            }
        }

        // Track güncelleme API endpoint'i
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTrack([FromBody] EditTrackViewModel request)
        {
            try
            {
                var userId = GetRequiredUserId();

                // Track'in kullanıcıya ait olup olmadığını kontrol et
                var track = await _trackService.GetTrackByIdAsync(request.Id);
                if (track == null || track.ArtistId != userId)
                {
                    return Json(new { success = false, message = "Track not found or unauthorized" });
                }

                // Track'i güncelle
                track.Title = SanitizeInput(request.Title?.Trim()) ?? track.Title;
                track.Description = SanitizeInput(request.Description?.Trim());
                track.Genre = request.Genre;
                track.SubGenre = SanitizeInput(request.SubGenre?.Trim());
                track.Composer = SanitizeInput(request.Composer?.Trim());
                track.Producer = SanitizeInput(request.Producer?.Trim());
                track.Lyricist = SanitizeInput(request.Lyricist?.Trim());
                track.Copyright = SanitizeInput(request.Copyright?.Trim());
                track.ReleaseDate = request.ReleaseDate;
                track.IsExplicit = request.IsExplicit;
                track.AllowComments = request.AllowComments;
                track.AllowDownloads = request.AllowDownloads;
                track.UpdatedAt = DateTime.UtcNow;

                var result = await _trackService.UpdateTrackAsync(track);

                if (result)
                {
                    return Json(new { success = true, message = "Track updated successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update track" });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating track {TrackId}", request.Id);
                return Json(new { success = false, message = "An error occurred while updating the track" });
            }
        }

        // İlişkili müzikleri getir
        [HttpGet]
        public async Task<IActionResult> GetRelatedTracks(Guid id)
        {
            try
            {
                var track = await _trackService.GetTrackByIdAsync(id);
                if (track == null)
                {
                    return Json(new { success = false, message = "Track not found" });
                }

                var relatedTracks = await _trackService.GetUserTracksAsync(track.ArtistId, Guid.Empty, 1, 5);
                var filteredTracks = relatedTracks.Where(t => t.Id != id).Take(4);

                var trackData = filteredTracks.Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    coverImageUrl = t.CoverImageUrl,
                    formattedDuration = t.FormattedDuration
                });

                return Json(new { success = true, tracks = trackData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading related tracks for {TrackId}", id);
                return Json(new { success = false, message = "Error loading related tracks" });
            }
        }

        // Search tracks for adding to playlist
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return Json(new List<object>());
                }

                var tracks = await _trackService.SearchTracksAsync(q, 1, 20);

                var trackData = tracks.Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    artistName = t.ArtistName,
                    coverImageUrl = t.CoverImageUrl,
                    formattedDuration = t.FormattedDuration
                });

                return Json(trackData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tracks");
                return Json(new { error = "Error searching tracks" });
            }
        }

        // Çalma listesi için tüm şarkıları al
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tracks = await _trackService.SearchTracksAsync(string.Empty);

                var trackList = tracks.Select(t => new
                {
                    id = t.Id.ToString(),
                    title = t.Title,
                    artistName = t.Artist?.Username ?? "Unknown Artist",
                    albumName = t.Album?.Title,
                    coverImageUrl = !string.IsNullOrEmpty(t.CoverImageUrl) && t.CoverImageUrl.StartsWith("http") ? t.CoverImageUrl : null,
                    formattedDuration = t.FormattedDuration
                }).ToList();

                return Json(trackList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all tracks");
                return Json(new { error = "Error loading tracks" });
            }
        }

        // Albüme şarkı ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToAlbum(AddTrackToAlbumViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Invalid data provided." });
                }

                var userId = GetRequiredUserId();
                var success = await _trackService.AddTrackToAlbumAsync(model, userId);

                if (success)
                {
                    return Json(new { success = true, message = "Track added to album successfully!" });
                }
                else
                {
                    return Json(new { success = false, error = "Failed to add track to album." });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, error = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding track to album {AlbumId}", model.AlbumId);
                return Json(new { success = false, error = "An error occurred while adding the track." });
            }
        }

    }
}
