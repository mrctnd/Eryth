namespace Eryth.Infrastructure
{
    // Cache servisi için arayüz
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemovePatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
    }
}
