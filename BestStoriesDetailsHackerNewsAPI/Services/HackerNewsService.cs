using BestStoriesDetailsHackerNewsAPI.DTOs;
using BestStoriesDetailsHackerNewsAPI.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utf8Json;

namespace BestStoriesDetailsHackerNewsAPI.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly IOptions<HackerNewsApiSettings> _apiSettings;
        private readonly HttpClient _htppClient = new HttpClient();

        public HackerNewsService(IOptions<HackerNewsApiSettings> apiSettings)
        {
            _apiSettings = apiSettings;
        }

        public async Task<IEnumerable<StoryDto>> GetBestStories(int rank, IMemoryCache cache)
        {
            var list = new List<StoryDto>();
            var bestStoriesCacheKey = _apiSettings.Value.CacheKey;
            var bestStoriesDataCacheKey = $"{bestStoriesCacheKey}_Data";
            var json = await _htppClient.GetStringAsync(_apiSettings.Value.BestStoriesUri);
            if (json.Length > 0)
            {
                var actualList = JsonSerializer.Deserialize<List<long>>(json).Take(rank).ToList();
                var cachedData = string.Empty;
                cache.TryGetValue(bestStoriesCacheKey, out cachedData);
                var cachedList = string.IsNullOrEmpty(cachedData) ? new List<long>() : JsonSerializer.Deserialize<List<long>>(cachedData);
                if (cachedList.SequenceEqual(actualList))
                {
                    cache.TryGetValue(bestStoriesDataCacheKey, out cachedData);
                    list = JsonSerializer.Deserialize<List<StoryDto>>(cachedData);
                }
                else
                {
                    foreach (var id in actualList)
                    {
                        var uri = _apiSettings.Value.StoryDetailsUri.Replace("{storyId}", id.ToString());
                        var storyJson = await _htppClient.GetStringAsync(uri);
                        if (storyJson.Length > 0)
                        {
                            var story = JsonSerializer.Deserialize<Story>(storyJson);
                            list.Add(new StoryDto
                            {
                                Title = story.title,
                                PostedBy = story.by,
                                Score = story.score,
                                Time = DateTimeHelper.ConvertUnixTime(story.time),
                                Uri = story.url,
                                CommentCount = story.kids != null ? story.kids.Count() : 0
                            });
                        }
                    }
                }
                cache.Set(bestStoriesCacheKey, JsonSerializer.ToJsonString(actualList));
            }
            cache.Set(bestStoriesDataCacheKey, JsonSerializer.ToJsonString(list));
            return list;
        }
    }
}