using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;
using Types = mud.Network.Types;

namespace v2
{
    public partial class Sync
    {
        private readonly ReplaySubject<Types.NetworkTableUpdate> _outputStream = new();
        public IObservable<Types.NetworkTableUpdate> OutputStream => _outputStream;
        private readonly CompositeDisposable _disposables = new();

        public async UniTask StartSync(string storeContractAddress, string account, string rpcUrl,
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
                Debug.Log("Latest block number: " + blockNum);
                endBlock = blockNum;
            }).Share();
            
            BigInteger startBlock = initialBlockNumber;


            var blockLogs = latestBlockNumber.Select(_ => (Start: startBlock, End: endBlock))
                .SelectMany(blockRange =>
                {
                    if (blockRange.Start > blockRange.End) return Observable.Empty<Types.NetworkTableUpdate>();

                    return Common.AsyncEnumerableToObservable(FetchLogs(storeContractAddress, account, rpcUrl,
                            blockRange.Start, blockRange.End))
                        .Do(_ => startBlock = blockRange.End + 1);
                });

            var blockLogsSubscription = ObservableExtensions.Subscribe(blockLogs, update =>
            {
                Debug.Log(JsonConvert.SerializeObject(update));
            });
        }
    }
}
