using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using Types = mud.Network.Types;

namespace v2
{
    
    public struct Log
    {
        public BigInteger BlockNumber;
        public int LogIndex;
    }

    public struct GroupLogsByBlockNumberResult
    {
        public BigInteger BlockNumber;
        public FilterLog[] Logs;
    }
    public class SyncWorker : IDisposable 
    {
        private readonly ReplaySubject<Types.NetworkTableUpdate> _outputStream = new();
        public IObservable<Types.NetworkTableUpdate> OutputStream => _outputStream;
        private readonly CompositeDisposable _disposables = new();

        public IObservable<List<GroupLogsByBlockNumberResult>> StartSync(IObservable<Block> blockStream, string storeContractAddress, string account, string rpcUrl,
            int initialBlockNumber)
        {
            Debug.Log("Fetching initial state from indexer...");

            BigInteger endBlock = 0;

            var latestBlockNumber = blockStream.Select(block =>
            {
                if (block.@params?.result.number == null) return 0;
                return Common.ConvertFromHexUnsigned(block.@params.result.number);
            }).Do(blockNum =>
            {
                endBlock = blockNum;
            }).Share();
            
            BigInteger startBlock = initialBlockNumber;

            var blockLogs = latestBlockNumber.Select(_ => (Start: startBlock, End: endBlock))
                .SelectMany(blockRange =>
                {
                    if (blockRange.Start > blockRange.End) return Observable.Empty<FilterLog>();

                    return Common.AsyncEnumerableToObservable(Sync.FetchLogs(storeContractAddress, account, rpcUrl,
                            blockRange.Start, blockRange.End))
                        .Do(_ => startBlock = blockRange.End + 1);
                });
            
            var orderedLogs = blockLogs.ToList().Select(logs =>
            {
                var blockNumbers = logs.Select(log => log.BlockNumber).Distinct().ToList();
                blockNumbers.Sort();

                var groupedBlocks = new List<GroupLogsByBlockNumberResult>();

                foreach (var blockNumber in blockNumbers)
                {
                    var blockNumberLogs = logs.Where(log => log.BlockNumber == blockNumber).ToList();

                    // bl.Sort((a, b) => a.LogIndex.CompareTo(b.LogIndex));
                    blockNumberLogs.Sort();

                    if (blockNumberLogs.Count > 0)
                    {
                        groupedBlocks.Add(new GroupLogsByBlockNumberResult
                            { BlockNumber = blockNumber, Logs = blockNumberLogs.ToArray() });
                    }
                }

                return groupedBlocks;
            });

            return orderedLogs;
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
