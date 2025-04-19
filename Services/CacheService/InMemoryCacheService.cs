using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace MerchantTransactionProcessing.Services.CacheService
{


    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemoryCacheService> _logger;

        public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                    return Task.FromResult(cachedValue);

                return Task.FromResult(default(T?));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from memory cache for key {Key}", key);
                return Task.FromResult(default(T?));
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
                };

                _memoryCache.Set(key, value, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting data in memory cache for key {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing data from memory cache for key {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                return Task.FromResult(_memoryCache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in memory cache: {Key}", key);
                return Task.FromResult(false);
            }
        }
    }
}
