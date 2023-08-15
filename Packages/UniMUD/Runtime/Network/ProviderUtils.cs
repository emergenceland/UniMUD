#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.Web3;
using UnityEngine;

namespace mud.Network
{
    public class ProviderUtils
    {
        public static async UniTask EnsureNetworkIsUp(Web3 provider, StreamingWebSocketClient? wsClient,
            CancellationToken cancellationToken)
        {
            async UniTask NetworkInfoPromise()
            {
                Debug.Log("NetworkInfoPromise()");
                // TODO: put back
                // if (cancellationToken.IsCancellationRequested) return;
                var providerBlockNumberTask = provider.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                // TODO: kinda weird 
                var wssProviderBlockNumberTask = wsClient != null
                    ? wsClient.IsStarted ? Task.FromResult(true) : Task.FromResult(false)
                    : Task.FromResult(true);
                await Task.WhenAll(providerBlockNumberTask, wssProviderBlockNumberTask);
                Debug.Log("Done with NetworkInfoPromise()");
            }

            // await CallWithRetry(NetworkInfoPromise, 10, 1000, cancellationToken);
            await NetworkInfoPromise();
        }

        public static async UniTask CallWithRetry(Func<UniTask> action, int maxRetries, int delay,
            CancellationToken cancellationToken)
        {
            var retries = 0;
            while (true)
            {
                // TODO: put back
                // cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await action();
                    break;
                }
                catch
                {
                    Debug.Log("Retrying...");
                    retries++;
                    if (retries >= maxRetries)
                    {
                        throw;
                    }

                    // TODO: put back
                    // await Task.Delay(delay, cancellationToken);
                    await Task.Delay(delay);
                }
            }
        }
    }
}
