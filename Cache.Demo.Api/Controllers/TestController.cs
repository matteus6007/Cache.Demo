using Cache.Demo.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cache.Demo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        private readonly IEtagApiClient _etagClient;

        public TestController(IEtagApiClient etagClient)
        {
            _etagClient = etagClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _etagClient.Get();

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }
    }
}
