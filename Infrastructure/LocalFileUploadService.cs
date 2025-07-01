using Microsoft.AspNetCore.Http;
using Eryth.Utilities;

namespace Eryth.Infrastructure
{
    // Yerel dosya yükleme servisi implementasyonu
    public class LocalFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsPath;

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

            return await UploadFileAsync(file, folder);
        }

        public async Task<string> UploadAudioAsync(IFormFile file, string folder)
        {
            if (!FileHelper.IsValidAudioFile(file.FileName, file.Length))
                throw new ArgumentException("Geçersiz müzik dosyası");

            return await UploadFileAsync(file, folder);
        }

        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var folderPath = Path.Combine(_uploadsPath, folder);
            FileHelper.EnsureDirectoryExists(folderPath);

            var fileName = FileHelper.GenerateSafeFileName(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.Combine("uploads", folder, fileName).Replace("\\", "/");
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
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
            return Task.FromResult(File.Exists(fullPath));
        }

        public string GetFileUrl(string filePath)
        {
            return $"/{filePath}";
        }
    }
}
