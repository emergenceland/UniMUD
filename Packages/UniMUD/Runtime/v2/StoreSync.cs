using System;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using UniRx;
using UniRx.Operators;
using UnityEngine;
using Types = mud.Network.Types;

namespace v2
{
    public class StoreSync : IDisposable
    {
        private readonly ReplaySubject<Types.NetworkTableUpdate> _outputStream = new();
        public IObservable<Types.NetworkTableUpdate> OutputStream => _outputStream;
        private readonly CompositeDisposable _disposables = new();

        public IObservable<StorageAdapterBlock> StartSync(BlockStream blockStream, string storeContractAddress,
            string account, string rpcUrl,
            int initialBlockNumber, string wsRpcUrl)
        {
            // TODO: fetch initial state from indexer
            Debug.Log("Fetching initial state from indexer...");

            BigInteger endBlock = 0;
            var latestBlock = blockStream.WatchBlocks(wsRpcUrl);
            var latestBlockNumber = latestBlock.Select(block =>
            {
                if (block.@params?.result.number == null) return 0;
                return Common.HexToBigInt(block.@params.result.number);
            }).Do(blockNum => { endBlock = blockNum; }).Share();

            BigInteger startBlock = initialBlockNumber;
            var blockLogs = latestBlockNumber.Select(_ => (Start: startBlock, End: endBlock))
                .SelectMany(blockRange =>
                {
                    if (blockRange.Start > blockRange.End) return Observable.Empty<FilterLog[]>();

                    return Common.AsyncEnumerableToObservable(Sync.FetchLogs(storeContractAddress, account, rpcUrl,
                            blockRange.Start, blockRange.End))
                        .Do(_ => startBlock = blockRange.End + 1);
                });

            var orderedLogs = Sync.ToStorageAdapterBlock(blockLogs).Share();
            return orderedLogs;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
