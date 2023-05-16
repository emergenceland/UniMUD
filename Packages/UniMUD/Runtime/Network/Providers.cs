#nullable enable
using System.Threading.Tasks;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
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
        public static async Task<ProviderPair> CreateReconnectingProviders(Account account, ProviderConfig config)
        {
            var jsonProvider = new Web3(account, config.JsonRpcUrl);
            StreamingWebSocketClient? wsClient = null;
            
            if (config.WsRpcUrl != null)
            {
                Debug.Log("Creating websocket client...");
                wsClient = new StreamingWebSocketClient(config.WsRpcUrl);
                Debug.Log("Websocket client created.");
            }

            await CallWithRetry(async () =>
            {
                if (config.SkipNetworkCheck is false or null)
                {
                    await EnsureNetworkIsUp(jsonProvider, wsClient);
                }
            }, 10, 1000);

            return new ProviderPair
            {
                Json = jsonProvider,
                Ws = wsClient
            };
        }
    }
}
