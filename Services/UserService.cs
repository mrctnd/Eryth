using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Kullanıcı işlemleri servis implementasyonu
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Tracks)
                .Include(u => u.Albums)
                .Include(u => u.Playlists)
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == id);
        }        
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Tracks.Where(t => t.Status == TrackStatus.Active))
                .Include(u => u.Albums)
                .Include(u => u.Playlists.Where(p => p.Privacy == PlaylistPrivacy.Public))
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserProfileViewModel> GetProfileAsync(Guid userId, Guid currentUserId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Kullanıcı bulunamadı", nameof(userId));

            return UserProfileViewModel.FromUser(user, currentUserId);
        }
        public async Task<IEnumerable<UserViewModel>> SearchUsersAsync(string query, Guid currentUserId, int page, int pageSize)
        {
            var users = await _context.Users
                .Where(u => u.Username.Contains(query) || u.DisplayName.Contains(query))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return users.Select(u => UserViewModel.FromUser(u, currentUserId));
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync(Guid currentUserId, int page, int pageSize)
        {
            // Get all users for debugging
            var allUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Only return active users but log all
            var activeUsers = allUsers.Where(u => u.Status == Models.Enums.AccountStatus.Active).ToList();
            
            return activeUsers.Select(u => UserViewModel.FromUser(u, currentUserId));
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UserProfileEditViewModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Username = model.Username;
            user.DisplayName = model.DisplayName ?? string.Empty;
            user.Email = model.Email;
            user.Bio = model.Bio;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadProfileImageAsync(Guid userId, string imageUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.ProfileImageUrl = imageUrl;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadBannerImageAsync(Guid userId, string bannerUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.BannerImageUrl = bannerUrl;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<int> GetFollowerCountAsync(Guid userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(Guid userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == userId);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users
                .AnyAsync(u => u.Username == username);
        }
        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        // Alias method for GetByUsernameAsync
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await GetByUsernameAsync(username);
        }

        public async Task<UserViewModel> GetUserViewModelAsync(Guid userId, Guid currentUserId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Kullanıcı bulunamadı", nameof(userId));

            return UserViewModel.FromUser(user, currentUserId);
        }
        public async Task<bool> UpdateUserAsync(Guid userId, UserSettingsViewModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Username = model.Username;
            user.DisplayName = model.DisplayName ?? string.Empty;
            user.Email = model.Email;
            user.Bio = model.Bio;
            user.Location = model.Location;
            user.Website = model.Website;
            user.IsPrivate = model.IsPrivate;
            user.EmailNotifications = model.EmailNotifications;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserAccountAsync(Guid userId, UserAccountSettingsViewModel model)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Username = model.Username;
            user.DisplayName = model.DisplayName ?? string.Empty;
            user.Email = model.Email;
            user.Bio = model.Bio;
            user.Location = model.Location;
            user.Website = model.Website;
            user.BirthYear = model.BirthYear;
            user.Gender = model.Gender;
            user.Theme = model.Theme;
            user.Language = model.Language;
            user.IsPrivate = model.IsPrivate;
            user.EmailNotifications = model.EmailNotifications;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }        public async Task<bool> DeleteUserAccountAsync(Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Tracks)
                    .Include(u => u.Albums)
                    .Include(u => u.Playlists)
                    .Include(u => u.Following)
                    .Include(u => u.Followers)
                    .Include(u => u.Likes)
                    .Include(u => u.Comments)
                    .Include(u => u.Notifications)
                    .Include(u => u.PlayHistory)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return false;

                // 1. Kullanıcının yorumlarını sil
                var userComments = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                _context.Comments.RemoveRange(userComments);

                // 2. Kullanıcının beğenilerini sil
                var userLikes = await _context.Likes
                    .Where(l => l.UserId == userId)
                    .ToListAsync();
                _context.Likes.RemoveRange(userLikes);

                // 3. Kullanıcının takip ilişkilerini sil (hem takip ettikleri hem de takipçileri)
                var userFollows = await _context.Follows
                    .Where(f => f.FollowerId == userId || f.FollowingId == userId)
                    .ToListAsync();
                _context.Follows.RemoveRange(userFollows);

                // 4. Kullanıcının bildirimlerini sil
                var userNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId || n.TriggeredByUserId == userId)
                    .ToListAsync();
                _context.Notifications.RemoveRange(userNotifications);

                // 5. Kullanıcının dinleme geçmişini sil
                var userPlayHistory = await _context.UserPlayHistories
                    .Where(ph => ph.UserId == userId)
                    .ToListAsync();
                _context.UserPlayHistories.RemoveRange(userPlayHistory);

                // 6. Kullanıcının playlist'lerindeki track'leri sil
                var playlistTracks = await _context.PlaylistTracks
                    .Where(pt => pt.Playlist.CreatedByUserId == userId)
                    .ToListAsync();
                _context.PlaylistTracks.RemoveRange(playlistTracks);

                // 7. Kullanıcının playlist'lerini sil
                var userPlaylists = await _context.Playlists
                    .Where(p => p.CreatedByUserId == userId)
                    .ToListAsync();
                _context.Playlists.RemoveRange(userPlaylists);

                // 8. Kullanıcının track'lerine yapılan yorumları sil
                var trackComments = await _context.Comments
                    .Include(c => c.Track)
                    .Where(c => c.Track != null && c.Track.ArtistId == userId)
                    .ToListAsync();
                _context.Comments.RemoveRange(trackComments);

                // 9. Kullanıcının track'lerine yapılan beğenileri sil
                var trackLikes = await _context.Likes
                    .Include(l => l.Track)
                    .Where(l => l.Track != null && l.Track.ArtistId == userId)
                    .ToListAsync();
                _context.Likes.RemoveRange(trackLikes);

                // 10. Kullanıcının track'lerini sil
                var userTracks = await _context.Tracks
                    .Where(t => t.ArtistId == userId)
                    .ToListAsync();
                _context.Tracks.RemoveRange(userTracks);

                // 11. Kullanıcının albümlerini sil
                var userAlbums = await _context.Albums
                    .Where(a => a.ArtistId == userId)
                    .ToListAsync();
                _context.Albums.RemoveRange(userAlbums);

                // 12. Kullanıcının mesajlarını sil
                var userMessages = await _context.Messages
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                    .ToListAsync();
                _context.Messages.RemoveRange(userMessages);                // 13. Kullanıcının raporlarını sil (hem gönderdiği hem de kendisi hakkında olanlar)
                var userReports = await _context.Reports
                    .Where(r => r.ReporterId == userId || r.ReportedUserId == userId)
                    .ToListAsync();
                _context.Reports.RemoveRange(userReports);

                // 14. Son olarak kullanıcıyı sil
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error deleting user account: {ex.Message}");
                return false;
            }
        }

        // Admin-specific methods
        public async Task<int> GetTotalUserCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetNewUserCountAsync(DateTime fromDate)
        {
            return await _context.Users.CountAsync(u => u.CreatedAt >= fromDate);
        }

        public async Task<int> GetActiveUserCountAsync(DateTime fromDate)
        {
            return await _context.Users.CountAsync(u => u.LastLoginAt >= fromDate);
        }

        public async Task<int> GetSuspendedUserCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Status == AccountStatus.Suspended);
        }

        public async Task<IEnumerable<UserViewModel>> GetRecentUsersAsync(int count)
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .ToListAsync();

            return users.Select(u => UserViewModel.FromUser(u, Guid.Empty));
        }

        public async Task<IEnumerable<UserViewModel>> GetUsersForAdminAsync(int page, int pageSize, string? search = null, UserRole? role = null, AccountStatus? status = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Username.Contains(search) ||
                                        u.DisplayName.Contains(search) ||
                                        u.Email.Contains(search));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return users.Select(u => UserViewModel.FromUser(u, Guid.Empty));
        }
        public async Task<IEnumerable<LoginHistoryViewModel>> GetUserLoginHistoryAsync(Guid userId, int page, int pageSize)
        {
            // This would require a LoginHistory table - for now return empty list
            // TODO: Implement proper login history tracking
            return await Task.FromResult(new List<LoginHistoryViewModel>());
        }

        public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole, Guid modifiedBy, string? reason = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserStatusAsync(Guid userId, AccountStatus newStatus, Guid modifiedBy, string? reason = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status = newStatus;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
