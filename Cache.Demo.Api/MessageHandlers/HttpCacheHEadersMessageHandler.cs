using Cache.Demo.Api.Extensions;
using Cache.Demo.Api.Models;
using Cache.Demo.Api.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;

namespace Cache.Demo.Api.MessageHandlers
{
    /// <summary>
    /// Http Cache Handler using <see cref="IMemoryCache"/>
    /// <see cref="https://github.com/aluRamb0/HttpCacheHeadersMessageHandler/tree/main"/>
    /// </summary>
    public class HttpCacheHeadersMessageHandler : DelegatingHandler
    {
        private const string CacheBustKey = "X-Cache-Bust";
        private static readonly string[] IgnoredHeaders = new[] { "Authorization", "Api-Key", "X-Api-Key" };
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccesor;
        private readonly CachingOptions _options;

        public HttpCacheHeadersMessageHandler(
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor,
            IOptions<CachingOptions> options)
        {
            _cache = cache;
            _httpContextAccesor = httpContextAccessor;
            _options = options.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var pathAndQuery = request.RequestUri?.AbsoluteUri;

            if (string.IsNullOrEmpty(pathAndQuery) || request.Method != HttpMethod.Get)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            // bypass caching
            if (_httpContextAccesor.HttpContext?.Request.Headers.ContainsKey(CacheBustKey) == true)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var key = request.GetRequestKey(IgnoredHeaders).ToSha256();

            if (_cache.TryGetValue<CachedHttpContent>(key, out var cachedEntry)
                && cachedEntry?.Etag is { } eTag)
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(eTag));
            }

            var response = await base.SendAsync(request, cancellationToken);

            var responseETag = response.Headers?.ETag?.Tag;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                await TryUpdateCache(key, response, responseETag);
            }

            if (response.StatusCode == HttpStatusCode.NotModified
                && !string.IsNullOrEmpty(responseETag)
                && cachedEntry != null)
            {
                // set response from cache
                response.Content = new ByteArrayContent(cachedEntry.Entry);
                response.StatusCode = cachedEntry.StatusCode;
            }

            return response;
        }

        private async Task<bool> TryUpdateCache(string key, HttpResponseMessage response, string? responseETag)
        {
            if (string.IsNullOrEmpty(responseETag) || response.Content == null)
            {
                return false;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var headers = response.Content.Headers.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);

            var modifiedEntry = new CachedHttpContent(responseETag, headers, response.StatusCode, bytes);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(_options.CacheDurationInSeconds)
            };

            _cache.Set(key, modifiedEntry, cacheOptions);

            return true;
        }
    }
}
