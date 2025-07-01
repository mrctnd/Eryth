using System.Text.RegularExpressions;

namespace Eryth.Utilities
{
    // Dosya işlemleri yardımcı sınıfı
    public static class FileHelper
    {
        // İzin verilen resim dosya türleri
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // İzin verilen müzik dosya türleri
        private static readonly string[] AllowedAudioExtensions = { ".mp3", ".wav", ".flac", ".aac", ".ogg" };

        // Maksimum dosya boyutları (bytes)
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
        private const long MaxAudioSize = 50 * 1024 * 1024; // 50MB

        // Dosya uzantısını al
        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant();
        }

        // Resim dosyası kontrolü
        public static bool IsValidImageFile(string fileName, long fileSize)
        {
            var extension = GetFileExtension(fileName);
            return AllowedImageExtensions.Contains(extension) && fileSize <= MaxImageSize;
        }

        // Müzik dosyası kontrolü
        public static bool IsValidAudioFile(string fileName, long fileSize)
        {
            var extension = GetFileExtension(fileName);
            return AllowedAudioExtensions.Contains(extension) && fileSize <= MaxAudioSize;
        }

        // Güvenli dosya adı oluşturma
        public static string GenerateSafeFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

            // Geçersiz karakterleri temizle
            var safeName = Regex.Replace(nameWithoutExtension, @"[^a-zA-Z0-9._-]", "_");
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];

            return $"{safeName}_{timestamp}_{guid}{extension}";
        }

        // Dosya boyutunu insan okunabilir formata çevir
        public static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }

        // MIME type kontrolü
        public static string GetMimeType(string fileName)
        {
            var extension = GetFileExtension(fileName);

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                _ => "application/octet-stream"
            };
        }

        // Klasör oluşturma
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
