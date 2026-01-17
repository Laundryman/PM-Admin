using PMApplication.Dtos;
using System.Configuration;

namespace PM_AdminApp.Server.Settings
{
    public interface IGraphSettings
    {
        string ClientId { get; }
        string TenantId { get; }
        string ClientSecret { get; }
        string Authority { get; }
    }

    public class GraphSettings : IGraphSettings
    {
        private readonly IConfiguration _configuration;


        public GraphSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string ClientId => _configuration["Graph:ClientId"] ?? string.Empty;
        public string TenantId => _configuration["Graph:TenantId"] ?? string.Empty;
        public string ClientSecret => _configuration["Graph:ClientSecret"] ?? string.Empty;
        public string Authority => _configuration["Graph:Authority"] ?? string.Empty;
    }
}
