using Microsoft.AspNetCore.Mvc;
using Eryth.Services;
using Eryth.ViewModels;
using Eryth.Models.Enums;
using static Eryth.ViewModels.SearchViewModel;
using Microsoft.Extensions.Caching.Memory;
using Eryth.Data;
using Microsoft.EntityFrameworkCore;

namespace Eryth.Controllers
{
    // Ana sayfa ve genel sayfa işlemlerini yöneten controller
    public class HomeController : BaseController
    {
        private readonly ITrackService _trackService;
        private readonly IPlaylistService _playlistService;
        private readonly IUserService _userService;
        private readonly IAlbumService _albumService;
        private readonly ApplicationDbContext _context;
        
        public HomeController(
            ITrackService trackService,
            IPlaylistService playlistService,
            IUserService userService,
            IAlbumService albumService,
            ApplicationDbContext context,
            IMemoryCache cache) : base(cache)
        {
            _trackService = trackService;
            _playlistService = playlistService;
            _userService = userService;
            _albumService = albumService;
            _context = context;
        }

        // Ana sayfa - trend müzikler ve çalma listeleri
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var (page, pageSize) = ValidatePagination(1, 20);

                var trendingTracks = await _trackService.GetTrendingTracksAsync(page, pageSize);
                var trendingAlbums = await _albumService.GetTrendingAlbumsAsync(page, 10);
                var popularPlaylists = await _playlistService.GetPublicPlaylistsAsync(page, 10);

                // Get platform statistics
                var stats = new DashboardStatsViewModel();
                if (User.Identity?.IsAuthenticated != true)
                {
                    // Only show public stats for non-authenticated users
                    stats.TotalTracks = await _context.Tracks.CountAsync(t => t.DeletedAt == null);
                    stats.TotalAlbums = await _context.Albums.CountAsync(a => a.DeletedAt == null);
                    stats.TotalPlaylists = await _context.Playlists.CountAsync(p => p.DeletedAt == null && p.IsPublic);
                }

