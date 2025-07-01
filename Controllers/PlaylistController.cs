using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Eryth.Infrastructure;

namespace Eryth.Controllers
{
    // Çalma listesi işlemlerini yöneten controller
    [Authorize]
    public class PlaylistController : BaseController
    {
        private readonly IPlaylistService _playlistService;
        private readonly ILikeService _likeService;
        private readonly IFileUploadService _fileUploadService;
        
        public PlaylistController(IPlaylistService playlistService, ILikeService likeService, IFileUploadService fileUploadService, IMemoryCache cache) : base(cache)
        {
            _playlistService = playlistService;
            _likeService = likeService;
            _fileUploadService = fileUploadService;
        }

        // Çalma listesi detay sayfası
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var playlist = await _playlistService.GetByIdAsync(id);

                if (playlist == null)
                {
                    TempData["Error"] = "Çalma listesi bulunamadı";
                    return RedirectToAction("Index", "Home");
                }

                var viewModel = PlaylistDetailsViewModel.FromPlaylist(playlist, currentUserId);
                return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Çalma listesi bulunamadı";
                return RedirectToAction("Index", "Home");
            }
        }

        // Çalma listesi oluşturma sayfası
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePlaylistViewModel());
        }

        // Çalma listesi oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlaylistViewModel model)
        {
            // Rate limiting kontrolü - günde 20 çalma listesi oluşturma limiti
            if (IsRateLimited("create-playlist", 20, TimeSpan.FromDays(1)))
            {
                ModelState.AddModelError(string.Empty, "Günlük çalma listesi oluşturma limitinizi aştınız. Yarın tekrar deneyin.");
                return View(model);
            }

            // Kapak görseli sağlanmışsa doğrula
            if (model.CoverImage != null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(model.CoverImage.ContentType.ToLower()))
                {
                    ModelState.AddModelError("CoverImage", "Lütfen geçerli bir resim dosyası seçin (JPEG, PNG, WebP)");
                }
                
                if (model.CoverImage.Length > 5 * 1024 * 1024) // 5MB
                {
                    ModelState.AddModelError("CoverImage", "Resim dosyası boyutu 5MB'dan küçük olmalıdır");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.Name = SanitizeInput(model.Name);
                model.Description = SanitizeInput(model.Description);

                var userId = GetRequiredUserId();
                var playlist = await _playlistService.CreatePlaylistAsync(model, userId);

                TempData["Success"] = "Çalma listesi başarıyla oluşturuldu";
                return RedirectToAction(nameof(Details), new { id = playlist.Id });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Çalma listesi oluşturulurken bir hata oluştu");
                return View(model);
            }
        }

        // Çalma listesi düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = GetRequiredUserId();
                var playlist = await _playlistService.GetByIdAsync(id);

                if (playlist == null || playlist.CreatedByUserId != userId)
                {
                    TempData["Error"] = "Çalma listesi bulunamadı veya düzenleme yetkiniz yok";
                    return RedirectToAction("Index", "Home");
                }

                var model = EditPlaylistViewModel.FromPlaylist(playlist);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Çalma listesi yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Çalma listesi düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPlaylistViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("edit-playlist", 15, TimeSpan.FromHours(1)))
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
                model.Name = SanitizeInput(model.Name);
                model.Description = SanitizeInput(model.Description);

                var userId = GetRequiredUserId();
                var playlistViewModel = new PlaylistViewModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    Privacy = model.Privacy
                };

                var success = await _playlistService.UpdatePlaylistAsync(model.Id, playlistViewModel, userId);

                if (success)
                {
                    TempData["Success"] = "Çalma listesi başarıyla güncellendi";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                TempData["Error"] = "Çalma listesi güncellenirken bir hata oluştu";
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Çalma listesi güncellenirken bir hata oluştu");
                return View(model);
            }
        }

        // Çalma listesi silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("delete-playlist", 10, TimeSpan.FromHours(1)))
            {
                return ErrorResult("Çok fazla silme denemesi. 1 saat bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _playlistService.DeletePlaylistAsync(id, userId);

                if (success)
                {
                    return SuccessResult(message: "Çalma listesi başarıyla silindi");
                }

                return ErrorResult("Çalma listesi silinirken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Yetkiniz bulunmuyor");
            }
            catch (Exception)
            {
                return ErrorResult("Çalma listesi silinirken bir hata oluştu");
            }
        }

        // Çalma listesine müzik ekleme
        [HttpPost]
        public async Task<IActionResult> AddTrack([FromBody] AddTrackRequest request)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("add-track-playlist", 50, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla müzik ekleme denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                if (request == null || request.PlaylistId == Guid.Empty || request.TrackId == Guid.Empty)
                {
                    return ErrorResult("Geçersiz istek");
                }

                var userId = GetRequiredUserId();
                var success = await _playlistService.AddTrackToPlaylistAsync(request.PlaylistId, request.TrackId, userId);

                if (success)
                {
                    return SuccessResult(message: "Müzik çalma listesine eklendi");
                }

                return ErrorResult("Müzik eklenirken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception ex)
            {
                // Hata ayıklama için gerçek hatayı kaydet
                Console.WriteLine($"AddTrack Error: {ex.Message}");
                return ErrorResult("Müzik eklenirken bir hata oluştu");
            }
        }

        // Çalma listesinden müzik çıkarma
        [HttpPost]
        public async Task<IActionResult> RemoveTrack(Guid playlistId, Guid trackId)
        {
            try
            {
                var userId = GetRequiredUserId();
                var success = await _playlistService.RemoveTrackFromPlaylistAsync(playlistId, trackId, userId);

                if (success)
                {
                    return SuccessResult(message: "Müzik çalma listesinden çıkarıldı");
                }

                return ErrorResult("Müzik çıkarılırken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Müzik çıkarılırken bir hata oluştu");
            }
        }

        // Çalma listesi beğenme/beğenmeme işlemi
        [HttpPost]
        public async Task<IActionResult> ToggleLike(Guid id)
        {
            try
            {
                // Kullanıcının kimlik doğrulaması yapılıp yapılmadığını kontrol et
                var currentUserId = GetCurrentUserId();
                if (currentUserId == null)
                {
                    return ErrorResult("Giriş yapmanız gerekiyor");
                }

                var userId = currentUserId.Value;

                // Rate limiting kontrolü
                if (IsRateLimited("toggle-like-playlist", 30, TimeSpan.FromMinutes(10)))
                {
                    return ErrorResult("Çok fazla beğeni denemesi. 10 dakika bekleyip tekrar deneyin.");
                }

                // Çalma listesi var mı kontrol et
                var playlistExists = await _playlistService.GetByIdAsync(id);
                if (playlistExists == null)
                {
                    return ErrorResult("Çalma listesi bulunamadı");
                }

                var isLiked = await _likeService.IsPlaylistLikedAsync(id, userId);
                Console.WriteLine($"Playlist {id} is currently liked by user {userId}: {isLiked}");

                bool success;
                if (isLiked)
                {
                    Console.WriteLine($"Attempting to unlike playlist {id} for user {userId}");
                    success = await _likeService.UnlikePlaylistAsync(id, userId);
                    Console.WriteLine($"Unlike result: {success}");
                }
                else
                {
                    Console.WriteLine($"Attempting to like playlist {id} for user {userId}");
                    success = await _likeService.LikePlaylistAsync(id, userId);
                    Console.WriteLine($"Like result: {success}");
                }

                if (success)
                {
                    var likeCount = await _likeService.GetPlaylistLikeCountAsync(id);
                    Console.WriteLine($"Like count after operation: {likeCount}");
                    return SuccessResult(new { isLiked = !isLiked, likeCount });
                }

                return ErrorResult("İşlem gerçekleştirilemedi");
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                Console.WriteLine($"ToggleLike error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return ErrorResult($"İşlem gerçekleştirilemedi: {ex.Message}");
            }
        }

        // Kullanıcının çalma listeleri
        public async Task<IActionResult> MyPlaylists(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var playlists = await _playlistService.GetUserPlaylistsAsync(userId, userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = playlists.Count() == pageSize;

                return View(playlists);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Çalma listeleri yüklenirken bir hata oluştu";
                return View(new List<PlaylistViewModel>());
            }
        }

        // Beğenilen çalma listeleri
        public async Task<IActionResult> Liked(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var playlists = await _likeService.GetLikedPlaylistsAsync(userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = playlists.Count() == pageSize;

                return View(playlists);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Beğenilen çalma listeleri yüklenirken bir hata oluştu";
                return View(new List<PlaylistViewModel>());
            }
        }

        // Çalma listesi klonlama işlemi
        [HttpPost]
        public async Task<IActionResult> Duplicate(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("duplicate-playlist", 10, TimeSpan.FromHours(1)))
            {
                return ErrorResult("Çok fazla kopyalama denemesi. 1 saat bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var playlist = await _playlistService.GetByIdAsync(id);

                if (playlist == null)
                {
                    return ErrorResult("Çalma listesi bulunamadı");
                }

                // Sadece public çalma listelerini veya kullanıcıya ait çalma listelerini kopyalamaya izin ver
                bool canDuplicate = playlist.CreatedByUserId == userId || 
                                   playlist.Privacy == Models.Enums.PlaylistPrivacy.Public;
                
                if (!canDuplicate)
                {
                    return ErrorResult("Bu çalma listesini kopyalama izniniz yok");
                }

                var duplicatedPlaylist = await _playlistService.DuplicatePlaylistAsync(id, userId);

                if (duplicatedPlaylist != null)
                {
                    return SuccessResult(new { id = duplicatedPlaylist.Id }, "Çalma listesi kopyalandı");
                }

                return ErrorResult("Çalma listesi kopyalanırken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception ex)
            {
                return ErrorResult("Çalma listesi kopyalanırken bir hata oluştu: " + ex.Message);
            }
        }
    }

    // Request models
    public class AddTrackRequest
    {
        public Guid PlaylistId { get; set; }
        public Guid TrackId { get; set; }
    }
}
