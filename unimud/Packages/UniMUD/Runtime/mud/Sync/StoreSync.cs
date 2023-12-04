using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
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

    public struct IndexerQuery
    {
        public int chainId;
        public string address;
        public SyncFilter[] filters;
    }

    public class StoreSync : IDisposable
    {
        public readonly ReplaySubject<RecordUpdate> onUpdate = new();
        private readonly CompositeDisposable _disposables = new();

        // TODO: make RxDataStore something like IStorageAdapter
        public IObservable<StorageAdapterBlock> StartSync(RxDatastore ds, IObservable<Block> blockStream,
            string storeContractAddress,
            string rpcUrl,
            int chainId,
            BigInteger initialBlockNumber, Action<OnProgressOpts> onProgress = null,
            string indexerUrl = null)
        {
            onProgress?.Invoke(new OnProgressOpts
            {
                step = SyncStep.Snapshot,
                percentage = 0,
                latestBlockNumber = 0,
                lastBlockNumberProcessed = 0,
                message = "Getting snapshot"
            });

            IObservable<StorageAdapterBlock?> initialBlockLogs =
                Snapshot.GetSnapshot(indexerUrl, chainId, storeContractAddress).ToObservable();

            onProgress?.Invoke(new OnProgressOpts
            {
                step = SyncStep.Snapshot,
                percentage = 100,
                latestBlockNumber = 0,
                lastBlockNumberProcessed = 0,
                message = "Received snapshot"
            });

            IObservable<StorageAdapterBlock> storedInitialBlockLogs = initialBlockLogs.Where(b => b != null)
                .Select(block => block.Value)
                .Select(block =>
                {
                    Debug.Log($"Hydrating {block.Logs.Length} logs to block {block.BlockNumber}");
                    onProgress?.Invoke(new OnProgressOpts
                    {
                        step = SyncStep.Snapshot,
                        percentage = 0,
                        latestBlockNumber = 0,
                        lastBlockNumberProcessed = block.BlockNumber,
                        message = "Hydrating from snapshot"
                    });
                    RxStorageAdapter.ToStorage(onUpdate, ds, block);
                    onProgress?.Invoke(new OnProgressOpts
                    {
                        step = SyncStep.Snapshot,
                        percentage = 100,
                        latestBlockNumber = 0,
                        lastBlockNumberProcessed = block.BlockNumber,
                        message = "Hydrated from snapshot"
                    });
                    return block;
                }).Concat().Replay(1).RefCount();

            BigInteger? endBlock = null;
            BigInteger? startBlock = null;
            
            var startBlockObservable = initialBlockLogs
                .Select(block => BigInteger.Max(block?.BlockNumber ?? 0, initialBlockNumber))
                .Do(b => Debug.Log("Starting sync from block " + b));

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
                .SelectMany(block => Sync.ToStorageAdapterBlock(block.Logs, block.ToBlock, ds).ToObservable()).Merge()
                .Share();

            BigInteger lastBlockNumberProcessed;
            IObservable<StorageAdapterBlock> storedBlockLogs = storedInitialBlockLogs.Concat(blockLogs.Select(block =>
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
                    var percentage = (int)(processedBlocks * 1000 / totalBlocks) / 1000;
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
