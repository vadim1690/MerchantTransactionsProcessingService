using MerchantTransactionProcessing.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace MerchantTransactionProcessing.Services.CacheService
{

    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedData = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedData))
                    return default;

                return await cachedData.FromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from Redis cache for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
                };

                if(value == null)
                {
                    _logger.LogError("Error setting data in Redis cache for key {Key}", key);
                    return;
                }
                await _distributedCache.SetStringAsync(
                    key,
                    value.ToJson(),
                    options
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting data in Redis cache for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing data from Redis cache for key {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _distributedCache.GetStringAsync(key) != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in Redis cache: {Key}", key);
                return false;
            }
        }
    }
}
