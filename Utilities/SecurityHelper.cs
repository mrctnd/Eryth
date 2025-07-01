using System.Security.Cryptography;
using System.Text;

namespace Eryth.Utilities
{
    // Şifreleme ve güvenlik yardımcı sınıfı
    public static class SecurityHelper
    {
        // Şifre hash'leme için salt uzunluğu
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        // Şifre hash'leme
        public static string HashPassword(string password)
        {
            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        // Şifre doğrulama
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }

            return true;
        }

        // Güvenli rastgele token oluşturma
        public static string GenerateSecureToken(int length = 32)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..length];
        }

        // Email doğrulama token'ı oluşturma
        public static string GenerateEmailVerificationToken()
        {
            return GenerateSecureToken(64);
        }        // Şifre sıfırlama token'ı oluşturma
        public static string GeneratePasswordResetToken()
        {
            return GenerateSecureToken(64);
        }

        // 2FA için secret anahtarı oluşturma
        public static string GenerateTwoFactorSecret()
        {
            return GenerateSecureToken(32);
        }

        // XSS koruması için güvenli string sanitization
        public static string SanitizeInput(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Tehlikeli karakterleri temizle
            return input
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("&", "&amp;");
        }
    }
}
