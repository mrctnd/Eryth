using Microsoft.AspNetCore.Mvc;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    public class ExploreController : BaseController
    {
        private readonly ITrackService _trackService;
        private readonly IUserService _userService;
        private readonly IPlaylistService _playlistService;
        private readonly IAlbumService _albumService;
        private readonly ILogger<ExploreController> _logger;

        public ExploreController(
            ITrackService trackService,
            IUserService userService,
            IPlaylistService playlistService,
            IAlbumService albumService,
            ILogger<ExploreController> logger,
            IMemoryCache cache) : base(cache)
        {
            _trackService = trackService;
            _userService = userService;
            _playlistService = playlistService;
            _albumService = albumService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                _logger.LogInformation("Explore page requested. Page: {Page}", page);
                
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var (validPage, pageSize) = ValidatePagination(page, 20);

                _logger.LogInformation("CurrentUserId: {UserId}, ValidPage: {Page}, PageSize: {PageSize}", 
                    currentUserId, validPage, pageSize);

                // Tüm kullanıcıları getir
                var users = await _userService.GetAllUsersAsync(currentUserId, validPage, pageSize);
                ViewBag.UsersCount = users.Count();
                _logger.LogInformation("Retrieved {UserCount} users", users.Count());
                
                // Tüm şarkıları getir
                var tracks = await _trackService.GetAllTracksAsync(validPage, pageSize);
                ViewBag.TracksCount = tracks.Count();
                _logger.LogInformation("Retrieved {TrackCount} tracks", tracks.Count());
                
                // Tüm playlistleri getir
                var playlists = await _playlistService.GetPublicPlaylistsAsync(validPage, pageSize);
                ViewBag.PlaylistsCount = playlists.Count();
                _logger.LogInformation("Retrieved {PlaylistCount} playlists", playlists.Count());
                
                // Tüm albümleri getir
                var albums = await _albumService.GetPublicAlbumsAsync(validPage, pageSize);
                ViewBag.AlbumsCount = albums.Count();
                _logger.LogInformation("Retrieved {AlbumCount} albums", albums.Count());

                // Debug bilgisi
                ViewBag.CurrentUserId = currentUserId;
                ViewBag.PageInfo = $"Page: {validPage}, PageSize: {pageSize}";

                var viewModel = new ExploreViewModel
                {
                    Users = users.ToList(),
                    Tracks = tracks.ToList(),
                    Playlists = playlists.ToList(),
                    Albums = albums.ToList(),
                    CurrentPage = validPage,
                    HasNextPage = users.Count() == pageSize || tracks.Count() == pageSize || 
                                 playlists.Count() == pageSize || albums.Count() == pageSize
                };

                _logger.LogInformation("ExploreViewModel created with Users: {Users}, Tracks: {Tracks}, Playlists: {Playlists}, Albums: {Albums}", 
                    viewModel.Users.Count, viewModel.Tracks.Count, viewModel.Playlists.Count, viewModel.Albums.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Explore page");
                TempData["Error"] = $"Explore sayfası yüklenirken bir hata oluştu: {ex.Message}";
                ViewBag.ErrorDetails = ex.ToString();
                return View(new ExploreViewModel());
            }
        }
    }
}
