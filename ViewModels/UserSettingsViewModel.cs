using System.ComponentModel.DataAnnotations;
using Eryth.Models;

namespace Eryth.ViewModels
{
    // Kullanıcı ayarları için ViewModel
    public class UserSettingsViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        public string? DisplayName { get; set; }

        [StringLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir")]
        public string? Bio { get; set; }

        public string? Location { get; set; }

        public string? Website { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public bool IsPrivate { get; set; }

        public bool EmailNotifications { get; set; }

        public static UserSettingsViewModel FromUser(User user)
        {
            return new UserSettingsViewModel
            {
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                Location = user.Location,
                Website = user.Website,
                IsPrivate = user.IsPrivate,
                EmailNotifications = user.EmailNotifications
            };
        }
    }
}
