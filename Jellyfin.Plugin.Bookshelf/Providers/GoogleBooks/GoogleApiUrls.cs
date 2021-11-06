﻿namespace Jellyfin.Plugin.Bookshelf.Providers.GoogleBooks
{
    /// <summary>
    /// Google API urls.
    /// </summary>
    public static class GoogleApiUrls
    {
        /// <summary>
        /// Gets the search url.
        /// </summary>
        public const string SearchUrl = @"https://www.googleapis.com/books/v1/volumes?q={0}&startIndex={1}&maxResults={2}";

        /// <summary>
        /// Gets the details url.
        /// </summary>
        public const string DetailsUrl = @"https://www.googleapis.com/books/v1/volumes/{0}";
    }
}
