using System;

namespace BestStoriesDetailsHackerNewsAPI.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ConvertUnixTime(long miliseconds)
        => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(miliseconds);
    }
}