using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cysharp.Threading.Tasks;
using mud.Network.IStore;
using mud.Network.IStore.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using UnityEngine;

namespace mud.Network
{
    public static partial class Sync
    {
        private static List<NewFilterInput> CreateFilterInputs(IStoreService store)
        {
            var storeEvents = new List<EventBase>
            {
                store.ContractHandler.GetEvent<StoreSetFieldEventDTO>(),
                store.ContractHandler.GetEvent<StoreSetRecordEventDTO>(),
                store.ContractHandler.GetEvent<StoreDeleteRecordEventDTO>(),
                store.ContractHandler.GetEvent<StoreEphemeralRecordEventDTO>()
            };

            return storeEvents.Select((e) => e.CreateFilterInput()).ToList();
        }

        public static async UniTask<List<Types.NetworkTableUpdate>> FetchStoreEventsInRange(IStoreService store,
            int fromBlock, int toBlock)
        {
            var filterInput = CreateFilterInputs(store);

            // Set the block range for the filter inputs
            filterInput.ForEach(fi =>
            {
                fi.FromBlock = new BlockParameter((ulong)fromBlock);
                fi.ToBlock = new BlockParameter((ulong)toBlock);
            });

            // Fetch logs for each filter input and aggregate them
            var allLogs = new List<FilterLog>();
            EthGetLogs ethGetLogs = new EthGetLogs(store.ContractHandler.EthApiContractService.Client);

            foreach (var fi in filterInput)
            {
                var logs = await ethGetLogs.SendRequestAsync(fi);
                allLogs.AddRange(logs);
            }

            // Sort logs by block number and transaction index
            allLogs.Sort((log1, log2) =>
            {
                int blockComparison = log1.BlockNumber.Value.CompareTo(log2.BlockNumber.Value);
                return blockComparison != 0
                    ? blockComparison
                    : log1.TransactionIndex.Value.CompareTo(log2.TransactionIndex.Value);
            });

            var filterLogs = allLogs.ToArray();
            var allUpdates = new List<Types.NetworkTableUpdate>();

            for (var i = 0; i < filterLogs.Length; i++)
            {
                var fl = filterLogs[i];
                // Since ECS events are coming in ordered over the wire, we check if the following event has a
                // different transaction then the current, which would mean an event associated with another
                // tx
                var lastEventInTx = (i == filterLogs.Length - 1) ||
                                    (filterLogs[i + 1].TransactionHash != fl.TransactionHash);
                var update = await EcsEventFromLog(fl, store, lastEventInTx);
                allUpdates.Add(update);
            }


            return allUpdates;
        }

        public static async UniTask<(Subject<Types.NetworkTableUpdate>, IDisposable)> SubscribeToStoreEvents(
            IStoreService store,
            StreamingWebSocketClient client)
        {
            await client.StartAsync();
            var disposables = new CompositeDisposable();
            disposables.Add(Disposable.Create(client.Dispose));

            var txStream = new Subject<Types.NetworkTableUpdate>();

            var filterInput = CreateFilterInputs(store);

            var observables = new List<IObservable<Types.NetworkTableUpdate>>();

            Debug.Log("Shaddap you face");
            foreach (var filter in filterInput)
            {
                var subscription = new EthLogsObservableSubscription(client);

                var observable = subscription.GetSubscriptionDataResponsesAsObservable()
                    .SelectMany(async log =>
                    {
                        var update = await EcsEventFromLog(log, store, false);
                        return update;
                    });
                Debug.Log("observable");
                observables.Add(observable);
                await subscription.SubscribeAsync(filter);
                var subscriptionDisposable = Disposable.Create(() => subscription.UnsubscribeAsync());
                disposables.Add(subscriptionDisposable);
            }

            var mergedObservable = observables.Merge();
            var mergeSubscriptionDisposable = mergedObservable.Subscribe(txStream.OnNext);
            disposables.Add(mergeSubscriptionDisposable);

            return (txStream, disposables);
        }
    }
}
