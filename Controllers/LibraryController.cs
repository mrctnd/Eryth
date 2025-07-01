using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.ViewModels;
using System.Security.Claims;

namespace Eryth.Controllers
{
    [Authorize]
    public class LibraryController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public LibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string filter = "All")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var userGuid = Guid.Parse(userId);

            var viewModel = new LibraryViewModel
            {
                ActiveFilter = filter
            };
            // Kullanıcının parçalarını al (silinmiş parçalar hariç)
            var userTracks = await _context.Tracks
                .Where(t => t.ArtistId == userGuid && t.DeletedAt == null)
                .Include(t => t.Artist)
                .Include(t => t.Album)
                .Include(t => t.Likes)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            viewModel.UserTracks = userTracks.Select(t => new TrackViewModel
            {
                Id = t.Id,
                Title = t.Title,
                ArtistName = t.Artist.Username,
                CoverImageUrl = t.CoverImageUrl,
                AudioFileUrl = t.AudioFileUrl,
                CreatedAt = t.CreatedAt,
                AlbumTitle = t.Album?.Title,
                LikeCount = t.Likes.Count,
                DurationInSeconds = t.DurationInSeconds
            }).ToList();

            // Kulanıcılanın albümlerini al
            var userAlbums = await _context.Albums
                .Where(a => a.ArtistId == userGuid)
                .Include(a => a.Tracks)
                .Include(a => a.Artist)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();            viewModel.UserAlbums = userAlbums.Select(a => new AlbumViewModel
            {
                Id = a.Id,
                Title = a.Title,
                ArtistName = a.Artist.Username,
                CoverImageUrl = a.CoverImageUrl,
                CreatedAt = a.CreatedAt,
                ReleaseDate = a.ReleaseDate ?? DateTime.UtcNow
            }).ToList();

            // Kullanıcının çalma listelerini al
            var userPlaylists = await _context.Playlists
                .Where(p => p.CreatedByUserId == userGuid)
                .Include(p => p.PlaylistTracks)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            viewModel.UserPlaylists = userPlaylists.Select(p => new PlaylistViewModel
            {
                Id = p.Id,
                Name = p.Title,
                Description = p.Description,
                CoverImageUrl = p.CoverImageUrl,
                TrackCount = p.PlaylistTracks.Count,
                CreatedAt = p.CreatedAt
            }).ToList();

            // Toplam dinlenmeyi hesapla
            viewModel.TotalTracks = viewModel.UserTracks.Count;
            viewModel.TotalAlbums = viewModel.UserAlbums.Count;
            viewModel.TotalPlaylists = viewModel.UserPlaylists.Count;
            viewModel.TotalItems = viewModel.TotalTracks + viewModel.TotalAlbums + viewModel.TotalPlaylists;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrack(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var userGuid = Guid.Parse(userId);

                // Parçanın kullanıcıya ait olup olmadığını ve zaten silinmemiş olduğunu kontrol et
                var track = await _context.Tracks
                    .FirstOrDefaultAsync(t => t.Id == id && t.ArtistId == userGuid && t.DeletedAt == null);
                
                if (track == null)
                {
                    return Json(new { success = false, message = "Track not found or unauthorized" });
                }

                // Parçayı yumuşak sil (soft delete) işlemi yap
                track.DeletedAt = DateTime.UtcNow;
                track.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Track deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting track: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTrack(Guid id, string title, string description)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var userGuid = Guid.Parse(userId);

                // Parçanın kullanıcıya ait olup olmadığını ve silinmemiş olduğunu kontrol et
                var track = await _context.Tracks
                    .FirstOrDefaultAsync(t => t.Id == id && t.ArtistId == userGuid && t.DeletedAt == null);
                
                if (track == null)
                {
                    return Json(new { success = false, message = "Track not found or unauthorized" });
                }

                // Şarkıyı güncelle
                track.Title = title?.Trim() ?? track.Title;
                track.Description = description?.Trim() ?? track.Description;
                track.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Track updated successfully", track = new { title = track.Title, description = track.Description } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating track: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrackDetails(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var userGuid = Guid.Parse(userId);
                  var track = await _context.Tracks
                    .Where(t => t.Id == id && t.ArtistId == userGuid && t.DeletedAt == null)
                    .Select(t => new { 
                        id = t.Id, 
                        title = t.Title, 
                        description = t.Description 
                    })
                    .FirstOrDefaultAsync();
                
                if (track == null)
                {
                    return Json(new { success = false, message = "Track not found or unauthorized" });
                }

                return Json(new { success = true, track = track });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error getting track details: " + ex.Message });
            }
        }
    }
}
