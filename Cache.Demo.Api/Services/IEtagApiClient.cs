namespace Cache.Demo.Api.Services
{
    public interface IEtagApiClient
    {
        Task<HttpResponseMessage> Get();
    }
}