                var viewModel = new DashboardViewModel
                {
                    TrendingTracks = trendingTracks.ToList(),
                    TrendingAlbums = trendingAlbums.ToList(),
                    PopularPlaylists = popularPlaylists.ToList(),
                    Stats = stats,
                    IsLoggedIn = currentUserId != Guid.Empty
                }; return View(viewModel);
            }
            catch (Exception)
            {
                TempData["Error"] = "Ana sayfa yüklenirken bir hata oluştu";
                return View(new DashboardViewModel());
            }
        }

        // Keşfet sayfası - müzik keşfi
        public async Task<IActionResult> Discover(int page = 1)
        {
            try
            {
                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var (validPage, pageSize) = ValidatePagination(page, 24);

                var tracks = await _trackService.GetDiscoverTracksAsync(currentUserId, validPage, pageSize);

                ViewBag.CurrentPage = validPage; ViewBag.HasNextPage = tracks.Count() == pageSize;

                return View(tracks);
            }
            catch (Exception)
            {
                TempData["Error"] = "Keşfet sayfası yüklenirken bir hata oluştu";
                return View(new List<TrackViewModel>());
            }
        }
        // Arama sayfası
        public async Task<IActionResult> Search(string query, string type = "all", int page = 1)
        {
            // Rate limiting kontrolü
            if (IsRateLimited("home-search", 50, TimeSpan.FromMinutes(5)))
            {
                TempData["Error"] = "Çok fazla arama denemesi. 5 dakika bekleyip tekrar deneyin.";
                return View(new SearchViewModel { Query = query });
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new SearchViewModel { Query = query });
            }

            try
            {
                query = SanitizeInput(query);

                var currentUserId = GetCurrentUserId() ?? Guid.Empty;
                var (validPage, pageSize) = ValidatePagination(page, 20);

                var searchResult = new SearchViewModel
                {
                    Query = query,
                    CurrentPage = validPage,
                    SearchType = Enum.TryParse<SearchType>(type, true, out var searchType) ? searchType : SearchType.All
                }; switch (type.ToLower())
                {
                    case "tracks":
                        var trackResults = await _trackService.SearchTracksAsync(query, validPage, pageSize);
                        searchResult.Results.Tracks = trackResults.Select(t => new SearchTrackViewModel
                        {
                            Id = t.Id,
                            Title = t.Title,
                            ArtistName = t.ArtistName,
                            Duration = t.DurationInSeconds,
                            Genre = t.Genre,
                            CoverArtUrl = t.CoverImageUrl,
                            CreatedAt = t.CreatedAt,
                            PlayCount = (int)t.PlayCount,
                            LikeCount = (int)t.LikeCount,
                            IsLikedByCurrentUser = t.IsLikedByCurrentUser,
                            CanPlay = true
                        }).ToList();
                        break;
                    case "users":
                        var userResults = await _userService.SearchUsersAsync(query, currentUserId, validPage, pageSize);
                        searchResult.Results.Users = userResults.Select(u => new SearchUserViewModel
                        {
                            Id = u.Id,
                            Username = u.Username,
                            DisplayName = u.DisplayName ?? u.Username,
                            AvatarUrl = u.ProfileImageUrl,
                            IsFollowedByCurrentUser = u.IsFollowedByCurrentUser,
                            FollowerCount = u.FollowerCount,
                            TrackCount = u.TrackCount
                        }).ToList();
                        break;
                    case "playlists":
                        var playlistResults = await _playlistService.GetPublicPlaylistsAsync(validPage, pageSize);
                        searchResult.Results.Playlists = playlistResults
                            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                            .Select(p => new SearchPlaylistViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Description = p.Description,
                                OwnerUsername = p.OwnerUsername,
                                TrackCount = p.TrackCount,
                                CreatedAt = p.CreatedAt,
                                Privacy = PlaylistPrivacy.Public
                            }).ToList();
                        break;
                    default:
                        var allTracks = await _trackService.SearchTracksAsync(query, validPage, 10);
                        searchResult.Results.Tracks = allTracks.Select(t => new SearchTrackViewModel
                        {
                            Id = t.Id,
                            Title = t.Title,
                            ArtistName = t.ArtistName,
                            Duration = t.DurationInSeconds,
                            Genre = t.Genre,
                            CoverArtUrl = t.CoverImageUrl,
                            CreatedAt = t.CreatedAt,
                            PlayCount = (int)t.PlayCount,
                            LikeCount = (int)t.LikeCount,
                            IsLikedByCurrentUser = t.IsLikedByCurrentUser,
                            CanPlay = true
                        }).ToList();

                        var allUsers = await _userService.SearchUsersAsync(query, currentUserId, validPage, 5);
                        searchResult.Results.Users = allUsers.Select(u => new SearchUserViewModel
                        {
                            Id = u.Id,
                            Username = u.Username,
                            DisplayName = u.DisplayName ?? u.Username,
                            AvatarUrl = u.ProfileImageUrl,
                            IsFollowedByCurrentUser = u.IsFollowedByCurrentUser,
                            FollowerCount = u.FollowerCount,
                            TrackCount = u.TrackCount
                        }).ToList();

                        var allPlaylists = await _playlistService.GetPublicPlaylistsAsync(validPage, 5);
                        searchResult.Results.Playlists = allPlaylists
                            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                            .Select(p => new SearchPlaylistViewModel
                            {
                                Id = p.Id,
                                Name = p.Name,
                                Description = p.Description,
                                OwnerUsername = p.OwnerUsername,
                                TrackCount = p.TrackCount,
                                CreatedAt = p.CreatedAt,
                                Privacy = PlaylistPrivacy.Public
                            }).ToList();
                        break;
                }
                return View(searchResult);
            }
            catch (Exception)
            {
                TempData["Error"] = "Arama yapılırken bir hata oluştu";
                return View(new SearchViewModel { Query = query });
            }
        }

        // Gizlilik politikası
        public IActionResult Privacy()
        {
            return View();
        }

        // Hakkında sayfası
        public async Task<IActionResult> About()
        {
            try
            {
                var viewModel = new AboutPageViewModel
                {
                    TotalUsers = await _userService.GetTotalUserCountAsync(),
                    TotalTracks = await _trackService.GetTotalTrackCountAsync(),
                    TotalAlbums = await _albumService.GetTotalAlbumCountAsync(),
                    TotalPlaylists = await _playlistService.GetTotalPlaylistCountAsync(),
                    TotalPlays = 0 // We'll calculate this from play history if needed
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                return View(new AboutPageViewModel());
            }
        }

        // Hata sayfası
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
