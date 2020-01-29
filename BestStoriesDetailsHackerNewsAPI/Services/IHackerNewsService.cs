using BestStoriesDetailsHackerNewsAPI.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BestStoriesDetailsHackerNewsAPI.Services
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<StoryDto>> GetBestStories(int rank, IMemoryCache cache);
    }
}