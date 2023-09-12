using System;
using System.Numerics;
using UniRx;
using UnityEngine;
using Types = mud.Network.Types;

namespace v2
{
    public class SyncWorker : IDisposable 
    {
        private readonly ReplaySubject<Types.NetworkTableUpdate> _outputStream = new();
        public IObservable<Types.NetworkTableUpdate> OutputStream => _outputStream;
        private readonly CompositeDisposable _disposables = new();

        public IObservable<Types.NetworkTableUpdate> StartSync(string storeContractAddress, string account, string rpcUrl,
            int initialBlockNumber)
        {
            Debug.Log("Fetching initial state from indexer...");

            BigInteger endBlock = 0;

            var blockStream = new BlockStream().AddTo(_disposables).WatchBlocks();
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
                    if (blockRange.Start > blockRange.End) return Observable.Empty<Types.NetworkTableUpdate>();

                    return Common.AsyncEnumerableToObservable(Sync.FetchLogs(storeContractAddress, account, rpcUrl,
                            blockRange.Start, blockRange.End))
                        .Do(_ => startBlock = blockRange.End + 1);
                });

            return blockLogs;
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
