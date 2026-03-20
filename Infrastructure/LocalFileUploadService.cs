using Microsoft.AspNetCore.Http;
using Eryth.Utilities;

namespace Eryth.Infrastructure
{
    // Yerel dosya yükleme servisi implementasyonu
    public class LocalFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsPath;

        // Magic bytes for file type validation
        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
            { ".mp3", new[] { new byte[] { 0xFF, 0xFB }, new byte[] { 0xFF, 0xF3 }, new byte[] { 0xFF, 0xF2 }, new byte[] { 0x49, 0x44, 0x33 } } },
            { ".wav", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
            { ".flac", new[] { new byte[] { 0x66, 0x4C, 0x61, 0x43 } } },
            { ".ogg", new[] { new byte[] { 0x4F, 0x67, 0x67, 0x53 } } },
            { ".aac", new[] { new byte[] { 0xFF, 0xF1 }, new byte[] { 0xFF, 0xF9 } } }
        };

        public LocalFileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            FileHelper.EnsureDirectoryExists(_uploadsPath);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (!FileHelper.IsValidImageFile(file.FileName, file.Length))
                throw new ArgumentException("Geçersiz resim dosyası");

            ValidateFileSignature(file);
            return await UploadFileAsync(file, folder);
        }

        public async Task<string> UploadAudioAsync(IFormFile file, string folder)
        {
            if (!FileHelper.IsValidAudioFile(file.FileName, file.Length))
                throw new ArgumentException("Geçersiz müzik dosyası");

            ValidateFileSignature(file);
            return await UploadFileAsync(file, folder);
        }

        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var folderPath = Path.Combine(_uploadsPath, folder);
            FileHelper.EnsureDirectoryExists(folderPath);

            var fileName = FileHelper.GenerateSafeFileName(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            // Path traversal protection
            var resolvedPath = Path.GetFullPath(filePath);
            if (!resolvedPath.StartsWith(Path.GetFullPath(_uploadsPath), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid file path");
            }

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.Combine("uploads", folder, fileName).Replace("\\", "/");
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);

                // Path traversal protection
                var resolvedPath = Path.GetFullPath(fullPath);
                if (!resolvedPath.StartsWith(Path.GetFullPath(_uploadsPath), StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(false);
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);

            // Path traversal protection
            var resolvedPath = Path.GetFullPath(fullPath);
            if (!resolvedPath.StartsWith(Path.GetFullPath(_uploadsPath), StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(File.Exists(fullPath));
        }

        public string GetFileUrl(string filePath)
        {
            return $"/{filePath}";
        }

        private static void ValidateFileSignature(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!FileSignatures.TryGetValue(extension, out var signatures))
            {
                return;
            }

            using var reader = new BinaryReader(file.OpenReadStream());
            var maxLength = signatures.Max(s => s.Length);
            var headerBytes = reader.ReadBytes(maxLength);

            var isValid = signatures.Any(signature =>
                headerBytes.Length >= signature.Length &&
                headerBytes.Take(signature.Length).SequenceEqual(signature));

            if (!isValid)
            {
                throw new ArgumentException($"File content does not match the expected format for {extension}");
            }
        }
    }
}
