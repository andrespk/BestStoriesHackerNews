using BestStoriesDetailsHackerNewsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoriesDetailsHackerNewsAPI.Controllers
{
    public class StoriesController : ControllerBase
    {
        private readonly HackerNewsService _service;

        public StoriesController(HackerNewsService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("/api/stories/best20")]
        public async Task<IActionResult> GetTwentyBestStories([FromServices]IMemoryCache cache)
        {
            var stories = await _service.GetBestStories(20, cache);
            if (stories.Any()) return Ok(stories);
            return NotFound();
        }
    }
}