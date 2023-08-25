#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NLog;
using static mud.Network.ProviderUtils;

namespace mud.Network
{
    public class ProviderPair
    {
        public Web3 Json { get; set; }
        public StreamingWebSocketClient? Ws { get; set; }

        public void Deconstruct(out Web3 json, out StreamingWebSocketClient? ws)
        {
            json = Json;
            ws = Ws;
        }
    }

    public class ProviderConfig
    {
        public string JsonRpcUrl { get; set; }
        public string? WsRpcUrl { get; set; }
        public bool? SkipNetworkCheck { get; set; }
    }

    public static class Providers
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<ProviderPair> CreateReconnectingProviders(Account account, ProviderConfig config, CancellationToken cancellationToken)
        {
            var jsonProvider = new Web3(account, config.JsonRpcUrl);
            StreamingWebSocketClient? wsClient = null;
            
            if (config.WsRpcUrl != null)
            {
                Logger.Debug("Creating websocket client...");
                wsClient = new StreamingWebSocketClient(config.WsRpcUrl);
                Logger.Debug("Websocket client created.");
            }

            // await CallWithRetry(async () =>
            // {
            //     if (config.SkipNetworkCheck is false or null)
            //     {
            //         await EnsureNetworkIsUp(jsonProvider, wsClient, cancellationToken);
            //     }
            // }, 10, 1000, cancellationToken);

            return new ProviderPair
            {
                Json = jsonProvider,
                Ws = wsClient
            };
        }
    }
}
