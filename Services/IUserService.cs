using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Kullanıcı işlemleri için servis arayüzü
    public interface IUserService
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetUserByUsernameAsync(string username); // Alias for GetByUsernameAsync
        Task<User?> GetByEmailAsync(string email);
        Task<UserProfileViewModel> GetProfileAsync(Guid userId, Guid currentUserId);
        Task<UserViewModel> GetUserViewModelAsync(Guid userId, Guid currentUserId);
        Task<IEnumerable<UserViewModel>> SearchUsersAsync(string query, Guid currentUserId, int page, int pageSize);
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync(Guid currentUserId, int page, int pageSize);
        Task<bool> UpdateProfileAsync(Guid userId, UserProfileEditViewModel model);
        Task<bool> UpdateUserAsync(Guid userId, UserSettingsViewModel model);
        Task<bool> UpdateUserAccountAsync(Guid userId, UserAccountSettingsViewModel model);
        Task<bool> UploadProfileImageAsync(Guid userId, string imageUrl);
        Task<bool> UploadBannerImageAsync(Guid userId, string bannerUrl);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> DeleteUserAccountAsync(Guid userId);
        Task<int> GetFollowerCountAsync(Guid userId);
        Task<int> GetFollowingCountAsync(Guid userId);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);

        // Admin-specific methods
        Task<int> GetTotalUserCountAsync();
        Task<int> GetNewUserCountAsync(DateTime fromDate);
        Task<int> GetActiveUserCountAsync(DateTime fromDate);
        Task<int> GetSuspendedUserCountAsync();
        Task<IEnumerable<UserViewModel>> GetRecentUsersAsync(int count);
        Task<IEnumerable<UserViewModel>> GetUsersForAdminAsync(int page, int pageSize, string? search = null, UserRole? role = null, AccountStatus? status = null);
        Task<IEnumerable<LoginHistoryViewModel>> GetUserLoginHistoryAsync(Guid userId, int page, int pageSize);
        Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole, Guid modifiedBy, string? reason = null);
        Task<bool> UpdateUserStatusAsync(Guid userId, AccountStatus newStatus, Guid modifiedBy, string? reason = null);
    }
}
