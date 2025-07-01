using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Eryth.Services;
using Eryth.ViewModels;

namespace Eryth.Controllers
{
    [Authorize]
    public class LikesController : BaseController
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService, IMemoryCache cache) : base(cache)
        {
            _likeService = likeService;
        }

        public async Task<IActionResult> Index(string tab = "tracks", int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var viewModel = new LikesViewModel
                {
                    ActiveTab = tab,
                    CurrentPage = validPage,
                    PageSize = pageSize
                };

                if (tab == "tracks")
                {
                    var tracks = await _likeService.GetLikedTracksAsync(userId, validPage, pageSize);
                    viewModel.LikedTracks = tracks.ToList();
                    viewModel.HasNextPage = tracks.Count() == pageSize;
                }
                else if (tab == "playlists")
                {
                    var playlists = await _likeService.GetLikedPlaylistsAsync(userId, validPage, pageSize);
                    viewModel.LikedPlaylists = playlists.ToList();
                    viewModel.HasNextPage = playlists.Count() == pageSize;
                }

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Beğeniler yüklenirken bir hata oluştu";
                return View(new LikesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] ToggleLikeRequest request)
        {
            try
            {
                var userId = GetRequiredUserId();
                bool success = false;
                
                if (request.Type.ToLower() == "track")
                {
                    var isLiked = await _likeService.IsTrackLikedAsync(Guid.Parse(request.ItemId), userId);
                    if (isLiked)
                    {
                        success = await _likeService.UnlikeTrackAsync(Guid.Parse(request.ItemId), userId);
                    }
                    else
                    {
                        success = await _likeService.LikeTrackAsync(Guid.Parse(request.ItemId), userId);
                    }
                }
                else if (request.Type.ToLower() == "playlist")
                {
                    var isLiked = await _likeService.IsPlaylistLikedAsync(Guid.Parse(request.ItemId), userId);
                    if (isLiked)
                    {
                        success = await _likeService.UnlikePlaylistAsync(Guid.Parse(request.ItemId), userId);
                    }
                    else
                    {
                        success = await _likeService.LikePlaylistAsync(Guid.Parse(request.ItemId), userId);
                    }
                }

                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    public class ToggleLikeRequest
    {
        public string ItemId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
