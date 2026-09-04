using System.Globalization;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.NguonC.Api;
using Jellyfin.Plugin.NguonC.Configuration;
using Jellyfin.Plugin.NguonC.Models;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.NguonC;

public class NguonCChannel : IChannel
{
    private readonly ILogger<NguonCChannel> _logger;
    private readonly NguonCApiClient _apiClient;

    public NguonCChannel(ILogger<NguonCChannel> logger, NguonCApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public string Name => "NguonC - Phim HD";

    public string Description => "Stream Vietnamese movies and TV shows from phim.nguonc.com";

    public string DataVersion => "1";

    public string HomePageUrl => "https://phim.nguonc.com";

    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    public bool IsEnabledFor(string userId) => true;

    public InternalChannelFeatures GetChannelFeatures()
    {
        return new InternalChannelFeatures
        {
            ContentTypes = new List<ChannelMediaContentType>
            {
                ChannelMediaContentType.Clip,
                ChannelMediaContentType.Movie,
                ChannelMediaContentType.Episode
            },
            MediaTypes = new List<ChannelMediaType>
            {
                ChannelMediaType.Video
            },
            MaxPageSize = 24,
            DefaultSortFields = new List<ChannelItemSortField>
            {
                ChannelItemSortField.DateCreated
            }
        };
    }

    public IEnumerable<ImageType> GetSupportedChannelImages()
    {
        return new List<ImageType>
        {
            ImageType.Primary,
            ImageType.Thumb,
            ImageType.Backdrop
        };
    }

    public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetChannelItems: FolderId={FolderId}", query.FolderId);

        // If no folder, show root categories
        if (string.IsNullOrEmpty(query.FolderId))
        {
            return GetRootFolders();
        }

        var folderId = query.FolderId;
        int page = ((query.StartIndex ?? 0) / (query.Limit ?? 24)) + 1;

        if (folderId.StartsWith("category:"))
        {
            var slug = folderId["category:".Length..];
            return await GetFilmsByCategoryAsync(slug, page, cancellationToken).ConfigureAwait(false);
        }

        if (folderId.StartsWith("genre:"))
        {
            var slug = folderId["genre:".Length..];
            return await GetFilmsByGenreAsync(slug, page, cancellationToken).ConfigureAwait(false);
        }

        if (folderId.StartsWith("country:"))
        {
            var slug = folderId["country:".Length..];
            return await GetFilmsByCountryAsync(slug, page, cancellationToken).ConfigureAwait(false);
        }

        if (folderId.StartsWith("year:"))
        {
            var year = folderId["year:".Length..];
            return await GetFilmsByYearAsync(year, page, cancellationToken).ConfigureAwait(false);
        }

        if (folderId.StartsWith("film:"))
        {
            var slug = folderId["film:".Length..];
            return await GetFilmEpisodesAsync(slug, cancellationToken).ConfigureAwait(false);
        }

        if (folderId.StartsWith("server:"))
        {
            var parts = folderId["server:".Length..].Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int serverIdx))
            {
                return await GetServerEpisodesAsync(parts[0], serverIdx, cancellationToken).ConfigureAwait(false);
            }
        }

        return await GetLatestFilmsAsync(page, cancellationToken).ConfigureAwait(false);
    }

    public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
    {
        var response = new DynamicImageResponse
        {
            Format = ImageFormat.Png,
            Stream = Stream.Null,
            HasImage = false
        };
        return Task.FromResult(response);
    }

    private ChannelItemResult GetRootFolders()
    {
        var items = new List<ChannelItemInfo>
        {
            CreateFolder("category:phim-moi-cap-nhat", "Phim Mới Cập Nhật"),
            CreateFolder("category:dang-chieu", "Đang Chiếu"),
            CreateFolder("category:hoan-tat", "Hoàn Tất"),
            CreateFolder("genre:hanh-dong", "Hành Động"),
            CreateFolder("genre:tinh-cam", "Tình Cảm"),
            CreateFolder("genre:hai-huoc", "Hài Hước"),
            CreateFolder("genre:kinh-di", "Kinh Dị"),
            CreateFolder("genre:viễn-tưởng", "Viễn Tưởng"),
            CreateFolder("genre:co-trang", "Cổ Trang"),
            CreateFolder("country:trung-quoc", "Trung Quốc"),
            CreateFolder("country:han-quoc", "Hàn Quốc"),
            CreateFolder("country:nhat-ban", "Nhật Bản"),
            CreateFolder("country:au-my", "Âu Mỹ"),
            CreateFolder("country:thai-lan", "Thái Lan"),
            CreateFolder("year:2026", "2026"),
            CreateFolder("year:2025", "2025"),
            CreateFolder("year:2024", "2024"),
        };

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count
        };
    }

    private async Task<ChannelItemResult> GetLatestFilmsAsync(int page, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetFilmsAsync(page).ConfigureAwait(false);
        return MapFilmList(result);
    }

    private async Task<ChannelItemResult> GetFilmsByCategoryAsync(string slug, int page, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetFilmsByCategoryAsync(slug, page).ConfigureAwait(false);
        return MapFilmList(result);
    }

    private async Task<ChannelItemResult> GetFilmsByGenreAsync(string slug, int page, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetFilmsByGenreAsync(slug, page).ConfigureAwait(false);
        return MapFilmList(result);
    }

    private async Task<ChannelItemResult> GetFilmsByCountryAsync(string slug, int page, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetFilmsByCountryAsync(slug, page).ConfigureAwait(false);
        return MapFilmList(result);
    }

    private async Task<ChannelItemResult> GetFilmsByYearAsync(string year, int page, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetFilmsByYearAsync(year, page).ConfigureAwait(false);
        return MapFilmList(result);
    }

    private async Task<ChannelItemResult> GetFilmEpisodesAsync(string slug, CancellationToken cancellationToken)
    {
        var detail = await _apiClient.GetFilmDetailAsync(slug).ConfigureAwait(false);
        if (detail?.Movie?.Episodes == null)
        {
            return new ChannelItemResult { Items = new List<ChannelItemInfo>() };
        }

        var items = new List<ChannelItemInfo>();
        for (int i = 0; i < detail.Movie.Episodes.Count; i++)
        {
            var server = detail.Movie.Episodes[i];
            items.Add(CreateFolder(
                $"server:{slug}:{i}",
                server.ServerName ?? $"Server {i + 1}"));
        }

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count
        };
    }

    private async Task<ChannelItemResult> GetServerEpisodesAsync(string filmSlug, int serverIndex, CancellationToken cancellationToken)
    {
        var detail = await _apiClient.GetFilmDetailAsync(filmSlug).ConfigureAwait(false);
        if (detail?.Movie?.Episodes == null || serverIndex >= detail.Movie.Episodes.Count)
        {
            return new ChannelItemResult { Items = new List<ChannelItemInfo>() };
        }

        var server = detail.Movie.Episodes[serverIndex];
        var items = new List<ChannelItemInfo>();

        if (server.Items != null)
        {
            foreach (var episode in server.Items)
            {
                if (string.IsNullOrEmpty(episode.Embed)) continue;

                var channelItem = new ChannelItemInfo
                {
                    Id = $"{filmSlug}:{serverIndex}:{episode.Slug}",
                    Name = $"Tập {episode.Name}",
                    Overview = $"{detail.Movie.Name} - Tập {episode.Name} ({server.ServerName})",
                    Type = ChannelItemType.Media,
                    MediaType = ChannelMediaType.Video,
                    ContentType = ChannelMediaContentType.Episode,
                    ImageUrl = detail.Movie.ThumbUrl,
                    HomePageUrl = episode.Embed,
                };

                // Parse episode number from name (e.g., "1", "2", "01")
                if (int.TryParse(episode.Name, out int epNum))
                {
                    channelItem.IndexNumber = epNum;
                }

                // Parse season number from server name if available
                var seasonMatch = Regex.Match(server.ServerName ?? "", @"Season\s*(\d+)", RegexOptions.IgnoreCase);
                if (seasonMatch.Success && int.TryParse(seasonMatch.Groups[1].Value, out int seasonNum))
                {
                    channelItem.ParentIndexNumber = seasonNum;
                }

                // Parse duration from time string
                if (!string.IsNullOrEmpty(detail.Movie.Time))
                {
                    var timeMatch = Regex.Match(detail.Movie.Time, @"(\d+)");
                    if (timeMatch.Success && int.TryParse(timeMatch.Groups[1].Value, out int minutes))
                    {
                        channelItem.RunTimeTicks = TimeSpan.FromMinutes(minutes).Ticks;
                    }
                }

                items.Add(channelItem);
            }
        }

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count
        };
    }

    private ChannelItemResult MapFilmList(PaginatedResponse<FilmListItem>? result)
    {
        if (result?.Items == null)
        {
            return new ChannelItemResult { Items = new List<ChannelItemInfo>() };
        }

        var items = result.Items.Select(film => new ChannelItemInfo
        {
            Id = $"film:{film.Slug}",
            Name = film.Name,
            OriginalTitle = film.OriginalName,
            Overview = film.Description,
            Type = ChannelItemType.Folder,
            ImageUrl = film.ThumbUrl ?? film.PosterUrl,
            ProductionYear = int.TryParse(film.Year, out var year) ? year : null,
            Genres = film.Language?.Split(',').Select(g => g.Trim()).ToList() ?? new List<string>()
        }).Cast<ChannelItemInfo>().ToList();

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = result.Paginate?.TotalItems ?? items.Count
        };
    }

    private static ChannelItemInfo CreateFolder(string id, string name)
    {
        return new ChannelItemInfo
        {
            Id = id,
            Name = name,
            Type = ChannelItemType.Folder
        };
    }
}
