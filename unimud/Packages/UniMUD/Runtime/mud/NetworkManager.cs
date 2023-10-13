using System;
using System.Collections;
using System.Numerics;
using Cysharp.Threading.Tasks;
using mud;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using UniRx;
using UnityEngine;


namespace mud
{
    public class NetworkManager : MonoBehaviour
    {
        [Header("Dev settings")] 
        public string pk;
        [Tooltip("Generate new wallet every time instead of loading from PlayerPrefs")]
        public bool uniqueWallets;

        private NetworkData activeNetwork;
        [Header("Network")] public NetworkTypes.NetworkType networkType;
        public NetworkData local;
        public NetworkData testnet;
        public NetworkData mainnet;
        private int _chainId;
        private string _rpcUrl;
        private string _wsRpcUrl;
        private string _worldAddress;

        [Header("Debug")] public string address;
        public string addressKey;

        public Account account;
        private int startingBlockNumber = -1;
        private int streamStartBlockNumber = 0; // TODO: get from indexer
        private BlockStream bs;
        public RxDatastore ds;
        public StoreSync sync;
        public CreateContract world;
        public static NetworkManager Instance { get; private set; }
        private readonly CompositeDisposable _disposables = new();
        
        public event Action<NetworkManager> OnNetworkInitialized = delegate { };
        private bool _networkReady = false;

        protected void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Already have a NetworkManager instance");
                return;
            }

            Instance = this;
            activeNetwork = networkType switch
            {
                NetworkTypes.NetworkType.Local => local,
                NetworkTypes.NetworkType.Testnet => testnet,
                NetworkTypes.NetworkType.Mainnet => mainnet,
                _ => activeNetwork
            };

            if (activeNetwork == null)
            {
                Debug.LogError("No network data", this);
            }

            if (activeNetwork != null)
            {
                _rpcUrl = activeNetwork.jsonRpcUrl;
                _wsRpcUrl = activeNetwork.wsRpcUrl;
                _chainId = activeNetwork.chainId;
                _worldAddress = activeNetwork.contractAddress;
            }
        }

        private async void Start()
        {
            /*
             * ==== ACCOUNT CREATION ====
             */
            if (!string.IsNullOrWhiteSpace(pk))
            {
                account = new Account(pk, _chainId);
                Debug.Log("Loaded account from pk: " + account.Address);
            }
            else
            {
                account = uniqueWallets
                    ? new Account(Common.GeneratePrivateKey(), _chainId)
                    : new Account(Common.GetBurnerPrivateKey(), _chainId);
                address = account.Address;
                Debug.Log("Burner wallet created/loaded: " + address);
                addressKey = "0x" + address.Replace("0x", "").PadLeft(64, '0').ToLower();
            }

            /*
             * ==== PROVIDER ====
             */

            Debug.Log("Connecting to websocket...");
            bs = new BlockStream().AddTo(_disposables);
            await bs.WatchBlocks(_wsRpcUrl);

            /*
             * ==== TX EXECUTOR ====
             */

            world = new CreateContract();
            await world.CreateTxExecutor(account, _worldAddress, _rpcUrl, _chainId);

            /*
             * ==== CLIENT CACHE ====
             */
            ds = new RxDatastore(); // TODO: add persistence

            var worldConfig = MudDefinitions.DefineWorldConfig(_worldAddress);
            var storeConfig = MudDefinitions.DefineStoreConfig(_worldAddress);
            Common.LoadConfig(worldConfig, ds);
            Common.LoadConfig(storeConfig, ds);

            /*
             * ==== SYNC ====
             */

            if (startingBlockNumber < 0) await GetStartingBlockNumber().ToUniTask();
            Debug.Log($"Starting sync from 0...{startingBlockNumber}");

            sync = new StoreSync().AddTo(_disposables);
            var updateStream =
                sync.StartSync(ds, bs.subject, _worldAddress, _rpcUrl, BigInteger.Zero, startingBlockNumber,
                    opts =>
                    {
                        if (opts.step == SyncStep.Live && !_networkReady)
                        {
                            _networkReady = true;
                            Debug.Log(opts.message);
                            OnNetworkInitialized(this);
                        }
                    });

            updateStream.Subscribe(onNext: _ => { }, onError: Debug.LogError)
                .AddTo(_disposables);

            /*
             * ==== FAUCET ====
             */
            // TODO
        }

        private IEnumerator GetStartingBlockNumber()
        {
            var blockNumberRequest = new EthBlockNumberUnityRequest(_rpcUrl);
            yield return blockNumberRequest.SendRequest();
            startingBlockNumber = (int)blockNumberRequest.Result.Value;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
