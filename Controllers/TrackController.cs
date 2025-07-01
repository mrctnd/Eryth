using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
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

        public TrackController(ITrackService trackService, ILikeService likeService, IMemoryCache cache) : base(cache)
        {
            _trackService = trackService;
            _likeService = likeService;
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
            catch (Exception)
            {
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
            if (model.AudioFile != null && !IsValidFileUpload(model.AudioFile, new[] { ".mp3", ".wav", ".flac", ".m4a" }, 50 * 1024 * 1024)) // 50MB limit
            {
                ModelState.AddModelError("AudioFile", "Geçersiz dosya formatı veya boyutu. Desteklenen formatlar: MP3, WAV, FLAC, M4A (Max: 50MB)");
                return View(model);
            }

            if (model.CoverImage != null && !IsValidFileUpload(model.CoverImage, new[] { ".jpg", ".jpeg", ".png", ".webp" }, 5 * 1024 * 1024)) // 5MB limit
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
                TempData["Error"] = "Müzik silinirken bir hata oluştu";
                return RedirectToAction(nameof(MyTracks));
            }
        }

        // Müzik beğenme/beğenmeme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"ToggleLike called for track: {id}");
            
            // Rate limiting kontrolü
            if (IsRateLimited("toggle-like", 50, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla beğeni denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                System.Diagnostics.Debug.WriteLine($"User ID: {userId}");
                
                var isLiked = await _likeService.IsTrackLikedAsync(id, userId);
                System.Diagnostics.Debug.WriteLine($"Track is currently liked: {isLiked}");

                bool success;
                if (isLiked)
                {
                    System.Diagnostics.Debug.WriteLine("Attempting to unlike track");
                    success = await _likeService.UnlikeTrackAsync(id, userId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Attempting to like track");
                    success = await _likeService.LikeTrackAsync(id, userId);
                }

                System.Diagnostics.Debug.WriteLine($"Operation success: {success}");

                if (success)
                {
                    var likeCount = await _likeService.GetTrackLikeCountAsync(id);
                    System.Diagnostics.Debug.WriteLine($"New like count: {likeCount}");
                    
                    var result = new { isLiked = !isLiked, likeCount };
                    System.Diagnostics.Debug.WriteLine($"Returning: {System.Text.Json.JsonSerializer.Serialize(result)}");
                    
                    return SuccessResult(data: result);
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
            catch (Exception)
            {
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
                ViewBag.HasNextPage = tracks.Count() == pageSize;

                return View(tracks);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Müzikler yüklenirken bir hata oluştu";
                return View(new List<TrackViewModel>());
            }
        }

        // Track güncelleme API endpoint'i
        [HttpPost]
        public async Task<IActionResult> UpdateTrack([FromBody] EditTrackViewModel request)
        {
            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"UpdateTrack called for ID: {request.Id}");
                System.Diagnostics.Debug.WriteLine($"Title: {request.Title}");
                System.Diagnostics.Debug.WriteLine($"Description: {request.Description}");
                System.Diagnostics.Debug.WriteLine($"Genre: {request.Genre}");
                System.Diagnostics.Debug.WriteLine($"ReleaseDate: {request.ReleaseDate}");

                var userId = GetRequiredUserId();

                // Track'in kullanıcıya ait olup olmadığını kontrol et
                var track = await _trackService.GetTrackByIdAsync(request.Id);
                if (track == null || track.ArtistId != userId)
                {
                    System.Diagnostics.Debug.WriteLine("Track not found or unauthorized");
                    return Json(new { success = false, message = "Track not found or unauthorized" });
                }

                System.Diagnostics.Debug.WriteLine($"Original track title: {track.Title}");

                // Track'i güncelle
                track.Title = request.Title?.Trim() ?? track.Title;
                track.Description = request.Description?.Trim();
                track.Genre = request.Genre;
                track.SubGenre = request.SubGenre?.Trim();
                track.Composer = request.Composer?.Trim();
                track.Producer = request.Producer?.Trim();
                track.Lyricist = request.Lyricist?.Trim();
                track.Copyright = request.Copyright?.Trim();
                track.ReleaseDate = request.ReleaseDate;
                track.IsExplicit = request.IsExplicit;
                track.AllowComments = request.AllowComments;
                track.AllowDownloads = request.AllowDownloads;
                track.UpdatedAt = DateTime.UtcNow;

                System.Diagnostics.Debug.WriteLine($"Updated track title: {track.Title}");

                var result = await _trackService.UpdateTrackAsync(track);

                System.Diagnostics.Debug.WriteLine($"Update result: {result}");

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
                // Log the error for debugging
                System.Diagnostics.Debug.WriteLine($"Error updating track: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
            catch (Exception)
            {
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
                return Json(new { error = "Error searching tracks: " + ex.Message });
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
                return Json(new { error = "Error loading tracks", message = ex.Message });
            }
        }

        // Albüme şarkı ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToAlbum(AddTrackToAlbumViewModel model)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddToAlbum called - Title: {model.Title}, AlbumId: {model.AlbumId}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                        .ToArray();
                    System.Diagnostics.Debug.WriteLine($"Model validation failed: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                    return Json(new { success = false, error = "Invalid data provided." });
                }

                var userId = GetRequiredUserId();
                var success = await _trackService.AddTrackToAlbumAsync(model, userId);

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("Track added successfully");
                    return Json(new { success = true, message = "Track added to album successfully!" });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to add track");
                    return Json(new { success = false, error = "Failed to add track to album." });
                }
            }
            catch (UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine("Unauthorized access");
                return Json(new { success = false, error = "Unauthorized access." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in AddToAlbum: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

    }
}
