using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;

namespace Cache.Demo.Api.MessageHandlers
{
    /// <summary>
    /// ETag Cache Handler using <see cref="ConcurrentDictionary"/>
    /// <see cref="https://gist.github.com/bbilginn/e41708d17998f69d204847e59f546a19"/>
    /// </summary>
    public class HttpETagCacheHandler : DelegatingHandler
    {
        private static readonly ConcurrentDictionary<string, string> _eTags = new();
        private static readonly ConcurrentDictionary<string, byte[]> _memoryCache = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var pathAndQuery = request.RequestUri?.AbsoluteUri;

            if (string.IsNullOrEmpty(pathAndQuery))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            if (_eTags.TryGetValue(pathAndQuery, out var eTag))
            {
                request.Headers.IfNoneMatch.Clear();

                // ensure you can retrieve it from cache
                if (_memoryCache.ContainsKey(eTag))
                {
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(eTag));
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            var responseETag = response.Headers?.ETag?.Tag;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                await UpdateCache(pathAndQuery, eTag, response, responseETag);
            }

            if (response.StatusCode == HttpStatusCode.NotModified && !string.IsNullOrEmpty(responseETag))
            {
                if (_memoryCache.TryGetValue(responseETag, out var data))
                {
                    response.Content = new ByteArrayContent(data);
                    response.StatusCode = HttpStatusCode.OK;
                }
            }

            return response;
        }

        private static async Task UpdateCache(string pathAndQuery, string? eTag, HttpResponseMessage response, string? responseETag)
        {
            // remove current cache values
            if (!string.IsNullOrEmpty(eTag))
            {
                _eTags.TryRemove(pathAndQuery, out _);
                _memoryCache.TryRemove(eTag, out _);
            }

            if (!string.IsNullOrEmpty(responseETag) && !_eTags.ContainsKey(responseETag) && response.Content != null)
            {
                _eTags.TryAdd(pathAndQuery, responseETag);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                _memoryCache.TryAdd(responseETag, bytes);
            }
        }
    }
}
