using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Bookshelf.Configuration
{
    /// <summary>
    /// Instance of the empty plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the ComicVine API Key.
        /// </summary>
        public string ComicVineApiKey { get; set; } = string.Empty;
    }
}
