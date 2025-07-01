using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Takip işlemleri servis implementasyonu
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public FollowService(ApplicationDbContext context, INotificationService notificationService)        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> FollowUserAsync(Guid followerId, Guid followedUserId)
        {
            if (followerId == followedUserId) return false;

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followedUserId);

            if (existingFollow != null) return false;

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followedUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await _notificationService.CreateFollowNotificationAsync(followerId, followedUserId);            return true;
        }

        public async Task<bool> UnfollowUserAsync(Guid followerId, Guid followedUserId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followedUserId);

            if (follow == null) return false;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followedUserId)        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followedUserId);
        }

        public async Task<IEnumerable<UserViewModel>> GetFollowersAsync(Guid userId, int page, int pageSize)
        {
            var followers = await _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == userId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower)
                .ToListAsync();

            return followers.Select(u => UserViewModel.FromUser(u, userId));
        }

        public async Task<IEnumerable<UserViewModel>> GetFollowingAsync(Guid userId, int page, int pageSize)
        {
            var following = await _context.Follows
                .Include(f => f.Following)
                .Where(f => f.FollowerId == userId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Following)
                .ToListAsync();

            return following.Select(u => UserViewModel.FromUser(u, userId));
        }

        public async Task<int> GetFollowerCountAsync(Guid userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowingId == userId);
        }        public async Task<int> GetFollowingCountAsync(Guid userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == userId);
        }

        // Artist following methods
        public async Task<bool> ToggleArtistFollowAsync(Guid userId, Guid artistId)
        {
            if (userId == artistId) return false;

            var existingFollow = await _context.ArtistFollowers
                .FirstOrDefaultAsync(af => af.UserId == userId && af.ArtistId == artistId);

            if (existingFollow != null)
            {
                // Unfollow
                _context.ArtistFollowers.Remove(existingFollow);
                await _context.SaveChangesAsync();
                return false; // Now unfollowed
            }
            else
            {
                // Follow
                var artistFollow = new ArtistFollower
                {
                    UserId = userId,
                    ArtistId = artistId,
                    FollowedAt = DateTime.UtcNow
                };

                _context.ArtistFollowers.Add(artistFollow);
                await _context.SaveChangesAsync();

                // Create notification
                await _notificationService.CreateFollowNotificationAsync(userId, artistId);
                return true; // Now following
            }
        }

        public async Task<bool> IsArtistFollowedAsync(Guid userId, Guid artistId)
        {
            return await _context.ArtistFollowers
                .AnyAsync(af => af.UserId == userId && af.ArtistId == artistId);
        }

        public async Task<int> GetArtistFollowerCountAsync(Guid artistId)
        {
            return await _context.ArtistFollowers
                .CountAsync(af => af.ArtistId == artistId);
        }

        public async Task<IEnumerable<UserViewModel>> GetArtistFollowersAsync(Guid artistId, int page, int pageSize)
        {
            var followers = await _context.ArtistFollowers
                .Include(af => af.User)
                .Where(af => af.ArtistId == artistId)
                .OrderByDescending(af => af.FollowedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(af => af.User)
                .ToListAsync();

            return followers.Select(u => UserViewModel.FromUser(u, artistId));
        }

        public async Task<IEnumerable<UserViewModel>> GetFollowedArtistsAsync(Guid userId, int page, int pageSize)
        {
            var artists = await _context.ArtistFollowers
                .Include(af => af.Artist)
                .Where(af => af.UserId == userId)
                .OrderByDescending(af => af.FollowedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(af => af.Artist)
                .ToListAsync();

            return artists.Select(u => UserViewModel.FromUser(u, userId));
        }

        // Playlist following methods
        public async Task<bool> TogglePlaylistFollowAsync(Guid userId, Guid playlistId)
        {
            var existingFollow = await _context.PlaylistFollowers
                .FirstOrDefaultAsync(pf => pf.UserId == userId && pf.PlaylistId == playlistId);

            if (existingFollow != null)
            {
                // Unfollow
                _context.PlaylistFollowers.Remove(existingFollow);
                await _context.SaveChangesAsync();
                return false; // Now unfollowed
            }
            else
            {
                // Follow
                var playlistFollow = new PlaylistFollower
                {
                    UserId = userId,
                    PlaylistId = playlistId,
                    FollowedAt = DateTime.UtcNow
                };

                _context.PlaylistFollowers.Add(playlistFollow);
                await _context.SaveChangesAsync();
                return true; // Now following
            }
        }

        public async Task<bool> IsPlaylistFollowedAsync(Guid userId, Guid playlistId)
        {
            return await _context.PlaylistFollowers
                .AnyAsync(pf => pf.UserId == userId && pf.PlaylistId == playlistId);
        }

        public async Task<int> GetPlaylistFollowerCountAsync(Guid playlistId)
        {
            return await _context.PlaylistFollowers
                .CountAsync(pf => pf.PlaylistId == playlistId);
        }

        public async Task<IEnumerable<UserViewModel>> GetPlaylistFollowersAsync(Guid playlistId, int page, int pageSize)
        {
            var followers = await _context.PlaylistFollowers
                .Include(pf => pf.User)
                .Where(pf => pf.PlaylistId == playlistId)
                .OrderByDescending(pf => pf.FollowedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pf => pf.User)
                .ToListAsync();

            return followers.Select(u => UserViewModel.FromUser(u, playlistId));
        }

        public async Task<IEnumerable<PlaylistViewModel>> GetFollowedPlaylistsAsync(Guid userId, int page, int pageSize)
        {
            var playlists = await _context.PlaylistFollowers
                .Include(pf => pf.Playlist)
                    .ThenInclude(p => p.CreatedByUser)
                .Include(pf => pf.Playlist)
                    .ThenInclude(p => p.PlaylistTracks)
                .Where(pf => pf.UserId == userId)
                .OrderByDescending(pf => pf.FollowedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pf => pf.Playlist)
                .ToListAsync();

            return playlists.Select(p => PlaylistViewModel.FromPlaylist(p, userId));
        }
    }
}
