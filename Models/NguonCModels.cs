using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.NguonC.Models;

public class PaginatedResponse<T>
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("paginate")]
    public Paginate? Paginate { get; set; }

    [JsonPropertyName("items")]
    public List<T>? Items { get; set; }
}

public class Paginate
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("total_page")]
    public int TotalPage { get; set; }

    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }

    [JsonPropertyName("items_per_page")]
    public int ItemsPerPage { get; set; }
}

public class FilmListItem
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("original_name")]
    public string? OriginalName { get; set; }

    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; set; }

    [JsonPropertyName("poster_url")]
    public string? PosterUrl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("total_episodes")]
    public int TotalEpisodes { get; set; }

    [JsonPropertyName("current_episode")]
    public string? CurrentEpisode { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("director")]
    public string? Director { get; set; }

    [JsonPropertyName("casts")]
    public string? Casts { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }
}

public class FilmDetail
{
    [JsonPropertyName("movie")]
    public MovieInfo? Movie { get; set; }
}

public class MovieInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("original_name")]
    public string? OriginalName { get; set; }

    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; set; }

    [JsonPropertyName("poster_url")]
    public string? PosterUrl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("total_episodes")]
    public int TotalEpisodes { get; set; }

    [JsonPropertyName("current_episode")]
    public string? CurrentEpisode { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("director")]
    public string? Director { get; set; }

    [JsonPropertyName("casts")]
    public string? Casts { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("category")]
    public Dictionary<string, CategoryGroup>? Category { get; set; }

    [JsonPropertyName("episodes")]
    public List<EpisodeServer>? Episodes { get; set; }
}

public class CategoryGroup
{
    [JsonPropertyName("group")]
    public CategoryInfo? Group { get; set; }

    [JsonPropertyName("list")]
    public List<CategoryInfo>? List { get; set; }
}

public class CategoryInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class EpisodeServer
{
    [JsonPropertyName("server_name")]
    public string? ServerName { get; set; }

    [JsonPropertyName("items")]
    public List<EpisodeItem>? Items { get; set; }
}

public class EpisodeItem
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("embed")]
    public string? Embed { get; set; }
}
