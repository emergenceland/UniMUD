using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using Nethereum.Hex.HexTypes;
using UnityEngine;

namespace v2
{
    public partial class Sync
    {
        // repeatedly calls FetchLogs at a defined polling interval
        public static async UniTask<IObservable<List<mud.Network.Types.NetworkTableUpdate>>> WatchLogs(
            string storeContractAddress, string account, string rpcUrl, int pollingInterval)
        {
            
            var lastFetchedBlock = await Common.GetLatestBlockNumber(rpcUrl);

            var observable = Observable.Interval(TimeSpan.FromMilliseconds(pollingInterval))
                .SelectMany(async _ =>
                {
                    Debug.Log("hee");
                    var latestBlock = await Common.GetLatestBlockNumber(rpcUrl);

                    if (latestBlock <= lastFetchedBlock)
                    {
                        // No new blocks since the last fetch, so return an empty list.
                        return new List<mud.Network.Types.NetworkTableUpdate>();
                    }

                    // Fetch logs between the last fetched block and the latest block.
                    var newFromBlock = BigInteger.Add(BigInteger.One, lastFetchedBlock);
                    var logs = await FetchLogs(storeContractAddress, account, rpcUrl, newFromBlock, latestBlock);

                    // Update the last fetched block number.
                    lastFetchedBlock = latestBlock;
                    Debug.Log("Last fetched block: " + lastFetchedBlock);

                    return logs;
                });
    
            return observable;
        }
    }
}
