using Microsoft.AspNetCore.Http;

namespace Eryth.Infrastructure
{
    // Dosya yükleme servisi için arayüz
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<string> UploadAudioAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        string GetFileUrl(string filePath);
    }
}
