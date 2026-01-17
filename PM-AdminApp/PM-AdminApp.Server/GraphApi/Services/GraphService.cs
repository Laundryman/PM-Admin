using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using PM_AdminApp.Server.GraphApi.Interfaces;
using Request = Azure.Core.Request;

namespace PM_AdminApp.Server.GraphApi.Services
{
    public class GraphService : IGraphService
    {
        private readonly ILogger<GraphService> _logger;
        public GraphService(ILogger<GraphService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetAccessTokenConfidentialClientAsync(string clientId, string tenantId,
            string clientSecret, string authority)
        {
            {
                // Define the scopes you need
                var scopes = new[]
                {
                    "https://graph.microsoft.com/.default"
                };

                try
                {
                    var publicClient = ConfidentialClientApplicationBuilder.Create(clientId)
                        .WithClientSecret(clientSecret)
                        .WithAuthority(authority)
                        .WithTenantId(tenantId)
                        .WithRedirectUri("http://localhost:7181/auth/login-callback-ms")
                        .Build();

                    var token = await publicClient.AcquireTokenForClient(scopes)
                        .WithTenantIdFromAuthority(new Uri(authority))
                        .ExecuteAsync();

                    var accessToken = token.AccessToken;

                    return accessToken;
                }
                catch (MsalUiRequiredException ex)
                {
                    _logger.LogCritical($"Error acquiring token: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<string> GetAccessTokenWithClientCredentialAsync(string clientId, string tenantId, string clientSecret,
            CancellationToken cancellationToken = default)
        {
            // Define the scopes you need
            var scopes = new[]
            {
                "https://graph.microsoft.com/.default"
            };

            try
            {
                var options = new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                var credential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

                var tokenRequestContext = new TokenRequestContext(scopes);
                var token = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);
                var accessToken = token.Token;

                return accessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogCritical($"Error acquiring token: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetAccessTokenByUserNamePassword(string clientId, ICollection<string> scopes, string authority, string userName,
            string password)
        {
            try
            {
                var app = PublicClientApplicationBuilder.Create(clientId)
                    .WithAuthority(authority)
                    .WithRedirectUri("http://localhost:7181/auth/login-callback-ms")
                    .Build();

                var result = await app.AcquireTokenByUsernamePassword(scopes, userName, password)
                    .ExecuteAsync();

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.Message);
            }
        }

        public Task<GraphServiceClient> GetGraphServiceClient(string clientId, string tenantId, string clientSecret, string token)
        {
            var options = new AuthorizationCodeCredentialOptions()
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };
            var credential = new AuthorizationCodeCredential(tenantId, clientId, clientSecret, token, options);

            //var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(credential);
            return Task.FromResult(graphClient);
        }

        public async Task<User?> GetUserIfExists(GraphServiceClient graphClient, string userEmail)
        {
            var userCollection = await graphClient.Users
                .GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Filter = $"userPrincipalName eq '{userEmail}'");
            return userCollection?.Value?.FirstOrDefault();
        }

        public async Task<User?> CreateUserAsync(GraphServiceClient graphClient, string? displayName, string userPrincipalName, string password)
        {
            var newUser = new User
            {
                AccountEnabled = true,
                DisplayName = displayName,
                MailNickname = userPrincipalName.Split('@')[0],
                Mail = userPrincipalName,
                UserPrincipalName = userPrincipalName,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = password
                }
            };
            return await graphClient.Users.PostAsync(newUser);
        }

        public async Task<List<User>>? GetUserListAsync(GraphServiceClient graphClient)
        {
            var usersResponse = await graphClient.Users
                .GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Select = ["id", "createdDateTime", "userPrincipalName"]);
            return usersResponse?.Value;
        }

        public Task<PageIterator<User, UserCollectionResponse>>? GetPageIterator(GraphServiceClient graphClient)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>>? GetUsersWithBatchRequest(GraphServiceClient graphClient)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetCurrentlyLoggedInUserInfo(GraphServiceClient graphClient)
        {
            try
            {
                var userInfo = await graphClient.Me.GetAsync();
                return userInfo;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.Message);
            }

        }

        public async Task<int?> GetUsersCount(GraphServiceClient graphClient)
        {
            var count = await graphClient.Users.Count.GetAsync(requestConfiguration =>
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual"));
            return count;
        }

        public async Task<UserCollectionResponse> GetUsersInGroup(GraphServiceClient graphClient, string groupId)
        {
            var usersInGroup = await graphClient.Groups[groupId].Members.GraphUser.GetAsync();
            return usersInGroup;
        }

        public async Task<ApplicationCollectionResponse> GetApplicationsInGroup(GraphServiceClient graphClient, string groupId)
        {
            try
            {
                var applicationsInGroup = await graphClient.Groups[groupId].Members.GraphApplication.GetAsync();
                return applicationsInGroup ?? throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.Message);
            }
        }
    }
}
