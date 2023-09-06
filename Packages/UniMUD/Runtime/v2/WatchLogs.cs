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
            return Observable.Create<List<mud.Network.Types.NetworkTableUpdate>>(async observer =>
            {
                var lastFetchedBlock = await Common.GetLatestBlockNumber(rpcUrl);

                return UniRx.Observable
                    .ObserveOnMainThread(Observable.Interval(TimeSpan.FromMilliseconds(pollingInterval))).Subscribe(
                        async _ =>
                        {
                            try
                            {
                                Debug.Log("hee");
                                var latestBlock = await Common.GetLatestBlockNumber(rpcUrl);

                                if (latestBlock <= lastFetchedBlock)
                                {
                                    // No new blocks since the last fetch, so we won't notify the observer of any new data.
                                    return;
                                }

                                var newFromBlock = BigInteger.Add(BigInteger.One, lastFetchedBlock);
                                var logs = await FetchLogs(storeContractAddress, account, rpcUrl, newFromBlock,
                                    latestBlock);

                                lastFetchedBlock = latestBlock;
                                Debug.Log("Last fetched block: " + lastFetchedBlock);
                                observer.OnNext(logs);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Error while fetching logs: {ex}");
                                // observer.OnError(ex);
                            }
                        });
            });
        }
    }
}
