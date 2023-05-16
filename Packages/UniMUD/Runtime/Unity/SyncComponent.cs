using System;
using mud.Client;
using UniRx;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

namespace mud.Unity
{
    public abstract class SyncComponent : MonoBehaviour, ISyncComponent
    {
        [NonSerialized] public NetworkManager networkManager;
        private readonly CompositeDisposable _disposables = new();
        [NonSerialized] public bool isLocalPlayer;
        // [NonSerialized] public string key;

        private void Start()
        {
            if (NetworkManager.Instance.isNetworkInitialized)
            {
                Initialize(NetworkManager.Instance);
            }
            else
            {
                NetworkManager.Instance.OnNetworkInitialized += Initialize;
            }
        }


        protected virtual void Initialize(NetworkManager network)
        {
            networkManager = network;
            SubscribeToDataStore();
        }

        private void SubscribeToDataStore()
        {
            ObservableExtensions
                .Subscribe(NetworkManager.Instance.ds.OnDataStoreUpdate.ObserveOnMainThread(), OnDataStoreUpdate)
                .AddTo(_disposables);
        }

        public abstract void OnDataStoreUpdate(RecordUpdate update);


        private void OnDestroy()
        {
            _disposables.Dispose();
            NetworkManager.Instance.OnNetworkInitialized -= Initialize;
        }
    }
}
