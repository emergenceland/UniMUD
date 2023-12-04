using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using UniRx;
using UnityEngine;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace mud
{
    public class NetworkManager : MonoBehaviour
    {
        public static string LocalKey
        {
            get { return Instance.key; }
        }

        public static string LocalAddress
        {
            get { return Instance.address; }
        }

        public static string WorldAddress
        {
            get { return Instance._worldAddress; }
        }

        public static NetworkManager Instance { get; private set; }

        public static CreateContract World
        {
            get { return Instance.world; }
        }

        public static RxDatastore Datastore
        {
            get { return Instance.ds; }
        }

        public static NetworkData ActiveNetwork;

        public static Account Account
        {
            get { return Instance.account; }
        }

        public event Action<NetworkManager> OnNetworkInitialized = delegate { };
        public static Action OnInitialized;
        public static bool Initialized;

        [Header("Settings")] [SerializeField] bool autoConnect = true;

        [Tooltip("Generate new wallet every time instead of loading from PlayerPrefs")] [SerializeField]
        bool burnerWallet = true;

        [SerializeField] bool uniqueWallets = false;
        [SerializeField] bool verbose = false;

        [Header("Network")] public NetworkTypes.NetworkType networkType;
        public NetworkData local;
        public NetworkData testnet;
        public NetworkData mainnet;


        [Header("Debug Account")] [SerializeField]
        string address;

        [SerializeField] string key;
        public Account account;
        public string pk;
        private int _chainId;
        private string _rpcUrl;
        private string _wsRpcUrl;
        private string _indexerUrl;

        [Header("Debug World")] public HybridWebSocket.WebSocketState wsState;
        [SerializeField] string _worldAddress;
        private int worldBlockNumber = -1;
        private int startingBlockNumber = -1;
        private BlockStream bs;
        public RxDatastore ds;
        public StoreSync sync;
        public CreateContract world;
        private readonly CompositeDisposable _disposables = new();

        public static bool Verbose
        {
            get { return Instance.verbose; }
        }

        WorldSelector worldSelector;
        private bool _networkReady = false;
        private static bool _firstLoad = true;

        protected void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Already have a NetworkManager instance");
                return;
            }

            Instance = this;
        }

        private async void Start()
        {
            if (autoConnect)
            {
                await CreateNetwork();
            }
        }

        public async UniTask CreateNetwork()
        {
            if (_firstLoad)
            {
                Initialize();
            }
            else
            {
                Reload();
            }

            await LoadWorldContract();
            await LoadOrMakeAccount();
            await Connect();
        }

        public void Initialize()
        {
            _firstLoad = false;
            LoadNetwork(networkType);
        }

        public void Reload()
        {
            SetNetwork(ActiveNetwork);
        }

        public void LoadNetwork(NetworkTypes.NetworkType newNetwork)
        {
            //load our network from enum
            networkType = newNetwork;
            ActiveNetwork = networkType switch
            {
                NetworkTypes.NetworkType.Local => local,
                NetworkTypes.NetworkType.Testnet => testnet,
                NetworkTypes.NetworkType.Mainnet => mainnet,
                _ => ActiveNetwork
            };

            SetNetwork(ActiveNetwork);
        }

        public void SetNetwork(NetworkData newData)
        {
            if (newData == null)
            {
                Debug.LogError($"Null network", this);
                return;
            }

            Debug.Log($"Set network to {newData.name}.");
            ActiveNetwork = newData;

            _rpcUrl = ActiveNetwork.jsonRpcUrl;
            _wsRpcUrl = ActiveNetwork.wsRpcUrl;
            _chainId = ActiveNetwork.chainId;
            _indexerUrl = ActiveNetwork.indexerUrl;
        }

        public void SwitchNetwork(NetworkData newData)
        {
            Debug.Log($"Switching to {newData.name}.");
            SetNetwork(newData);
            SceneManager.LoadScene(0);
        }

        public async UniTask LoadWorldContract()
        {
            //load either directly from worlds.json or NetworkData
            if (string.IsNullOrEmpty(ActiveNetwork.contractAddress))
            {
                worldSelector = GetComponent<WorldSelector>();
                if (worldSelector == null)
                {
                    worldSelector = gameObject.AddComponent<WorldSelector>();
                }

                await worldSelector.LoadWorldFile();

                _worldAddress = worldSelector.GetWorldContract(ActiveNetwork);
            }
            else
            {
                _worldAddress = ActiveNetwork.contractAddress;
            }
        }

        public async UniTask LoadOrMakeAccount()
        {
            Account newAccount = null;

            if (!string.IsNullOrWhiteSpace(pk))
            {
                newAccount = CreateAccount(pk);
                Debug.Log("Loaded account from pk: " + newAccount.Address);
            }
            else
            {
                string loadedOrCreatedKey = uniqueWallets ? Common.GeneratePrivateKey() : Common.GetBurnerPrivateKey();
                newAccount = CreateAccount(loadedOrCreatedKey);
                Debug.Log("Burner wallet created/loaded: " + newAccount.Address);
            }

            await SetAccount(newAccount);
        }

        public static Account CreateAccount(string newPrivateKey)
        {
            return Common.CreateAndSaveAccount(newPrivateKey, Instance._chainId);
        }

        public static async UniTask SetAccount(Account newAccount)
        {
            await Instance.UpdateAccount(newAccount);
        }

        protected async UniTask UpdateAccount(Account newAccount)
        {
            account = newAccount;
            address = newAccount.Address;
            key = AccountKey(address);

            world = new CreateContract();
            await world.CreateTxExecutor(account, _worldAddress, _rpcUrl, _chainId);

            //drip
            await Drip();

            Debug.Log("Account Setup: " + newAccount.Address);
        }

        public static string AccountKey(string a)
        {
            return "0x" + a.Replace("0x", "").PadLeft(64, '0').ToUpper();
        }

        public async UniTask Drip()
        {
            if (string.IsNullOrEmpty(ActiveNetwork.faucetUrl))
            {
                return;
            }

            Debug.Log($"[Dev Faucet]: Player address -> {address}");
            var balance = await GetBalance(address);
            Debug.Log($"Player balance -> {balance} ETH");
            if (balance < (decimal)0.1)
            {
                Debug.Log("Balance is low, requesting funds from faucet...");
                try
                {
                    await Common.GetRequestAsync($"{ActiveNetwork.faucetUrl}?address={address}");
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                }

                // Expected output: ReferenceError: nonExistentFunction is not defined
                // (Note: the exact output may be browser-dependent)
            }
        }


        public async UniTask Connect()
        {
            if (Account == null)
            {
                Debug.LogError("No account", this);
                return;
            }

            if (bs != null)
            {
                Debug.LogError("Network Manager: Already connected", this);
                return;
            }

            /*
             * ==== PROVIDER ====
             */

            Debug.Log($"Connecting to websocket... [{_wsRpcUrl}]");
            bs = new BlockStream().AddTo(_disposables);
            await bs.WatchBlocks(_wsRpcUrl);

            /*
             * ==== CLIENT CACHE ====
             */
            Debug.Log($"Creating datastore...");
            ds = new RxDatastore(); // TODO: add persistence

            var worldConfig = MudDefinitions.DefineWorldConfig(_worldAddress);
            var storeConfig = MudDefinitions.DefineStoreConfig(_worldAddress);
            var moduleConfig = MudDefinitions.DefineModuleConfig(_worldAddress);

            Common.LoadConfig(worldConfig, ds);
            Common.LoadConfig(storeConfig, ds);
            Common.LoadConfig(moduleConfig, ds);
            /*
             * ==== SYNC ====
             */

            await UniTask.SwitchToMainThread();

            if (worldBlockNumber < 0)
            {
                worldBlockNumber = worldSelector.GetBlockNumber(ActiveNetwork);
            }

            sync = new StoreSync().AddTo(_disposables);
            var updateStream =
                sync.StartSync(ds, bs.subject, _worldAddress, _rpcUrl, _chainId, worldBlockNumber,
                    opts =>
                    {
                        if (opts.step == SyncStep.Live && !_networkReady)
                        {
                            _networkReady = true;
                            Debug.Log(opts.message);

                            Initialized = true;
                            OnInitialized?.Invoke();
                            OnNetworkInitialized(this);
                        }
                    }, _indexerUrl);

            updateStream.Subscribe(onNext: _ => { }, onError: Debug.LogError)
                .AddTo(_disposables);
        }

        private IEnumerator GetStartingBlockNumber()
        {
            var blockNumberRequest = new EthBlockNumberUnityRequest(_rpcUrl);
            yield return blockNumberRequest.SendRequest();
            startingBlockNumber = (int)blockNumberRequest.Result.Value;
        }

        private async UniTask<decimal> GetBalance(string address)
        {
            var balanceRequest = new EthGetBalanceUnityRequest(_rpcUrl);
            await balanceRequest.SendRequest(address, BlockParameter.CreateLatest());
            var balance = balanceRequest.Result.Value;
            var ethBalance = Web3.Convert.FromWei(balance);
            return ethBalance;
        }

        void Update()
        {
            wsState = bs?.WS?.GetState() ?? HybridWebSocket.WebSocketState.Closed;
        }

        private void OnDestroy()
        {
            Instance = null;
            Initialized = false;
            _disposables?.Dispose();
        }
    }
}
