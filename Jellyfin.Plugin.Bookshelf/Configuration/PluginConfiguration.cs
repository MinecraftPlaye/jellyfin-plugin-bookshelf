using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Bookshelf.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the ComicVine API Key.
        /// </summary>
        public string ComicVineApiKey { get; set; } = string.Empty;
    }
}
