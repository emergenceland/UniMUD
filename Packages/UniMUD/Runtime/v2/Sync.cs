using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using Observable = System.Reactive.Linq.Observable;
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
            var initialLiveEvents = new List<Types.NetworkTableUpdate>();
            var latestEvents = await WatchLogs(storeContractAddress, account, rpcUrl, 1000);

            Debug.Log("Yipe");
            var latestEventSub = ObservableExtensions.Subscribe(latestEvents, list =>
            {
                initialLiveEvents.AddRange(list);
                Debug.Log(JsonConvert.SerializeObject(list));
            });
            Debug.Log("Subbed.");
            _disposables.Add(latestEventSub);
        }
    }
}
