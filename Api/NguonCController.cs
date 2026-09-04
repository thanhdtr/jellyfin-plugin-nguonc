using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Jellyfin.Plugin.NguonC.Api;
using Jellyfin.Plugin.NguonC.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NguonC.Api;

/// <summary>
/// API controller for proxying NguonC embed pages and streams.
/// This allows Jellyfin clients to play content by fetching the embed page,
/// extracting the stream URL, and proxying it through the Jellyfin server.
/// </summary>
[ApiController]
[Route("NguonC")]
public class NguonCController : ControllerBase
{
    private readonly ILogger<NguonCController> _logger;
    private readonly NguonCApiClient _apiClient;
    private readonly PluginConfiguration _config;

    public NguonCController(
        ILogger<NguonCController> logger,
        NguonCApiClient apiClient,
        PluginConfiguration config)
    {
        _logger = logger;
        _apiClient = apiClient;
        _config = config;
    }

    /// <summary>
    /// Proxies an embed page request, extracting the stream data.
    /// GET /NguonC/embed?url={embedUrl}
    /// </summary>
    [HttpGet("embed")]
    public async Task<IActionResult> GetEmbedPage([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL parameter is required");
        }

        // Validate the URL is from the expected domain
        if (!url.Contains("embed.streamc.xyz") && !url.Contains("phim.nguonc.com"))
        {
            return BadRequest("Invalid embed URL domain");
        }

        _logger.LogDebug("Proxying embed page: {Url}", url);

        var html = await _apiClient.GetEmbedPageHtmlAsync(url).ConfigureAwait(false);
        if (string.IsNullOrEmpty(html))
        {
            return StatusCode(502, "Failed to fetch embed page");
        }

        // Extract data-obf from the page
        var obfMatch = Regex.Match(html, @"data-obf=""([^""]+)""");
        if (obfMatch.Success)
        {
            var obfValue = obfMatch.Groups[1].Value;
            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(obfValue));
                _logger.LogDebug("Decoded stream data: {Data}", decoded);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decode obf value");
            }
        }

        // Return the HTML with proper content type
        return Content(html, "text/html", System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Proxies a stream request (m3u8, mp4, etc).
    /// GET /NguonC/stream?url={streamUrl}&referer={referer}
    /// </summary>
    [HttpGet("stream")]
    public async Task<IActionResult> ProxyStream(
        [FromQuery] string url,
        [FromQuery] string? referer = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL parameter is required");
        }

        referer ??= "https://phim.nguonc.com/";

        _logger.LogDebug("Proxying stream: {Url}", url);

        var response = await _apiClient.ProxyStreamRequestAsync(url, referer).ConfigureAwait(false);
        if (response == null || !response.IsSuccessStatusCode)
        {
            return StatusCode(502, "Failed to fetch stream");
        }

        // Get content type from response
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        // Stream the response
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        // Copy relevant headers
        if (response.Content.Headers.ContentLength.HasValue)
        {
            Response.ContentLength = response.Content.Headers.ContentLength;
        }

        if (response.Content.Headers.ContentType != null)
        {
            Response.ContentType = response.Content.Headers.ContentType.ToString();
        }

        // Add CORS headers for video playback
        Response.Headers.Append("Access-Control-Allow-Origin", "*");
        Response.Headers.Append("Access-Control-Allow-Headers", "*");

        return File(stream, contentType);
    }

    /// <summary>
    /// Resolves an embed URL to a playable m3u8/mp4 URL.
    /// GET /NguonC/resolve?url={embedUrl}
    /// </summary>
    [HttpGet("resolve")]
    public async Task<IActionResult> ResolveStreamUrl([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL parameter is required");
        }

        _logger.LogDebug("Resolving stream URL: {Url}", url);

        var html = await _apiClient.GetEmbedPageHtmlAsync(url).ConfigureAwait(false);
        if (string.IsNullOrEmpty(html))
        {
            return StatusCode(502, "Failed to fetch embed page");
        }

        // Extract data-obf
        var obfMatch = Regex.Match(html, @"data-obf=""([^""]+)""");
        if (!obfMatch.Success)
        {
            return BadRequest("Could not find stream data in embed page");
        }

        try
        {
            var obfValue = obfMatch.Groups[1].Value;
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(obfValue));
            var jsonData = System.Text.Json.JsonDocument.Parse(decoded);

            if (jsonData.RootElement.TryGetProperty("sUb", out var subElement))
            {
                var streamPath = subElement.GetString();
                // Construct the stream URL
                var streamUrl = $"https://embed.streamc.xyz/{streamPath}?d=1";

                return Ok(new
                {
                    streamUrl,
                    path = streamPath,
                    obfData = decoded
                });
            }

            return BadRequest("Could not extract stream path from data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving stream URL");
            return StatusCode(500, "Error resolving stream URL");
        }
    }

    /// <summary>
    /// Gets film list from the API.
    /// GET /NguonC/films?page={page}
    /// </summary>
    [HttpGet("films")]
    public async Task<IActionResult> GetFilms([FromQuery] int page = 1)
    {
        var result = await _apiClient.GetFilmsAsync(page).ConfigureAwait(false);
        if (result == null)
        {
            return StatusCode(502, "Failed to fetch films");
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets film detail.
    /// GET /NguonC/film/{slug}
    /// </summary>
    [HttpGet("film/{slug}")]
    public async Task<IActionResult> GetFilmDetail(string slug)
    {
        var result = await _apiClient.GetFilmDetailAsync(slug).ConfigureAwait(false);
        if (result == null)
        {
            return NotFound("Film not found");
        }
        return Ok(result);
    }

    /// <summary>
    /// Search films.
    /// GET /NguonC/search?keyword={keyword}
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var result = await _apiClient.SearchFilmsAsync(keyword).ConfigureAwait(false);
        if (result == null)
        {
            return StatusCode(502, "Search failed");
        }
        return Ok(result);
    }
}
