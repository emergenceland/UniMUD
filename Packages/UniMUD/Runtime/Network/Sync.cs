using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using mud.Client;
using mud.Network.IStore;
using Nethereum.JsonRpc.WebSocketStreamingClient;

namespace mud.Network
{
    public class SyncWorker : IDisposable
    {
        private readonly ReplaySubject<Types.NetworkTableUpdate> _outputStream = new();
        public IObservable<Types.NetworkTableUpdate> OutputStream => _outputStream;
        private readonly CompositeDisposable _disposables = new();
        
        public async Task StartSync(IStoreService store, StreamingWebSocketClient client, int initialBlockNumber, int streamStartBlockNumber)
        {
            var initialLiveEvents = new List<Types.NetworkTableUpdate>();
            var (latestEvents, disposables) = await Sync.SubscribeToStoreEvents(store, client);
            
            var latestEventSub = latestEvents.Subscribe((e) => { initialLiveEvents.Add(e); });

            var gapStateEvents =
                await Sync.FetchStoreEventsInRange(store, initialBlockNumber, streamStartBlockNumber);
            
            var merged = new List<Types.NetworkTableUpdate>();
            merged.AddRange(initialLiveEvents);
            merged.AddRange(gapStateEvents);
            
            var filteredMerged = merged.Where(e => e is { Ephemeral: null or false });

            latestEventSub.Dispose();

            foreach (var update in filteredMerged)
            {
                _outputStream.OnNext(update);
            }
            
            // TODO: persist to disk?

            var liveEvents = latestEvents.Subscribe((e) =>
            {
                _outputStream.OnNext(e);
            });
            _disposables.Add(liveEvents);
        }

        public void Dispose()
        {
            _outputStream?.Dispose();
            _disposables?.Dispose();
        }
    }
}
