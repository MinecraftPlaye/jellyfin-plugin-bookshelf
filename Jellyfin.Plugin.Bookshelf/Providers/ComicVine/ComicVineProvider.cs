using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions.Json;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

using Jellyfin.Plugin.Bookshelf.Configuration;

#nullable enable
namespace Jellyfin.Plugin.Bookshelf.Providers.ComicVine
{
    public class ComicVineProvider : IRemoteMetadataProvider<Book, BookInfo>
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger<ComicVineProvider> _logger;

        private string _apiKey;

        public string Name => "ComicVine Provider";

        public ComicVineProvider(ILogger<ComicVineProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();

            ComicVineRequestHandler.Instance = new ComicVineRequestHandler(httpClientFactory, version);

            Plugin.Instance!.ConfigurationChanged += (_, _) =>
            {
                _apiKey = GetOptions().ApiKey;
            };

            _apiKey = GetOptions().ApiKey;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(BookInfo item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public async Task<MetadataResult<Book>> GetMetadata(BookInfo item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        private PluginConfiguration GetOptions() => Plugin.Instance!.Configuration;
    }
}

