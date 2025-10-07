using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Utils;

namespace SPTLeaderboard;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "github.harmonyzt.SPTLeaderboard";
    public override string Name { get; init; } = "SPTLeaderboard";
    public override string Author { get; init; } = "Harmony";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");


    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "MIT";
}

[Injectable]
public class CustomStaticRouter : StaticRouter
{
    private static HttpResponseUtil _httpResponseUtil;

    public CustomStaticRouter(
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil) : base(
        jsonUtil,
        GetCustomRoutes()
    )
    {
        _httpResponseUtil = httpResponseUtil;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                "/client/game/profile/items/moving",
                async (
                    url,
                    info,
                    sessionId,
                    output
                ) => await HandleRoute(url, info, sessionId, output)
            )
        ];
    }

    private static ValueTask<object> HandleRoute(string url, IRequestData info, MongoId sessionId, object output)
    {
        Console.WriteLine("Hooked moving items..?");

        return new ValueTask<object>(output);
    }
}
