using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TokenApi.Models;

namespace TokenApi.Controllers
{
    [ApiController]
    public class DirectLineTokenController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public DirectLineTokenController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Endpoint for generating a Direct Line token bound to a random user ID
        [HttpPost]
        [Route("/api/direct-line-token")]
        [EnableCors("AllowAllPolicy")]
        public async Task<IActionResult> Post()
        {
            // Generate a random user ID to use for DirectLine token
            var randomUserId = GenerateRandomUserId();

            // Get user-specific DirectLine token and return it
            var directLineSecret = _configuration["DirectLine:DirectLineSecret"];
            DirectLineTokenDetails directLineTokenDetails;
            try
            {
                directLineTokenDetails = await FetchDirectLineTokenAsync(directLineSecret, randomUserId);
            }
            catch (InvalidOperationException invalidOpException)
            {
                return BadRequest(new { message = invalidOpException.Message });
            }

            var response = new
            {
                conversationId = directLineTokenDetails.ConversationId,
                token = directLineTokenDetails.Token,
                expiresIn = directLineTokenDetails.ExpiresIn,
                userId = randomUserId,
            };
            return Ok(response);
        }

        // Generates a random user ID
        // Prefixed with "dl_", as required by the Direct Line API
        private static string GenerateRandomUserId()
        {
            byte[] tokenData = new byte[16];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(tokenData);

            return $"dl_{BitConverter.ToString(tokenData).Replace("-", "").ToLower()}";
        }

        // Calls Direct Line API to generate a Direct Line token
        // Provides user ID in the request body to bind the user ID to the token
        private async Task<DirectLineTokenDetails> FetchDirectLineTokenAsync(string directLineSecret, string userId, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(directLineSecret)) throw new ArgumentNullException(nameof(directLineSecret));
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var fetchTokenRequestBody = new { user = new { id = userId } };

            var fetchTokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://directline.botframework.com/v3/directline/tokens/generate")
            {
                Headers =
                {
                    { "Authorization", $"Bearer {directLineSecret}" },
                },
                Content = new StringContent(JsonSerializer.Serialize(fetchTokenRequestBody), Encoding.UTF8, "application/json"),
            };

            var client = _httpClientFactory.CreateClient();
            var fetchTokenResponse = await client.SendAsync(fetchTokenRequest, cancellationToken);

            if (!fetchTokenResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Direct Line token API call failed with status code {fetchTokenResponse.StatusCode}");
            }

            using var responseContentStream = await fetchTokenResponse.Content.ReadAsStreamAsync();
            var tokenApiResponse = await JsonSerializer.DeserializeAsync<DirectLineTokenApiResponse>(responseContentStream);

            return new DirectLineTokenDetails
            {
                Token = tokenApiResponse.Token,
                ExpiresIn = tokenApiResponse.ExpiresIn,
                ConversationId = tokenApiResponse.ConversationId,
            };
        }

        private class DirectLineTokenApiResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("conversationId")]
            public string ConversationId { get; set; }
        }
    }
}
