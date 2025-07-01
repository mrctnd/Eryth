using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;
using Eryth.Infrastructure;

namespace Eryth.Services
{
    // Albüm işlemleri servis implementasyonu
    public class AlbumService : IAlbumService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public AlbumService(ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<Album?> GetByIdAsync(Guid id)
        {
            return await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
        }        public async Task<AlbumViewModel> GetAlbumViewModelAsync(Guid albumId, Guid currentUserId)
        {
            var album = await GetByIdAsync(albumId);
            if (album == null)
                return null!;

            var albumViewModel = AlbumViewModel.FromAlbum(album, currentUserId == album.ArtistId);
            System.Diagnostics.Debug.WriteLine($"Album {album.Title} cover URL: {album.CoverImageUrl}");
            return albumViewModel;
        }

        public async Task<IEnumerable<AlbumViewModel>> GetUserAlbumsAsync(Guid userId, Guid currentUserId, int page, int pageSize)
        {
            var albums = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .Where(a => a.ArtistId == userId && a.DeletedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return albums.Select(a => AlbumViewModel.FromAlbum(a, currentUserId == a.ArtistId));
        }

        public async Task<IEnumerable<AlbumViewModel>> GetPublicAlbumsAsync(int page, int pageSize)
        {
            var currentDate = DateTime.UtcNow;
            var albums = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .Where(a => a.DeletedAt == null && a.ReleaseDate.HasValue && a.ReleaseDate.Value <= currentDate)
                .OrderByDescending(a => a.ReleaseDate ?? a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return albums.Select(a => AlbumViewModel.FromAlbum(a, false));
        }

        public async Task<IEnumerable<AlbumViewModel>> GetTrendingAlbumsAsync(int page, int pageSize)
        {
            var currentDate = DateTime.UtcNow;
            var albums = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .Where(a => a.DeletedAt == null && a.ReleaseDate.HasValue && a.ReleaseDate.Value <= currentDate)
                .OrderByDescending(a => a.TotalPlayCount)
                .ThenByDescending(a => a.TotalLikeCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return albums.Select(a => AlbumViewModel.FromAlbum(a, false));
        }        public async Task<Album> CreateAlbumAsync(AlbumCreateViewModel model, Guid userId)
        {
            string? coverImageUrl = null;            // Handle cover image upload
            if (model.CoverImage != null)
            {
                try
                {
                    coverImageUrl = await _fileUploadService.UploadImageAsync(model.CoverImage, "albums");
                    System.Diagnostics.Debug.WriteLine($"Album cover uploaded: {coverImageUrl}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cover image upload failed: {ex.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(model.CoverImageUrl))
            {
                coverImageUrl = model.CoverImageUrl;
                System.Diagnostics.Debug.WriteLine($"Album cover URL provided: {coverImageUrl}");
            }

            var album = new Album
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                PrimaryGenre = model.PrimaryGenre,
                ReleaseDate = model.ReleaseDate,
                RecordLabel = model.RecordLabel,
                Copyright = model.Copyright,
                IsExplicit = model.IsExplicit,
                CoverImageUrl = coverImageUrl,
                ArtistId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
            return album;
        }

        public async Task<bool> UpdateAlbumAsync(Guid albumId, AlbumViewModel model, Guid userId)
        {
            var album = await _context.Albums
                .FirstOrDefaultAsync(a => a.Id == albumId && a.ArtistId == userId && a.DeletedAt == null);

            if (album == null) return false;

            album.Title = model.Title;
            album.Description = model.Description;
            album.PrimaryGenre = model.PrimaryGenre;
            album.ReleaseDate = model.ReleaseDate;
            album.RecordLabel = model.RecordLabel;
            album.Copyright = model.Copyright;
            album.IsExplicit = model.IsExplicit;
            album.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }        public async Task<bool> DeleteAlbumAsync(Guid albumId, Guid userId)
        {
            var album = await _context.Albums
                .Include(a => a.Tracks) // Include tracks to delete them as well
                .FirstOrDefaultAsync(a => a.Id == albumId && a.ArtistId == userId);

            if (album == null) return false;

            var trackIds = album.Tracks?.Select(t => t.Id).ToList() ?? new List<Guid>();

            if (trackIds.Any())
            {
                // Delete playlist track entries
                var playlistTracks = await _context.PlaylistTracks
                    .Where(pt => trackIds.Contains(pt.TrackId))
                    .ToListAsync();
                
                if (playlistTracks.Any())
                {
                    _context.PlaylistTracks.RemoveRange(playlistTracks);
                }

                // Delete track likes
                var trackLikes = await _context.Likes
                    .Where(l => l.TrackId.HasValue && trackIds.Contains(l.TrackId.Value))
                    .ToListAsync();
                
                if (trackLikes.Any())
                {
                    _context.Likes.RemoveRange(trackLikes);
                }

                // Delete track comments
                var trackComments = await _context.Comments
                    .Where(c => c.TrackId.HasValue && trackIds.Contains(c.TrackId.Value))
                    .ToListAsync();
                
                if (trackComments.Any())
                {
                    _context.Comments.RemoveRange(trackComments);
                }

                // Delete user play history entries for these tracks
                var playHistoryEntries = await _context.UserPlayHistories
                    .Where(h => trackIds.Contains(h.TrackId))
                    .ToListAsync();

                if (playHistoryEntries.Any())
                {
                    _context.UserPlayHistories.RemoveRange(playHistoryEntries);
                }

                // Delete all tracks associated with this album
                if (album.Tracks != null && album.Tracks.Any())
                {
                    _context.Tracks.RemoveRange(album.Tracks);
                }
            }

            // Finally, delete the album itself
            _context.Albums.Remove(album);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddTrackToAlbumAsync(Guid albumId, Guid trackId, Guid userId)
        {
            var album = await _context.Albums
                .FirstOrDefaultAsync(a => a.Id == albumId && a.ArtistId == userId && a.DeletedAt == null);

            if (album == null) return false;

            var track = await _context.Tracks
                .FirstOrDefaultAsync(t => t.Id == trackId && t.ArtistId == userId);

            if (track == null || track.AlbumId.HasValue) return false;

            track.AlbumId = albumId;
            track.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTrackFromAlbumAsync(Guid albumId, Guid trackId, Guid userId)
        {
            var track = await _context.Tracks
                .FirstOrDefaultAsync(t => t.Id == trackId && t.AlbumId == albumId && t.ArtistId == userId);

            if (track == null) return false;

            track.AlbumId = null;
            track.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TrackViewModel>> GetAlbumTracksAsync(Guid albumId, Guid userId)
        {
            var tracks = await _context.Tracks
                .Include(t => t.Artist)
                .Include(t => t.Likes)
                .Where(t => t.AlbumId == albumId && t.Status == TrackStatus.Active)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            return tracks.Select(t => TrackViewModel.FromTrack(t, userId == t.ArtistId, true, true, true));
        }

        public async Task<IEnumerable<AlbumViewModel>> SearchAlbumsAsync(string query, int page, int pageSize)
        {
            var currentDate = DateTime.UtcNow;
            var albums = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Tracks)
                .Where(a => a.DeletedAt == null && a.ReleaseDate.HasValue && a.ReleaseDate.Value <= currentDate &&
                           (a.Title.Contains(query) ||
                            a.Artist.Username.Contains(query) ||
                            a.Artist.DisplayName.Contains(query)))
                .OrderByDescending(a => a.TotalPlayCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return albums.Select(a => AlbumViewModel.FromAlbum(a, false));
        }

        public async Task<bool> IsTrackInAlbumAsync(Guid albumId, Guid trackId)
        {
            return await _context.Tracks
                .AnyAsync(t => t.Id == trackId && t.AlbumId == albumId);
        }

        public async Task<bool> CanUserEditAlbumAsync(Guid albumId, Guid userId)
        {
            return await _context.Albums
                .AnyAsync(a => a.Id == albumId && a.ArtistId == userId && a.DeletedAt == null);
        }

        // Admin-specific methods
        public async Task<int> GetTotalAlbumCountAsync()
        {
            return await _context.Albums.CountAsync(a => a.DeletedAt == null);
        }
    }
}
