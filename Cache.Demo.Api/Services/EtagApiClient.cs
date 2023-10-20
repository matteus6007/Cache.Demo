using Cache.Demo.Api.Options;
using Microsoft.Extensions.Options;

namespace Cache.Demo.Api.Services
{
    public class EtagApiClient : IEtagApiClient
    {
        private readonly HttpClient _client;
        private readonly ETagClientOptions _options;

        public EtagApiClient(HttpClient client, IOptions<ETagClientOptions> options)
        {
            if (options.Value.BaseAddress == null)
            {
                throw new ArgumentNullException(nameof(options.Value.BaseAddress));
            }

            _client = client;
            _client.BaseAddress = options.Value.BaseAddress;
            _options = options.Value;
        }

        public async Task<HttpResponseMessage> Get()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _options.UrlPath);

            var response = await _client.SendAsync(request);

            return response;
        }
    }
}
