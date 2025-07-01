using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    // Albüm işlemlerini yöneten controller
    [Authorize]
    public class AlbumController : BaseController
    {
        private readonly IAlbumService _albumService;
        private readonly ITrackService _trackService;
        private readonly ILikeService _likeService;

        public AlbumController(
            IAlbumService albumService,
            ITrackService trackService,
            ILikeService likeService,
            IMemoryCache cache) : base(cache)
        {
            _albumService = albumService;
            _trackService = trackService;
            _likeService = likeService;
        }

        // Albüm detay sayfası
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var album = await _albumService.GetAlbumViewModelAsync(id, currentUserId);

                if (album == null)
                {
                    TempData["Error"] = "Albüm bulunamadı";
                    return RedirectToAction("Index", "Home");
                }

                var tracks = await _albumService.GetAlbumTracksAsync(id, currentUserId);
                ViewBag.Tracks = tracks;

                return View(album);
            }
            catch (Exception)
            {
                TempData["Error"] = "Albüm yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Albüm oluşturma sayfası
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AlbumCreateViewModel());
        }
        // Albüm oluşturma işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlbumCreateViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("create-album", 5, TimeSpan.FromHours(1)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla albüm oluşturma denemesi. 1 saat bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Girdi temizleme
                model.Title = SanitizeInput(model.Title);
                model.Description = SanitizeInput(model.Description);
                model.RecordLabel = SanitizeInput(model.RecordLabel);
                model.Copyright = SanitizeInput(model.Copyright);

                var userId = GetRequiredUserId();
                var album = await _albumService.CreateAlbumAsync(model, userId);

                TempData["Success"] = "Albüm başarıyla oluşturuldu";
                return RedirectToAction(nameof(Details), new { id = album.Id });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Albüm oluşturulurken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        // Albüm düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = GetRequiredUserId();

                if (!await _albumService.CanUserEditAlbumAsync(id, userId))
                {
                    TempData["Error"] = "Bu albümü düzenleme yetkiniz yok";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var album = await _albumService.GetAlbumViewModelAsync(id, userId);
                if (album == null)
                {
                    TempData["Error"] = "Albüm bulunamadı";
                    return RedirectToAction("Index", "Home");
                }

                return View(album);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Albüm düzenleme sayfası yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }

        // Albüm düzenleme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AlbumViewModel model)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("edit-album", 10, TimeSpan.FromMinutes(30)))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla albüm düzenleme denemesi. 30 dakika bekleyip tekrar deneyin.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Girdi temizleme
                model.Title = SanitizeInput(model.Title);
                model.Description = SanitizeInput(model.Description);
                model.RecordLabel = SanitizeInput(model.RecordLabel);
                model.Copyright = SanitizeInput(model.Copyright);

                var userId = GetRequiredUserId();
                var success = await _albumService.UpdateAlbumAsync(id, model, userId);

                if (success)
                {
                    TempData["Success"] = "Albüm başarıyla güncellendi";
                    return RedirectToAction(nameof(Details), new { id });
                }

                ModelState.AddModelError(string.Empty, "Albüm güncellenirken bir hata oluştu");
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Albüm güncellenirken bir hata oluştu");
                return View(model);
            }
        }

        // Albüm silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("delete-album", 3, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla albüm silme denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _albumService.DeleteAlbumAsync(id, userId);

                if (success)
                {
                    return SuccessResult("Albüm başarıyla silindi");
                }

                return ErrorResult("Albüm silinirken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Albüm silinirken bir hata oluştu");
            }
        }

        // Albüme şarkı ekleme
        [HttpPost]
        public async Task<IActionResult> AddTrack(Guid albumId, Guid trackId)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("album-add-track", 20, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla şarkı ekleme denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _albumService.AddTrackToAlbumAsync(albumId, trackId, userId);

                if (success)
                {
                    return SuccessResult("Şarkı albüme başarıyla eklendi");
                }

                return ErrorResult("Şarkı albüme eklenirken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Şarkı albüme eklenirken bir hata oluştu");
            }
        }

        // Albümden şarkı çıkarma
        [HttpPost]
        public async Task<IActionResult> RemoveTrack(Guid albumId, Guid trackId)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("album-remove-track", 20, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Çok fazla şarkı çıkarma denemesi. 10 dakika bekleyip tekrar deneyin.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var success = await _albumService.RemoveTrackFromAlbumAsync(albumId, trackId, userId);

                if (success)
                {
                    return SuccessResult("Şarkı albümden başarıyla çıkarıldı");
                }

                return ErrorResult("Şarkı albümden çıkarılırken bir hata oluştu");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("Giriş yapmanız gerekiyor");
            }
            catch (Exception)
            {
                return ErrorResult("Şarkı albümden çıkarılırken bir hata oluştu");
            }
        }

        // Kullanıcının albümleri
        public async Task<IActionResult> MyAlbums(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var albums = await _albumService.GetUserAlbumsAsync(userId, userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = albums.Count() == pageSize;

                return View(albums);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Albümler yüklenirken bir hata oluştu";
                return View(new List<AlbumViewModel>());
            }
        }

        // Trend albümler
        [AllowAnonymous]
        public async Task<IActionResult> Trending(int page = 1)
        {
            try
            {
                var (validPage, pageSize) = ValidatePagination(page, 20);
                var albums = await _albumService.GetTrendingAlbumsAsync(validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = albums.Count() == pageSize;

                return View(albums);
            }
            catch (Exception)
            {
                TempData["Error"] = "Trend albümler yüklenirken bir hata oluştu";
                return View(new List<AlbumViewModel>());
            }
        }

        // Albüm arama
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("album-search", 30, TimeSpan.FromMinutes(5)))
            {
                TempData["Error"] = "Çok fazla arama denemesi. 5 dakika bekleyip tekrar deneyin.";
                return View(new List<AlbumViewModel>());
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<AlbumViewModel>());
            }

            try
            {
                // Girdi temizleme
                query = SanitizeInput(query);

                var (validPage, pageSize) = ValidatePagination(page);
                var albums = await _albumService.SearchAlbumsAsync(query, validPage, pageSize);

                ViewBag.Query = query;
                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = albums.Count() == pageSize;

                return View(albums);
            }
            catch (Exception)
            {
                TempData["Error"] = "Arama yapılırken bir hata oluştu";
                return View(new List<AlbumViewModel>());
            }
        }

        // Albüm listeleme sayfası
        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1, string sortBy = "recent", string genre = "")
        {            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                IEnumerable<AlbumViewModel> albums;

                // sortBy parametresine göre mevcut yöntemleri kullan
                switch (sortBy.ToLower())
                {
                    case "trending":
                        albums = await _albumService.GetTrendingAlbumsAsync(page, 20);
                        break;
                    default:
                        albums = await _albumService.GetPublicAlbumsAsync(page, 20);
                        break;
                }
                
                ViewBag.CurrentPage = page;
                ViewBag.SortBy = sortBy;
                ViewBag.TotalCount = await _albumService.GetTotalAlbumCountAsync();

                return View(albums);
            }
            catch (Exception)
            {
                TempData["Error"] = "Albums yüklenirken bir hata oluştu";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
