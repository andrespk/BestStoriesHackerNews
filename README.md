# BestStoriesHackerNews
This is a REST client designed to retrieve the Best 20 Stories of Hacker News REST API

## Instructions
1. The API endpoint is /api/stories/best20;
2. The HackerNews API settings are located on appsettings.json file;
3. It was implemented a InMemoryCache strategy in order to reduce the HackerNews API workload;
4. The Json.NET lib was replaced by Utf8Json lib in order to improve the serialization/deserialization performance
