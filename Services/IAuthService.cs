using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    // Kimlik doğrulama servisi için arayüz
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<AuthResult> LoginAsync(LoginViewModel model);
        Task<AuthResult> RegisterAsync(RegisterViewModel model);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<string> GenerateEmailVerificationTokenAsync(Guid userId);
        Task<bool> VerifyEmailAsync(string token);
        Task<AuthResult> ConfirmEmailAsync(string userId, string token);
        Task<bool> IsEmailVerifiedAsync(Guid userId);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task LogoutAsync();
    }

    // Authentication result model
    public class AuthResult    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}
