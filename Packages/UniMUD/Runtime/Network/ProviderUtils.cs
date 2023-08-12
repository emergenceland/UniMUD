#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.Web3;

namespace mud.Network
{
    public class ProviderUtils
    {
        public static async UniTask EnsureNetworkIsUp(Web3 provider, StreamingWebSocketClient? wsClient,
            CancellationToken cancellationToken)
        {
            async UniTask NetworkInfoPromise()
            {
                if (cancellationToken.IsCancellationRequested) return;
                var providerBlockNumberTask = provider.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                // TODO: kinda weird 
                var wssProviderBlockNumberTask = wsClient != null
                    ? wsClient.IsStarted ? Task.FromResult(true) : Task.FromResult(false)
                    : Task.FromResult(true);
                await Task.WhenAll(providerBlockNumberTask, wssProviderBlockNumberTask);
            }

            await CallWithRetry(NetworkInfoPromise, 10, 1000, cancellationToken);
        }

        public static async UniTask CallWithRetry(Func<UniTask> action, int maxRetries, int delay,
            CancellationToken cancellationToken)
        {
            var retries = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}
