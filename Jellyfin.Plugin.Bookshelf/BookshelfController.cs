using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jellyfin.Plugin.Bookshelf.Providers.ComicVine;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Jellyfin.Plugin.Bookshelf
{
    /// <summary>
    /// The open subtitles plugin controller.
    /// </summary>
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize(Policy = "DefaultAuthorization")]
    public class BookshelfController : ControllerBase
    {
        /// <summary>
        /// Validates the ComicVine API key.
        /// </summary>
        /// <remarks>
        /// Accepts plugin configuration as JSON body.
        /// </remarks>
        /// Bogus request. If the Api key is wrong, the following error will be thrown in the response body: 100: "Invalid
        /// # API Key"
        /// <response code="100">Login info is invalid.</response>
        /// <param name="body">The request body.</param>
        /// <returns>
        /// An <see cref="NoContentResult"/> if the login info is valid or <see cref="UnauthorizedResult"/> if the login info is not valid.
        /// </returns>
        [HttpPost("Jellyfin.Plugin.Bookshelf/ValidateComicVineApiKey")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ValidateComicVineApiKey([FromBody] ComicVineApiInput body)
        {
            var response = await ComicVineRequestHandler.Instance.TestApiKey(body.ApiKey, CancellationToken.None).ConfigureAwait(false);
            var statusCode = response.Body.XPathSelectElement("response/status_code");

            if (statusCode is null && string.IsNullOrWhiteSpace(statusCode.Value))
            {
                var msg = "No status code in API request response!";
                return Unauthorized(new { Message = msg });
            }

            // Bogus request. If the Api key is wrong, the following error will be thrown: 100: "Invalid # API Key"
            if (statusCode.Value == "100")
            {
                var msg = "Invalid API key provided";
                return Unauthorized(new { Message = msg });
            }
            return Ok();
        }
    }
}
