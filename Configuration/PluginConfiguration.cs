using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.NguonC.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://phim.nguonc.com/api";

    /// <summary>
    /// Gets or sets the items per page.
    /// </summary>
    public int ItemsPerPage { get; set; } = 24;

    /// <summary>
    /// Gets or sets the default language (Vietsub, Thuyet Minh, etc.).
    /// </summary>
    public string PreferredLanguage { get; set; } = "all";
}
