using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Eryth.Data;
using Eryth.Models;
using Eryth.Models.Enums;
using Eryth.ViewModels;
using Eryth.Utilities;
using Eryth.Infrastructure;

namespace Eryth.Services
{
    // Kimlik doğrulama servisi implementasyonu
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IEmailService emailService, ICacheService cacheService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _cacheService = cacheService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !SecurityHelper.VerifyPassword(password, user.PasswordHash))
                return null;

            // Son giriş zamanını güncelle
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.EmailOrUsername || u.Username == model.EmailOrUsername);

                if (user == null)
                {
                    return new AuthResult { Success = false, Message = "Kullanıcı bulunamadı" };
                }
                if (!SecurityHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    return new AuthResult { Success = false, Message = "Hatalı şifre" };
                }

                // Email doğrulaması zorunluluğu kaldırıldı - tüm hesaplar giriş yapabilir
                // if (user.Status != AccountStatus.Active)
                // {
                //     return new AuthResult { Success = false, Message = "Hesabınız aktif değil. Lütfen e-posta doğrulamasını tamamlayın." };
                // }                // Claims oluştur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("DisplayName", user.DisplayName ?? user.Username),
                    new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                await _httpContextAccessor.HttpContext!.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Son giriş zamanını güncelle
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new AuthResult { Success = true, Message = "Giriş başarılı", User = user };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "Giriş yapılırken bir hata oluştu" };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Email kontrolü
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);

                if (existingUser != null)
                {
                    return new AuthResult { Success = false, Message = "Bu email veya kullanıcı adı zaten kullanılıyor" };
                }
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    DisplayName = model.DisplayName ?? string.Empty,
                    PasswordHash = SecurityHelper.HashPassword(model.Password),
                    Status = AccountStatus.Active, // Email doğrulaması geçici olarak devre dışı
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Email gönderimi geçici olarak devre dışı (SMTP sorunu nedeniyle)
                // TODO: SMTP ayarları düzeltildikten sonra aşağıdaki satırları aktif et
                // var verificationToken = await GenerateEmailVerificationTokenAsync(user.Id);
                // await _emailService.SendEmailVerificationAsync(user.Email, verificationToken);
                // await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);

                return new AuthResult { Success = true, Message = "Kayıt başarılı! Giriş yapabilirsiniz.", User = user };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "Kayıt olurken bir hata oluştu" };
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!SecurityHelper.VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = SecurityHelper.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new ArgumentException("Bu email adresi ile kayıtlı kullanıcı bulunamadı");

            var token = SecurityHelper.GeneratePasswordResetToken();
            var cacheKey = $"password_reset_{token}";

            // Token'ı 1 saat boyunca cache'de sakla
            await _cacheService.SetAsync(cacheKey, user.Id, TimeSpan.FromHours(1));
            await _emailService.SendPasswordResetEmailAsync(email, token);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var cacheKey = $"password_reset_{token}";
            var userId = await _cacheService.GetAsync<Guid>(cacheKey);

            if (userId == Guid.Empty) return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = SecurityHelper.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Token'ı cache'den sil
            await _cacheService.RemoveAsync(cacheKey);
            return true;
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            try
            {
                var result = await ResetPasswordAsync(model.Token, model.Password);
                if (result)
                {
                    return new AuthResult { Success = true, Message = "Şifreniz başarıyla sıfırlandı" };
                }
                return new AuthResult { Success = false, Message = "Şifre sıfırlama token'ı geçersiz veya süresi dolmuş" };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "Şifre sıfırlanırken bir hata oluştu" };
            }
        }

        public async Task<string> GenerateEmailVerificationTokenAsync(Guid userId)
        {
            var token = SecurityHelper.GenerateEmailVerificationToken();
            var cacheKey = $"email_verification_{token}";

            // Token'ı 24 saat boyunca cache'de sakla
            await _cacheService.SetAsync(cacheKey, userId, TimeSpan.FromHours(24));
            return token;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var cacheKey = $"email_verification_{token}";
            var userId = await _cacheService.GetAsync<Guid>(cacheKey);

            if (userId == Guid.Empty) return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status = AccountStatus.Active;
            await _context.SaveChangesAsync();

            // Token'ı cache'den sil
            await _cacheService.RemoveAsync(cacheKey);
            return true;
        }

        public async Task<AuthResult> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                var result = await VerifyEmailAsync(token);
                if (result)
                {
                    return new AuthResult { Success = true, Message = "E-posta adresiniz başarıyla doğrulandı" };
                }
                return new AuthResult { Success = false, Message = "E-posta doğrulama token'ı geçersiz veya süresi dolmuş" };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, Message = "E-posta doğrulanırken bir hata oluştu" };
            }
        }

        public async Task<bool> IsEmailVerifiedAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Status == AccountStatus.Active;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                await GeneratePasswordResetTokenAsync(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
