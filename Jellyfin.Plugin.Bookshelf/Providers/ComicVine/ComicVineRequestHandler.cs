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

            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(API_BASE_URL + "/issue/1/?api_key=" + apiKey + "&format=json&field_list=name"),
                Headers =
                {
                    UserAgent = { new ProductInfoHeaderValue("Jellyfin-Plugin-Bookshelf", _version) },
                }
            };

            var client = _clientFactory.CreateClient("Default");
            var result = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var resultBody = await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            return new HttpResponse { Body = resultBody, Code = result.StatusCode };
        }
    }
}
