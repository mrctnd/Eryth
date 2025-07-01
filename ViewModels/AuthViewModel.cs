using System.ComponentModel.DataAnnotations;
using Eryth.Models;

namespace Eryth.ViewModels
{
    // ViewModel giriş yapmak için
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta veya kullanıcı adı gereklidir")]
        [Display(Name = "E-posta veya Kullanıcı Adı")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni hatırla")]
        public bool RememberMe { get; set; }

        [Display(Name = "Dönüş URL'si")]
        public string? ReturnUrl { get; set; }

        // Security and validation
        public string? RecaptchaToken { get; set; }
        public int LoginAttempts { get; set; }
        public bool IsAccountLocked { get; set; }
        public DateTime? LockoutEndTime { get; set; }
        public bool ShowRecaptcha => false;
        public bool IsLocked => IsAccountLocked && LockoutEndTime > DateTime.UtcNow;
        public string? LockoutMessage => IsLocked ? $"Hesap {LockoutEndTime:HH:mm} saatine kadar kilitli" : null;
    }

    // ViewModel kayıt olmak için
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Kullanıcı adı sadece harfler, rakamlar, alt çizgi ve tire içerebilir")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz")]
        [StringLength(254, ErrorMessage = "E-posta 254 karakteri geçemez")]
        [Display(Name = "E-posta")]       
         public string Email { get; set; } = string.Empty; [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter uzunluğunda olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_=+\[\]{}|;:,.<>])[A-Za-z\d@$!%*?&\-_=+\[\]{}|;:,.<>]+$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi onaylayın")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Onayı")]
        [Compare("Password", ErrorMessage = "Şifre ve şifre onayı eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Görünen ad 100 karakteri geçemez")]
        [Display(Name = "Görünen Ad (İsteğe Bağlı)")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "Hizmet Şartlarını kabul etmelisiniz")]
        [Display(Name = "Hizmet Şartları ve Gizlilik Politikasını kabul ediyorum")]
        public bool AgreeToTerms { get; set; }

        [Display(Name = "Bültene abone ol")]
        public bool SubscribeToNewsletter { get; set; }
        public string RecaptchaToken { get; set; } = string.Empty;

        // Davet kodu (isteğe bağlı)
        [Display(Name = "Davet Kodu (İsteğe Bağlı)")]
        [StringLength(20, ErrorMessage = "Davet kodu 20 karakteri geçemez")]
        public string? InvitationCode { get; set; }
        public User ToUser()
        {
            return new User
            {
                Username = Username?.Trim() ?? string.Empty,
                Email = Email?.Trim().ToLowerInvariant() ?? string.Empty,
                DisplayName = DisplayName?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    // ViewModel şifremi unuttum işlemi için
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        // [Required(ErrorMessage = "Lütfen reCAPTCHA'yı tamamlayın")] // Geçici olarak devre dışı - anahtar mevcut değil
        public string RecaptchaToken { get; set; } = string.Empty;

        public bool EmailSent { get; set; }
    }

    /// ViewModel şifre resetlemek için
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty; [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter uzunluğunda olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_=+\[\]{}|;:,.<>])[A-Za-z\d@$!%*?&\-_=+\[\]{}|;:,.<>]+$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen şifrenizi onaylayın")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Onayı")]
        [Compare("Password", ErrorMessage = "Şifre ve şifre onayı eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// ViewModel şifre değiştirmek için
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; } = string.Empty; [Required(ErrorMessage = "Yeni şifre gereklidir")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter uzunluğunda olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_=+\[\]{}|;:,.<>])[A-Za-z\d@$!%*?&\-_=+\[\]{}|;:,.<>]+$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir")]
        public string NewPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Lütfen yeni şifrenizi onaylayın")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Onayı")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve şifre onayı eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool PasswordChanged { get; set; }
    }

    /// ViewModel e posta doğrulamak için
    public class ConfirmEmailViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }
        public bool TokenExpired { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// ViewModel e posta doğrulama linki göndermek için
    public class ResendConfirmationViewModel
    {
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        public bool EmailSent { get; set; }
        public string? Message { get; set; }
    }

    /// ViewModel iki aşamalı doğrulamayı yüklemek için
    public class TwoFactorSetupViewModel
    {
        public string QrCodeUrl { get; set; } = string.Empty;
        public string ManualKey { get; set; } = string.Empty;
        [Required(ErrorMessage = "Doğrulama kodu gereklidir")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Doğrulama kodu 6 rakam olmalıdır")]
        [Display(Name = "Doğrulama Kodu")]
        public string VerificationCode { get; set; } = string.Empty;

        public List<string> RecoveryCodes { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool ShowRecoveryCodes { get; set; }
    }

    /// ViewModel iki faktörlü doğrulama için
    public class TwoFactorViewModel
    {
        [Required(ErrorMessage = "Kimlik doğrulama kodu gereklidir")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kimlik doğrulama kodu 6 rakam olmalıdır")]
        [Display(Name = "Kimlik Doğrulama Kodu")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Bu cihazı hatırla")]
        public bool RememberDevice { get; set; }

        public string? ReturnUrl { get; set; }
        public bool UseRecoveryCode { get; set; }
    }

    /// ViewModel kurtarma anahtarı oluşturmak için
    public class RecoveryCodeViewModel
    {
        [Required(ErrorMessage = "Kurtarma kodu gereklidir")]
        [Display(Name = "Kurtarma Kodu")]
        public string RecoveryCode { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    /// ViewModel kullanıcı güvenlik ayarları için
    public class SecuritySettingsViewModel
    {
        public bool TwoFactorEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public List<UserSessionViewModel> ActiveSessions { get; set; } = new();
        public List<UserLoginHistoryViewModel> RecentLogins { get; set; } = new();

        public int PasswordStrengthScore { get; set; }
        public string PasswordStrengthText => GetPasswordStrengthText();
        public string PasswordStrengthColor => GetPasswordStrengthColor();

        public List<SecurityRecommendationViewModel> Recommendations { get; set; } = new();        private string GetPasswordStrengthText()
        {
            return PasswordStrengthScore switch
            {
                >= 80 => "Çok Güçlü",
                >= 60 => "Güçlü",
                >= 40 => "Orta",
                >= 20 => "Zayıf",
                _ => "Çok Zayıf"
            };
        }

        private string GetPasswordStrengthColor()
        {
            return PasswordStrengthScore switch
            {
                >= 80 => "success",
                >= 60 => "primary",
                >= 40 => "warning",
                >= 20 => "danger",
                _ => "danger"
            };
        }
    }

    /// ViewModel aktif kullanıcı oturumu için
    public class UserSessionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsCurrent { get; set; }

        public string RelativeLastActivity => GetRelativeTime(LastActivity);
        public string SessionDuration => GetSessionDuration();

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}dk önce",
                < 1 => $"{(int)timeSpan.TotalHours}s önce",
                < 7 => $"{(int)timeSpan.TotalDays}g önce",
                _ => dateTime.ToString("dd MMM yyyy")
            };
        }

        private string GetSessionDuration()
        {
            var duration = LastActivity - CreatedAt;            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays}g {duration.Hours}s";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}s {duration.Minutes}dk";
            return $"{(int)duration.TotalMinutes}dk";
        }
    }

    /// ViewModel kullanıcı giriş yapma geçmişi için
    public class UserLoginHistoryViewModel
    {
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool WasSuccessful { get; set; }
        public string? FailureReason { get; set; }

        public string RelativeTime => GetRelativeTime(LoginTime);
        public string StatusIcon => WasSuccessful ? "✅" : "❌";
        public string StatusColor => WasSuccessful ? "text-success" : "text-danger";

        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;            return timeSpan.TotalDays switch
            {
                < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes}dk önce",
                < 1 => $"{(int)timeSpan.TotalHours}s önce",
                < 7 => $"{(int)timeSpan.TotalDays}g önce",
                _ => dateTime.ToString("dd MMM yyyy HH:mm")
            };
        }
    }

    /// ViewModel güvenlik önerileri için
    public class SecurityRecommendationViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public SecurityRecommendationType Type { get; set; }
        public SecurityRecommendationPriority Priority { get; set; }

        public string PriorityColor => Priority switch
        {
            SecurityRecommendationPriority.High => "danger",
            SecurityRecommendationPriority.Medium => "warning",
            SecurityRecommendationPriority.Low => "info",
            _ => "secondary"
        };

        public string PriorityIcon => Priority switch
        {
            SecurityRecommendationPriority.High => "⚠️",
            SecurityRecommendationPriority.Medium => "⚡",
            SecurityRecommendationPriority.Low => "💡",
            _ => "ℹ️"
        };
    }

    public enum SecurityRecommendationType
    {
        EmailConfirmation,
        TwoFactorAuth,
        PasswordStrength,
        SuspiciousActivity,
        DeviceManagement
    }

    public enum SecurityRecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}
