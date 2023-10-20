﻿using System.Net;
using System.Text.Json;

namespace Cache.Demo.Api.Models
{
    public class CachedHttpContent
    {
        public string Etag { get; }
        public IDictionary<string, IEnumerable<string>> Headers { get; }
        public HttpStatusCode StatusCode { get; }
        public byte[] Entry { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="etag">The etag value <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag"/></param>
        /// <param name="headers">The <see cref="HttpContentHeaders"/> required to re-build the <see cref="HttpContent"/> from cache</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> since the Etag will cause the server to return <see cref="HttpStatusCode.NotModified"/></param>
        /// <param name="entry">The response content as byte array. The <paramref name="headers"/> will ensure that the Content-Type is not lost.</param>
        public CachedHttpContent(string etag, IDictionary<string, IEnumerable<string>> headers, HttpStatusCode statusCode, byte[] entry)
        {
            Etag = etag;
            Headers = headers;
            StatusCode = statusCode;
            Entry = entry;
        }

        /// <returns><code>application/json</code> string</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
