using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using mud.IStore.ContractDefinition;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using UniRx;

namespace mud
{
    public struct StorageAdapterBlock
    {
        public BigInteger BlockNumber;
        public FilterLog[] Logs;
    }

    public struct BlockRangeType
    {
        public BigInteger StartBlock;
        public BigInteger EndBlock;
    }

    public struct FetchLogsResult
    {
        public BigInteger FromBlock;
        public BigInteger ToBlock;
        public FilterLog[] Logs;
    }

    public static partial class Sync
    {
        public static IObservable<FetchLogsResult> BlockRangeToLogs(this IObservable<BlockRangeType> source,
            string contractAddress, string rpcUrl)
        {
           BigInteger fromBlock = default;
           BigInteger toBlock = default; 
           
            return source.Scan(
                    seed: new BlockRangeType(),
                    accumulator: (acc, range) =>
                    {
                        fromBlock = fromBlock == null ? range.StartBlock : fromBlock;
                        toBlock = range.EndBlock;
                        return acc;
                    }
                )
                .SelectMany(acc =>
                {
                    if (fromBlock > toBlock)
                        return Observable.Empty<FetchLogsResult>();

                    return Common
                        .AsyncEnumerableToObservable(FetchLogs(contractAddress, rpcUrl, fromBlock, toBlock))
                        .Do(_ => { fromBlock = toBlock + 1; });
                }).Concat();
        }

        public static async IAsyncEnumerable<FetchLogsResult> FetchLogs(string storeContractAddress,
            string rpcUrl, BigInteger? fromBlock, BigInteger? toBlock)
        {
            var storeEvents = new List<EventABI>
            {
                new StoreSpliceDynamicDataEventDTO().GetEventABI(),
                new StoreSpliceStaticDataEventDTO().GetEventABI(),
                new IStore.ContractDefinition.StoreSetRecordEventDTO().GetEventABI(),
                new IStore.ContractDefinition.StoreDeleteRecordEventDTO().GetEventABI(),
                // new StoreEphemeralRecordEventDTO().GetEventABI()
            };
            var events = storeEvents.Select(e => e.CreateFilterInput()).ToList();
            var from = fromBlock ?? 0;
            if (toBlock != null)
            {
                var blockRange = BigInteger.Min(BigInteger.Parse("1000"), (BigInteger)(toBlock - from));
                // var retryCount = 0;
                // var maxRetryCount = 3;

                while (from <= toBlock)
                {
                    // try
                    // {
                    var to = from + blockRange;
                    events.ForEach(fi =>
                    {
                        fi.Address = new[] { storeContractAddress };
                        if (fromBlock != null) fi.FromBlock = new BlockParameter((ulong)from);
                        fi.ToBlock = new BlockParameter((ulong)to);
                    });
                    var getLogsRequest = new EthGetLogsUnityRequest(rpcUrl);
                    await UniTask.SwitchToMainThread();
                    var results = new FilterLog[] { };

                    foreach (var fi in events)
                    {
                        await getLogsRequest.SendRequest(fi).ToUniTask();
                        results = results.Concat(getLogsRequest.Result).ToArray();
                    }

                    yield return new FetchLogsResult
                    {
                        FromBlock = from,
                        ToBlock = to,
                        Logs = results
                    };

                    from = to + 1;
                    blockRange = BigInteger.Min(BigInteger.Parse("1000"), (BigInteger)(toBlock - from));
                }
            }

            // catch (Exception error)
            // {
            //     if (error.Message.Contains("rate limit exceeded") && retryCount < maxRetryCount)
            //     {
            //         var seconds = 2 * retryCount;
            //         Debug.Log($"Rate limit exceeded, retrying in {seconds} seconds...");
            //         await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            //         retryCount++;
            //         continue;
            //     }
            //     throw;
            // }

            // }
        }

        public static IEnumerable<StorageAdapterBlock> ToStorageAdapterBlock(FilterLog[] logs, BigInteger toBlock)
        {
            var blockNumbers = logs.Select(log => log.BlockNumber).Distinct().ToList();
            blockNumbers.Sort((a, b) => a.Value.CompareTo(b.Value));

            var groupedBlocks = blockNumbers.Select(blockNumber =>
            {
                var blockNumberLogs = logs.Where(log => log.BlockNumber == blockNumber).ToList();
                blockNumberLogs.Sort((a, b) => a.LogIndex.Value.CompareTo(b.LogIndex.Value));

                if (blockNumberLogs.Count > 0)
                {
                    return new StorageAdapterBlock
                        { BlockNumber = blockNumber, Logs = blockNumberLogs.ToArray() };
                }

                return default;
            });
            var groupedBlocksList = groupedBlocks.ToList();
            var lastBlockNumber = blockNumbers.Count > 0 ? blockNumbers[^1] : null;
            if (lastBlockNumber == null || toBlock > lastBlockNumber)
            {
                groupedBlocksList.Add(new StorageAdapterBlock()
                {
                    BlockNumber = toBlock,
                    Logs = new FilterLog[] { }
                });
            }

            return groupedBlocksList;
        }
    }
}
