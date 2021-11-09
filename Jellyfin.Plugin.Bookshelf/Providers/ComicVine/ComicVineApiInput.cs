using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.Bookshelf.Providers.ComicVine
{
    /// <summary>
    /// The login model used to authenticate against the ComicVine API.
    /// </summary>
    public class ComicVineApiInput
    {

        /// <summary>
        /// Gets or sets the api key.
        /// </summary>
        [Required]
        public string ApiKey { get; set; } = null!;
    }
}
