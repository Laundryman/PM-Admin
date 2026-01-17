using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace PM_AdminApp.Server.GraphApi.Interfaces
{

        public interface IGraphService
        {
            Task<string> GetAccessTokenConfidentialClientAsync(string clientId, string tenantId, string clientSecret, string authority);
            Task<string> GetAccessTokenWithClientCredentialAsync(string clientId, string tenantId, string clientSecret, CancellationToken cancellationToken = default);
            Task<string> GetAccessTokenByUserNamePassword(string clientId, ICollection<string> scopes, string authority, string userName, string password);
            Task<GraphServiceClient> GetGraphServiceClient(string clientId, string tenantId, string clientSecret, string token);
            Task<User?> GetUserIfExists(GraphServiceClient graphClient, string userEmail);
            Task<User?> CreateUserAsync(GraphServiceClient graphClient, string? displayName, string userPrincipalName, string password);
            Task<List<User>>? GetUserListAsync(GraphServiceClient graphClient);
            Task<PageIterator<User, UserCollectionResponse>>? GetPageIterator(GraphServiceClient graphClient);
            Task<List<User>>? GetUsersWithBatchRequest(GraphServiceClient graphClient);
            Task<User> GetCurrentlyLoggedInUserInfo(GraphServiceClient graphClient);
            Task<int?> GetUsersCount(GraphServiceClient graphClient);
            Task<UserCollectionResponse> GetUsersInGroup(GraphServiceClient graphClient, string groupId);
            Task<ApplicationCollectionResponse> GetApplicationsInGroup(GraphServiceClient graphClient, string groupId);
        }
}
