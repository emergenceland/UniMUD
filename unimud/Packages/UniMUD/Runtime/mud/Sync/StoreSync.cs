using System;
using System.Numerics;
using UniRx;
using UnityEngine;

namespace mud
{
    public enum SyncStep
    {
        Initialize,
        Snapshot,
        Rpc,
        Live
    }

    public struct OnProgressOpts
    {
        public SyncStep step;
        public int percentage;
        public BigInteger latestBlockNumber;
        public BigInteger lastBlockNumberProcessed;
        public string message;
    }

    public class StoreSync : IDisposable
    {
        public readonly ReplaySubject<RecordUpdate> onUpdate = new();
        private readonly CompositeDisposable _disposables = new();

        // TODO: make RxDataStore something like IStorageAdapter
        public IObservable<StorageAdapterBlock> StartSync(RxDatastore ds, IObservable<Block> blockStream,
            string storeContractAddress,
            string rpcUrl,
            BigInteger initialBlockNumber, int streamStartBlockNumber, Action<OnProgressOpts> onProgress = null)
        {
            // TODO: fetch initial state from indexer

            BigInteger? endBlock = null;
            BigInteger? startBlock = null;

            // TODO: Derive from initialState or deploy event
            var startBlockObservable = Observable.Return(streamStartBlockNumber).Replay(1).RefCount();

            Debug.Log($"Fetching gap state events from...{initialBlockNumber}-{streamStartBlockNumber}");
            var blockRangeObservable = Observable.Return(new BlockRangeType
            {
                StartBlock = initialBlockNumber,
                EndBlock = streamStartBlockNumber
            });
            
            IObservable<StorageAdapterBlock> gapStateEvents = blockRangeObservable.BlockRangeToLogs(storeContractAddress, rpcUrl).SelectMany(block =>
                    Sync.ToStorageAdapterBlock(block.Logs, block.ToBlock).ToObservable()).Merge().Replay(1).RefCount();
            
            IObservable<StorageAdapterBlock> initialLogs = gapStateEvents.Select(block =>
            {
                RxStorageAdapter.ToStorage(onUpdate, ds, block);
                return block;
            }).Concat().Replay(1).RefCount();
            
            IObservable<BigInteger> latestBlockNumber = blockStream.Replay(1).RefCount().Select(block =>
            {
                if (block.@params?.result.number == null) return 0;
                return Common.HexToBigInt(block.@params.result.number);
            }).Replay(1).RefCount();

            IObservable<StorageAdapterBlock> blockLogs = startBlockObservable.CombineLatest(latestBlockNumber,
                    (start, end) => new BlockRangeType { StartBlock = start, EndBlock = end })
                .Do(blockRange =>
                {
                    startBlock = blockRange.StartBlock;
                    endBlock = blockRange.EndBlock;
                }).BlockRangeToLogs(storeContractAddress, rpcUrl)
                .SelectMany(block => Sync.ToStorageAdapterBlock(block.Logs, block.ToBlock).ToObservable()).Merge()
                .Share();

            BigInteger lastBlockNumberProcessed;
            IObservable<StorageAdapterBlock> storedBlockLogs = initialLogs.Concat(blockLogs.Select(block =>
            {
                RxStorageAdapter.ToStorage(onUpdate, ds, block);
                return block;
            }).Do(storageBlock =>
            {
                lastBlockNumberProcessed = storageBlock.BlockNumber;

                if (startBlock == null || endBlock == null) return;
                if (storageBlock.BlockNumber < endBlock)
                {
                    var totalBlocks = endBlock - startBlock;
                    var processedBlocks = lastBlockNumberProcessed - startBlock;
                    var percentage = (int)((processedBlocks * 1000) / totalBlocks) / 1000;
                    // TODO: fix percentage
                    onProgress?.Invoke(new OnProgressOpts
                    {
                        step = SyncStep.Rpc,
                        percentage = percentage,
                        latestBlockNumber = endBlock ?? 0,
                        lastBlockNumberProcessed = lastBlockNumberProcessed,
                        message = "Hydrating from RPC"
                    });
                }
                else
                {
                    onProgress?.Invoke(new OnProgressOpts
                    {
                        step = SyncStep.Live,
                        percentage = 100,
                        latestBlockNumber = endBlock ?? 0,
                        lastBlockNumberProcessed = lastBlockNumberProcessed,
                        message = "All caught up"
                    });
                }
            })).Share();
            return storedBlockLogs;
        }

        // TODO: this doesn't do anything
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
