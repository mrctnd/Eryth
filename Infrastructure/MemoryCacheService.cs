using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace Eryth.Infrastructure
{
    // Memory cache servisi implementasyonu
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _keys;
        private readonly object _lock = new();

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
            _keys = new HashSet<string>();
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
                options.SetSlidingExpiration(TimeSpan.FromMinutes(30));

            options.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                lock (_lock)
                {
                    _keys.Remove(k.ToString() ?? string.Empty);
                }
            });

            _cache.Set(key, value, options);

            lock (_lock)
            {
                _keys.Add(key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);

            lock (_lock)
            {
                _keys.Remove(key);
            }

            return Task.CompletedTask;
        }

        public Task RemovePatternAsync(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var keysToRemove = new List<string>();

            lock (_lock)
            {
                keysToRemove.AddRange(_keys.Where(key => regex.IsMatch(key)));
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            lock (_lock)
            {
                foreach (var key in keysToRemove)
                {
                    _keys.Remove(key);
                }
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
