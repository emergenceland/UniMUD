using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using v2.IStore.ContractDefinition;
using StoreDeleteRecordEventDTO = v2.IStore.ContractDefinition.StoreDeleteRecordEventDTO;
// using StoreEphemeralRecordEventDTO = v2.IStore.ContractDefinition.StoreEphemeralRecordEventDTO;
// using StoreSetFieldEventDTO = v2.IStore.ContractDefinition.StoreSetFieldEventDTO;
using StoreSetRecordEventDTO = v2.IStore.ContractDefinition.StoreSetRecordEventDTO;
using Types = mud.Network.Types;

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
                var blockNumbers = logs.Select(log => log.BlockNumber).Distinct().ToList();
                blockNumbers.Sort();

                var groupedBlocks = new List<StorageAdapterBlock>();

                foreach (var blockNumber in blockNumbers)
                {
                    var blockNumberLogs = logs.Where(log => log.BlockNumber == blockNumber).ToList();

                    blockNumberLogs.Sort((a, b) => a.LogIndex.Value.CompareTo(b.LogIndex.Value));

                    if (blockNumberLogs.Count > 0)
                    {
                        groupedBlocks.Add(new StorageAdapterBlock
                            { BlockNumber = blockNumber, Logs = blockNumberLogs.ToArray() });
                    }
                }

                return groupedBlocks;
            });
        }
    }
}
