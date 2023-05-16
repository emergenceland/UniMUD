using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.Web3;

namespace mud.Network
{
    public class ProviderUtils
    {
        public static async Task EnsureNetworkIsUp(Web3 provider, [CanBeNull] StreamingWebSocketClient wsClient)
        {
            async Task NetworkInfoPromise()
            {
                var providerBlockNumberTask = provider.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                // TODO: kinda weird 
                var wssProviderBlockNumberTask = wsClient != null
                    ? wsClient.IsStarted ? Task.FromResult(true) : Task.FromResult(false)
                    : Task.FromResult(true);
                await Task.WhenAll(providerBlockNumberTask, wssProviderBlockNumberTask);
            }

            await CallWithRetry(NetworkInfoPromise, 10, 1000);
        }

        public static async Task CallWithRetry(Func<Task> action, int maxRetries, int delay)
        {
            var retries = 0;
            while (true)
            {
                try
                {
                    await action();
                    break;
                }
                catch
                {
                    retries++;
                    if (retries >= maxRetries)
                    {
                        throw;
                    }

                    await Task.Delay(delay);
                }
            }
        }
    }
}
