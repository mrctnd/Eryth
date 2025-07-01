using System.ComponentModel.DataAnnotations;
using Eryth.Models;

namespace Eryth.ViewModels
{    /// <summary>
    /// Kullanıcı hesap ayarları için kapsamlı ViewModel
    /// </summary>
    public class UserAccountSettingsViewModel
    {
        // Basic Information
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        [Display(Name = "Görünen Ad")]
        public string? DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir")]
        [Display(Name = "Biyografi")]
        public string? Bio { get; set; }

        [StringLength(100, ErrorMessage = "Konum en fazla 100 karakter olabilir")]
        [Display(Name = "Konum")]
        public string? Location { get; set; }

        [Url(ErrorMessage = "Geçerli bir web sitesi adresi giriniz")]
        [StringLength(255, ErrorMessage = "Web sitesi adresi en fazla 255 karakter olabilir")]
        [Display(Name = "Web Sitesi")]
        public string? Website { get; set; }

        // Kişisel bilgiler
        [Range(1900, 2100, ErrorMessage = "Geçerli bir doğum yılı giriniz")]
        [Display(Name = "Doğum Yılı")]
        public int? BirthYear { get; set; }

        [Display(Name = "Cinsiyet")]
        public string? Gender { get; set; }

        // Gizililik ayarları
        [Display(Name = "Hesabımı Gizli Yap")]
        public bool IsPrivate { get; set; }

        [Display(Name = "E-posta Bildirimleri")]
        public bool EmailNotifications { get; set; }

        // Tema ayarları (İlerde!)
        [Display(Name = "Tema")]
        public string Theme { get; set; } = "dark";
        // Dil ayarları (İlerde!)
        [Display(Name = "Dil")]
        public string Language { get; set; } = "tr";

        // Güvenlik ayarları
        [Display(Name = "İki Faktörlü Doğrulama")]
        public bool IsTwoFactorEnabled { get; set; }
        // Profil fotoğrafı
        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile? ProfileImage { get; set; }

        public string? CurrentProfileImageUrl { get; set; }

        [Display(Name = "Banner Görseli")]
        public IFormFile? BannerImage { get; set; }

        public string? CurrentBannerImageUrl { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve tekrarı uyuşmuyor")]
        public string? ConfirmNewPassword { get; set; }

        [Display(Name = "Hesabımı Silmek İstiyorum")]
        public bool RequestAccountDeletion { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifrenizi Onaylayın")]
        public string? DeleteAccountPassword { get; set; }

        public static UserAccountSettingsViewModel FromUser(User user)
        {
            return new UserAccountSettingsViewModel
            {
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                Location = user.Location,
                Website = user.Website,
                BirthYear = user.BirthYear,
                Gender = user.Gender,
                IsPrivate = user.IsPrivate,                EmailNotifications = user.EmailNotifications,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                Theme = user.Theme,
                Language = user.Language,
                CurrentProfileImageUrl = user.ProfileImageUrl,
                CurrentBannerImageUrl = user.BannerImageUrl
            };
        }
    }
}
