using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

#nullable enable
namespace Jellyfin.Plugin.Bookshelf.Providers.ComicVine
{
    public class ComicVineRequestHandler
    {
        public const string API_BASE_URL = "https://comicvine.gamespot.com/api";

        public static ComicVineRequestHandler? Instance { get; set; }

        private readonly IHttpClientFactory _clientFactory;

        private readonly string _version;

        public ComicVineRequestHandler(IHttpClientFactory factory, string version)
        {
            _clientFactory = factory;
            _version = version;
        }

        public async Task<HttpResponse> TestApiKey(string? apiKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Provided API key is blank", nameof(apiKey));
            }

            // for reference, the full url:
            //https://comicvine.gamespot.com/api/issue/1/?api_key=t&format=json&field_list=name
            var client = _clientFactory.CreateClient("Default");
            var result = await client.GetAsync(API_BASE_URL + "/issue/1/?api_key=" + apiKey + "&format=json&field_list=name").ConfigureAwait(false);

            // Todo: change to json
            var resultBody = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var resultXml = await XDocument.LoadAsync(resultBody, LoadOptions.None, cancellationToken);

            return new HttpResponse { Body = resultXml, Code = result.StatusCode };
        }
    }
}
