using Microsoft.AspNetCore.Mvc;
using Riveet.Prerender.Services;
using System.Threading.Tasks;

namespace Riveet.Prerender.Api
{
    [ApiController]
    [Route("api/prerender")]
    public class PrerenderApi : ControllerBase
    {
        protected readonly WebPageService _webPageSerivce;

        public PrerenderApi(WebPageService webPageSerivce)
        {
            _webPageSerivce = webPageSerivce;
        }

        [Produces("text/plain")]
        [HttpGet]
        public async Task<string> Get([FromQuery]string url)
        {
            var result = await _webPageSerivce.Get(url);
            return result;
        }
    }
}