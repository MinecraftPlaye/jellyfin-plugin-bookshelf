using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jellyfin.Plugin.Bookshelf.Providers.ComicVine;

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
        /// Bogus request. If the Api key is wrong, the following error will be thrown: 100: "Invalid
        /// # API Key"
        /// <response code="100">Login info is invalid.</response>
        /// <param name="body">The request body.</param>
        /// <returns>
        /// An <see cref="NoContentResult"/> if the login info is valid or <see cref="UnauthorizedResult"/> if the login info is not valid.
        /// </returns>
        [HttpPost("Jellyfin.Plugin.Bookshelf/ValidateComicVineApiKey")]
        [ProducesResponseType(StatusCodes.Status100Continue)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ValidateComicVineApiKey([FromBody] ComicVineApiInput body)
        {
            var response = await ComicVineRequestHandler.Instance.TestApiKey(body.ApiKey, CancellationToken.None).ConfigureAwait(false);

            // Bogus request. If the Api key is wrong, the following error will be thrown: 100: "Invalid # API Key"
            if (response.Code != System.Net.HttpStatusCode.Continue)
            {
                var msg = "Invalid API key provided";
                return Unauthorized(new { Message = msg });
            }
            return Ok();
        }
    }
}
