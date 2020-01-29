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

        private async Task<List<long>> getStoryListFromApi(int rank)
        {
            var json = await _htppClient.GetStringAsync(_apiSettings.Value.BestStoriesUri);
            if (json.Length > 0)
            {
                return JsonSerializer.Deserialize<List<long>>(json).Take(rank).ToList();
            }
            return new List<long>();
        }

        private async Task<StoryDto> getStoryDetailsFromApi(long storyId)
        {
            var uri = _apiSettings.Value.StoryDetailsUri.Replace("{storyId}", storyId.ToString());
            var storyJson = await _htppClient.GetStringAsync(uri);
            if (storyJson.Length > 0)
            {
                var story = JsonSerializer.Deserialize<Story>(storyJson);
                return new StoryDto
                {
                    Title = story.title,
                    PostedBy = story.by,
                    Score = story.score,
                    Time = DateTimeHelper.ConvertUnixTime(story.time),
                    Uri = story.url,
                    CommentCount = story.kids != null ? story.kids.Count() : 0
                };
            }
            return new StoryDto();
        }

        private List<long> getStoryListFromCache(IMemoryCache cache)
        {
            var cachedData = string.Empty;
            cache.TryGetValue(_apiSettings.Value.CacheKey, out cachedData);
            return string.IsNullOrEmpty(cachedData) ? new List<long>() : JsonSerializer.Deserialize<List<long>>(cachedData);
        }

        private List<StoryDto> getCachedStoryDetailsList(IMemoryCache cache)
        {
            var cachedData = string.Empty;
            var storyDetailsCacheKey = $"{_apiSettings.Value.CacheKey}_Data";
            cache.TryGetValue(storyDetailsCacheKey, out cachedData);
            return string.IsNullOrEmpty(cachedData) ? new List<StoryDto>() : JsonSerializer.Deserialize<List<StoryDto>>(cachedData);
        }

        private void setCache(string cacheKey, List<long> storyList, List<StoryDto> storyDetailsList, IMemoryCache cache)
        {
            var storyDetailsCacheKey = $"{cacheKey}_Data";
            cache.Set(cacheKey, JsonSerializer.ToJsonString(storyList));
            cache.Set(storyDetailsCacheKey, JsonSerializer.ToJsonString(storyDetailsList));
        }

        public async Task<IEnumerable<StoryDto>> GetBestStories(int rank, IMemoryCache cache)
        {
            var actualList = await getStoryListFromApi(rank);
            var cachedList = getStoryListFromCache(cache);
            var storyDetailsList = (getCachedStoryDetailsList(cache) ?? new List<StoryDto>()).Take(rank).ToList();
            if (!cachedList.SequenceEqual(actualList))
            {
                for (var i = 0; i < actualList.Count(); i++)
                {
                    if (cachedList.Count() >= i + 1)
                    {
                        if (actualList[i] != cachedList[i])
                            storyDetailsList[i] = await getStoryDetailsFromApi(actualList[i]);
                    }
                    else storyDetailsList.Add(await getStoryDetailsFromApi(actualList[i]));
                }
            }
            setCache(_apiSettings.Value.CacheKey, actualList, storyDetailsList, cache);
            return storyDetailsList;
        }
    }
}