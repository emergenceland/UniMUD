using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using mud.Network.IStore.ContractDefinition;
using mud.Network.schemas;
using mud.Network.schemas;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using Newtonsoft.Json;
using UnityEngine;
using Types = mud.Network.Types;

namespace v2
{
    public partial class Sync
    {
        public static async IAsyncEnumerable<Types.NetworkTableUpdate> FetchLogs(string storeContractAddress,
            string account,
            string rpcUrl, BigInteger fromBlock, BigInteger toBlock)
        {
            var storeEvents = new List<EventABI>
            {
                new StoreSetFieldEventDTO().GetEventABI(),
                new StoreSetRecordEventDTO().GetEventABI(),
                new StoreDeleteRecordEventDTO().GetEventABI(),
                new StoreEphemeralRecordEventDTO().GetEventABI()
            };
            // var events = storeEvents.Select(e => e.CreateFilterInput(e, new[] { storeContractAddress },
            //     new BlockParameter((ulong)fromBlock), new BlockParameter((ulong)toBlock))).ToList();
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

            // Sort logs by block number and transaction index
            allLogs.Sort((log1, log2) =>
            {
                int blockComparison = log1.BlockNumber.Value.CompareTo(log2.BlockNumber.Value);
                return blockComparison != 0
                    ? blockComparison
                    : log1.TransactionIndex.Value.CompareTo(log2.TransactionIndex.Value);
            });

            var filterLogs = allLogs.ToArray();
            Debug.Log(JsonConvert.SerializeObject(filterLogs));
            // var allUpdates = new List<mud.Network.Types.NetworkTableUpdate>();

            for (var i = 0; i < filterLogs.Length; i++)
            {
                var fl = filterLogs[i];
                // Since ECS events are coming in ordered over the wire, we check if the following event has a
                // different transaction then the current, which would mean an event associated with another
                // tx
                var lastEventInTx = (i == filterLogs.Length - 1) ||
                                    (filterLogs[i + 1].TransactionHash != fl.TransactionHash);
                var update = await BlockLogsToStorage(fl, storeContractAddress, account, lastEventInTx);
                yield return update;
                // allUpdates.Add(update);
            }

            // Debug.Log(JsonConvert.SerializeObject(allUpdates));
            // return allUpdates;
        }
    }
}
