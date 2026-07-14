using System.Collections.Generic;
using Core.Models;

namespace Core.Bootstrap
{
    public static partial class GameConfigRoute
    {
        public const string ActiveGameConfigs = "/api/v1/internal/game-configs/active";
    }
}

namespace Core.ApiClientExtensions
{
    using Core.API;
    using Core.Bootstrap;
    using Core.Models;
    using System.Threading.Tasks;

    public static class GameConfigApiExtensions
    {
        public static Task<GamesConfigResponse> GetActiveGameConfigs(this ApiClient client)
        {
            if (client == null) return Task.FromResult<GamesConfigResponse>(null);
            return client.Get<GamesConfigResponse>(GameConfigRoute.ActiveGameConfigs);
        }
    }
}
