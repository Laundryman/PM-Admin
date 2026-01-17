using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using PM_AdminApp.Server.GraphApi.Interfaces;
using PM_AdminApp.Server.Settings;

namespace PM_AdminApp.Server.Controllers.Graph
{
        [Route("api/graph/")]
        [ApiController]
        //[Authorize]
    public class GraphApiController : Controller
        {
            private readonly IGraphSettings _graphSettings;
            private readonly IGraphService _graphService;
            private readonly IConfiguration _configuration;

            public GraphApiController(IGraphService graphService, IGraphSettings graphSettings, IConfiguration configuration)
            {
            _graphSettings = graphSettings;
            _graphService = graphService;
                _configuration = configuration;
            }

            private async Task<GraphServiceClient> GetGraphClientAsync(string token)
            {
                return await _graphService.GetGraphServiceClient(_graphSettings.ClientId, _graphSettings.TenantId, _graphSettings.ClientSecret, token);
            }

            [HttpGet("get-access-token-confidential-client-credentials")]
            public async Task<IActionResult> GetAccessTokenWithConfidentialClientCredential()
            {
                var accessToken = await _graphService.GetAccessTokenConfidentialClientAsync(
                    _graphSettings.ClientId,
                    _graphSettings.TenantId,
                    _graphSettings.ClientSecret,
                    _graphSettings.Authority
                );
                return Ok(new { accessToken });
            }

            [HttpGet("get-access-token-client-credentials")]
            public async Task<IActionResult> GetAccessTokenWithClientCredential()
            {
                var accessToken = await _graphService.GetAccessTokenWithClientCredentialAsync(
                    _graphSettings.ClientId,
                    _graphSettings.TenantId,
                    _graphSettings.ClientSecret
                );
                return Ok(new { accessToken });
            }

            [HttpPost("create-user-if-not-exists")]
            public async Task<IActionResult> CreateUserIfNotExists(string userEmail, string password, string displayName, string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var validUser = await _graphService.GetUserIfExists(graphClient, userEmail);
                if (validUser != null)
                {
                    return NotFound("User Already Exists");
                }

                var user = await _graphService.CreateUserAsync(graphClient, displayName, userEmail, password);
                return Ok(new { user });
            }

            [HttpGet("get-list-of-users")]
            public async Task<IActionResult> GetUsersList(string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var users = await _graphService.GetUserListAsync(graphClient);
                return Ok(new { users });
            }

            [HttpGet("get-page-iterator")]
            public async Task<IActionResult> GetPageIterator(string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var pageIterator = await _graphService.GetPageIterator(graphClient);
                await pageIterator.IterateAsync();

                return Ok(new { pageIterator });
            }

            [HttpGet("get-users-with-batch-request")]
            public async Task<IActionResult> GetUsersWithBatchRequest(string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var users = await _graphService.GetUsersWithBatchRequest(graphClient);
                return Ok(new { users });
            }

            [HttpGet("get-currently-logged-in-user-info")]
            public async Task<IActionResult> GetCurrentlyLoggedInUserInfo()
            {
                StringValues authHeaders = string.Empty;
                var result = Request.Headers.TryGetValue("Authorization", out authHeaders);
                if (result)
                {
                var token = authHeaders.FirstOrDefault()?.Replace("Bearer", string.Empty).Trim();
                //var token = authHeaders.FirstOrDefault();
                    if (token != null)
                    {
                        var graphClient = await GetGraphClientAsync(token);
                        var loggedInUserInfo = await _graphService.GetCurrentlyLoggedInUserInfo(graphClient);
                        return Ok(new { loggedInUserInfo });
                    }
                    return BadRequest("Token is null");
                }
                else
                {
                    return Unauthorized();
                }
            }

            [HttpGet("get-users-count")]
            public async Task<IActionResult> GetUsersCount(string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var usersCount = await _graphService.GetUsersCount(graphClient);
                return Ok(new { usersCount });
            }

            [HttpGet("get-users-in-group")]
            public async Task<IActionResult> GetUsersInGroup(string groupId, string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var usersInGroup = await _graphService.GetUsersInGroup(graphClient, groupId);
                return Ok(new { usersInGroup });
            }

            [HttpGet("get-applications-in-group")]
            public async Task<IActionResult> GetApplicationsInGroup(string groupId, string token)
            {
                var graphClient = await GetGraphClientAsync(token);
                var applicationsInGroup = await _graphService.GetApplicationsInGroup(graphClient, groupId);
                return Ok(new { applicationsInGroup });
            }

            [HttpPost("get-access-token-username-password")]
            public async Task<IActionResult> GetAccessTokenWithUserNamePassword(string userName, string password)
            {
                var accessToken = await _graphService.GetAccessTokenByUserNamePassword(
                    _graphSettings.ClientId,
                    new[] { "User.Read", "User.ReadAll" },
                    _graphSettings.Authority,
                    userName,
                    password
                );
                return Ok(new { accessToken });
            }
        }
}
