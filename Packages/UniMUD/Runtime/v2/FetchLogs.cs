using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using mud.Network.IStore.ContractDefinition;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using Types = mud.Network.Types;

namespace v2
{
    public partial class Sync
    {
        public static async IAsyncEnumerable<FilterLog[]> FetchLogs(string storeContractAddress,
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

            // foreach (var fl in filterLogs)
            // {
            //     // Since ECS events are coming in ordered over the wire, we check if the following event has a
            //     // different transaction then the current, which would mean an event associated with another
            //     // tx
            //     // var lastEventInTx = (i == filterLogs.Length - 1) ||
            //     //                     (filterLogs[i + 1].TransactionHash != fl.TransactionHash);
            //     // var update = await BlockLogsToStorage(fl, storeContractAddress, account, lastEventInTx);
            //     yield return fl;
            // }
        }
    }
}
