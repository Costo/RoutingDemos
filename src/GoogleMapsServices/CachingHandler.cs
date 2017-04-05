using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace GoogleMapsServices
{
    public class CachingHandler: DelegatingHandler
    {
        readonly IMemoryCache _memoryCache;
        readonly ILogger _logger;

        public CachingHandler(ILogger logger, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cached = _memoryCache.Get<HttpResponseMessage>(GetCacheKey(request));
            if(cached != null)
            {
                _logger.LogInformation($"Cache hit for {request.RequestUri}");
                return cached;
            }

            var response = await base.SendAsync(request, cancellationToken);

            var isCacheable = response.IsSuccessStatusCode
                               && response.StatusCode == HttpStatusCode.OK
                               && response.Headers.CacheControl.MaxAge > TimeSpan.Zero;

            if(isCacheable)
            {
                var maxAge = response.Headers.CacheControl.MaxAge;
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = maxAge,
                };
                _memoryCache.Set(GetCacheKey(request), response, options);
            }

            return response;

        }

        private string GetCacheKey(HttpRequestMessage request)
        {
            return $"CachingHandler:{request.RequestUri.ToString()}";
        }
    }
}