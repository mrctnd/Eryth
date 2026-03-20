using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Eryth.Infrastructure
{
    // Memory cache servisi implementasyonu
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, byte> _keys = new();
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            var result = _cache.TryGetValue(key, out var value) ? (T?)value : default;
            return Task.FromResult(result);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);
            else
                options.SetSlidingExpiration(DefaultExpiration);

            options.RegisterPostEvictionCallback((k, _, _, _) =>
            {
                _keys.TryRemove(k.ToString() ?? string.Empty, out _);
            });

            _cache.Set(key, value, options);
            _keys.TryAdd(key, 0);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        public Task RemovePatternAsync(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var keysToRemove = _keys.Keys.Where(key => regex.IsMatch(key)).ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
    }
}
