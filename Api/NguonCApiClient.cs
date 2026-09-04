using System.Net;
using System.Text.Json;
using Jellyfin.Plugin.NguonC.Configuration;
using Jellyfin.Plugin.NguonC.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NguonC.Api;

public class NguonCApiClient
{
    private readonly ILogger<NguonCApiClient> _logger;
    private readonly PluginConfiguration _config;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NguonCApiClient(ILogger<NguonCApiClient> logger, PluginConfiguration config)
    {
        _logger = logger;
        _config = config;

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
    }

    /// <summary>
    /// Gets paginated film list.
    /// </summary>
    public async Task<PaginatedResponse<FilmListItem>?> GetFilmsAsync(int page = 1)
    {
        return await GetAsync<PaginatedResponse<FilmListItem>>(
            $"{_config.ApiBaseUrl}/films/phim-moi-cap-nhat?page={page}").ConfigureAwait(false);
    }

    /// <summary>
    /// Gets films by category slug.
    /// </summary>
    public async Task<PaginatedResponse<FilmListItem>?> GetFilmsByCategoryAsync(string slug, int page = 1)
    {
        return await GetAsync<PaginatedResponse<FilmListItem>>(
            $"{_config.ApiBaseUrl}/films/danh-sach/{slug}?page={page}").ConfigureAwait(false);
    }

    /// <summary>
    /// Gets films by genre slug.
    /// </summary>
    public async Task<PaginatedResponse<FilmListItem>?> GetFilmsByGenreAsync(string slug, int page = 1)
    {
        return await GetAsync<PaginatedResponse<FilmListItem>>(
            $"{_config.ApiBaseUrl}/films/the-loai/{slug}?page={page}").ConfigureAwait(false);
    }

    /// <summary>
    /// Gets films by country slug.
    /// </summary>
    public async Task<PaginatedResponse<FilmListItem>?> GetFilmsByCountryAsync(string slug, int page = 1)
    {
        return await GetAsync<PaginatedResponse<FilmListItem>>(
            $"{_config.ApiBaseUrl}/films/quoc-gia/{slug}?page={page}").ConfigureAwait(false);
    }

    /// <summary>
    /// Gets films by year.
    /// </summary>
    public async Task<PaginatedResponse<FilmListItem>?> GetFilmsByYearAsync(string year, int page = 1)
    {
        return await GetAsync<PaginatedResponse<FilmListItem>>(
            $"{_config.ApiBaseUrl}/films/nam-phat-hanh/{year}?page={page}").ConfigureAwait(false);
    }

    /// <summary>
    /// Searches films by keyword.
    /// </summary>
    public async Task<PaginatedResponse<FilmListItem>?> SearchFilmsAsync(string keyword)
    {
        return await GetAsync<PaginatedResponse<FilmListItem>>(
            $"{_config.ApiBaseUrl}/films/search?keyword={Uri.EscapeDataString(keyword)}").ConfigureAwait(false);
    }

    /// <summary>
    /// Gets film detail with episode list.
    /// </summary>
    public async Task<FilmDetail?> GetFilmDetailAsync(string slug)
    {
        return await GetAsync<FilmDetail>(
            $"{_config.ApiBaseUrl}/film/{slug}").ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches the embed page HTML to extract stream data.
    /// </summary>
    public async Task<string?> GetEmbedPageHtmlAsync(string embedUrl)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, embedUrl);
            request.Headers.Referrer = new Uri("https://phim.nguonc.com/");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.Headers.Add("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            _logger.LogWarning("Failed to fetch embed page {Url}: {Status}", embedUrl, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching embed page {Url}", embedUrl);
            return null;
        }
    }

    /// <summary>
    /// Proxies a stream URL request (for m3u8/mp4 streams).
    /// </summary>
    public async Task<HttpResponseMessage?> ProxyStreamRequestAsync(string url, string referer)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Referrer = new Uri(referer);
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("Origin", "https://embed.streamc.xyz");

            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying stream request {Url}", url);
            return null;
        }
    }

    private async Task<T?> GetAsync<T>(string url) where T : class
    {
        try
        {
            _logger.LogDebug("Fetching: {Url}", url);
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }

            _logger.LogWarning("API request failed: {Url} -> {Status}", url, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching {Url}", url);
            return null;
        }
    }
}
