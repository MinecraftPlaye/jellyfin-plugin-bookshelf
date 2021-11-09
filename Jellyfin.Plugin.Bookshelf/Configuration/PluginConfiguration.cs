using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Bookshelf.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the API Key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
