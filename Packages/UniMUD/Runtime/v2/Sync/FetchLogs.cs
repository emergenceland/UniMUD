using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using UniRx;
using v2.IStore.ContractDefinition;
using StoreDeleteRecordEventDTO = v2.IStore.ContractDefinition.StoreDeleteRecordEventDTO;
using StoreSetRecordEventDTO = v2.IStore.ContractDefinition.StoreSetRecordEventDTO;

namespace v2
{
    public struct StorageAdapterBlock
    {
        public BigInteger BlockNumber;
        public FilterLog[] Logs;
    }

    public partial class Sync
    {
        public static async IAsyncEnumerable<FilterLog[]> FetchLogs(string storeContractAddress,
            string account,
            string rpcUrl, BigInteger fromBlock, BigInteger toBlock)
        {
            var storeEvents = new List<EventABI>
            {
                new StoreSpliceDynamicDataEventDTO().GetEventABI(),
                new StoreSpliceStaticDataEventDTO().GetEventABI(),
                new StoreSetRecordEventDTO().GetEventABI(),
                new StoreDeleteRecordEventDTO().GetEventABI(),
                // new StoreEphemeralRecordEventDTO().GetEventABI()
            };
            var events = storeEvents.Select(e => e.CreateFilterInput()).ToList();

            events.ForEach(fi =>
            {
                fi.Address = new[] { storeContractAddress };
                fi.FromBlock = new BlockParameter((ulong)fromBlock);
                fi.ToBlock = new BlockParameter((ulong)toBlock);
            });

            var allLogs = new List<FilterLog>();
            var getLogsRequest = new EthGetLogsUnityRequest(rpcUrl);

            foreach (var fi in events)
            {
                await UniTask.SwitchToMainThread();
                await getLogsRequest.SendRequest(fi).ToUniTask();
                allLogs.AddRange(getLogsRequest.Result);
            }

            yield return allLogs.ToArray();
        }

        public static IObservable<StorageAdapterBlock> ToStorageAdapterBlock(IObservable<FilterLog[]> blockLogs)
        {
            return blockLogs.SelectMany(logs =>
            {
                return Observable.Create<StorageAdapterBlock>(observer =>
                {
                    var blockNumbers = logs.Select(log => log.BlockNumber).Distinct().ToList();
                    blockNumbers.Sort((a, b) => a.Value.CompareTo(b.Value));

                    foreach (var blockNumber in blockNumbers)
                    {
                        var blockNumberLogs = logs.Where(log => log.BlockNumber == blockNumber).ToList();

                        blockNumberLogs.Sort((a, b) => a.LogIndex.Value.CompareTo(b.LogIndex.Value));

                        if (blockNumberLogs.Count > 0)
                        {
                            var storageAdapterBlock = new StorageAdapterBlock
                                { BlockNumber = blockNumber, Logs = blockNumberLogs.ToArray() };
                            observer.OnNext(storageAdapterBlock);
                        }
                    }

                    observer.OnCompleted();
                    return Disposable.Empty;
                });
            });
        }
    }
}
