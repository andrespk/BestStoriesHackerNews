using System.Collections.Generic;

namespace BestStoriesDetailsHackerNewsAPI
{
    public class Story
    {
        public long id { get; set; }
        public string by { get; set; }
        public int descendants { get; set; }
        public long[] kids { get; set; }
        public string title { get; set; }
        public int score { get; set; }
        public long time { get; set; }
        public string yype { get; set; }
        public string url { get; set; }
    }
}