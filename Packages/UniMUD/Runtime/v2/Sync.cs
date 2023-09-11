using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UniRx.Operators;
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
            var blockStream = new BlockStream().AddTo(_disposables).WatchBlocks();
            var latestBlockNumber = blockStream.Select(block =>
            {
                if (block.@params?.result.number == null) return 0;
                return Common.ConvertFromHexUnsigned(block.@params.result.number);
            }).Do(blockNum => Debug.Log("Latest block number: " + blockNum)).Share();
            ObservableExtensions.Subscribe(latestBlockNumber, integer => { }, Debug.LogError);
        }

    }
}
