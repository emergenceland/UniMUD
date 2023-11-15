using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using UniRx;
using UnityEngine;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace mud
{
    public class NetworkManager : MonoBehaviour
    {
        public static string LocalKey {get{return Instance.key;}}
        public static string LocalAddress {get{return Instance.address;}}
        public static string WorldAddress {get{return Instance._worldAddress;}}
        public static NetworkManager Instance { get; private set; }
        public static CreateContract World { get {return Instance.world;}}
        public static RxDatastore Datastore { get {return Instance.ds;}}
        public static NetworkData Network { get {return Instance.activeNetwork;}}
        public static Account Account {get{return Instance.account;}}
        public event Action<NetworkManager> OnNetworkInitialized = delegate { };
        public static Action OnInitialized;
        public static bool Initialized;

        [Header("Settings")] 
        [SerializeField] bool autoConnect = true;
        [Tooltip("Generate new wallet every time instead of loading from PlayerPrefs")]
        [SerializeField] bool burnerWallet = true;
        [SerializeField] bool uniqueWallets = false;        
        [SerializeField] bool verbose = false;        

        private NetworkData activeNetwork;

        [Header("Network")] 
        public NetworkTypes.NetworkType networkType;
        public HybridWebSocket.WebSocketState wsState;
        public NetworkData local;
        public NetworkData testnet;
        public NetworkData mainnet;


        [Header("Debug Account")] 
        [SerializeField] string address;
        [SerializeField] string key;
        public Account account;
        public string pk;
        private int _chainId;
        private string _rpcUrl;
        private string _wsRpcUrl;

        [Header("Debug World")] 
        [SerializeField] string _worldAddress;
        private int worldBlockNumber = -1;
        private int startingBlockNumber = -1;
        private BlockStream bs;
        public RxDatastore ds;
        public StoreSync sync;
        public CreateContract world;
        private readonly CompositeDisposable _disposables = new();
        public static bool Verbose {get{return Instance.verbose;}}

        WorldSelector worldSelector;
        private bool _networkReady = false;

        protected void Awake() {
            if (Instance != null) { Debug.LogError("Already have a NetworkManager instance"); return;}
            Instance = this;
        }

        private async void Start() {

            if(autoConnect) {
                await CreateNetwork();
            }
        }

        public async UniTask CreateNetwork() {

            LoadNetwork();

            await LoadWorldContract();
            await LoadOrMakeAccount();
            await Connect();
        }
        
        public void LoadNetwork() {LoadNetwork(networkType);}
        public void LoadNetwork(NetworkTypes.NetworkType newNetwork) {

            //load our network from enum
            networkType = newNetwork;
            activeNetwork = networkType switch
            {
                NetworkTypes.NetworkType.Local => local,
                NetworkTypes.NetworkType.Testnet => testnet,
                NetworkTypes.NetworkType.Mainnet => mainnet,
                _ => activeNetwork
            };

            SetNetwork(activeNetwork);
        }
        public void SetNetwork(NetworkData newData) {

            if (newData == null) {
                Debug.LogError($"Null network", this);
                return;
            }

            activeNetwork = newData;

            _rpcUrl = activeNetwork.jsonRpcUrl;
            _wsRpcUrl = activeNetwork.wsRpcUrl;
            _chainId = activeNetwork.chainId;
        }

        public async UniTask LoadWorldContract() {
            //load either directly from worlds.json or NetworkData
            if(string.IsNullOrEmpty(activeNetwork.contractAddress)) {
                
                worldSelector = GetComponent<WorldSelector>();
                if(worldSelector == null) {worldSelector = gameObject.AddComponent<WorldSelector>();}
                await worldSelector.LoadWorldFile();

                _worldAddress = worldSelector.GetWorldContract(activeNetwork);
            } else {
                _worldAddress = activeNetwork.contractAddress;
            }
        }

        public async UniTask LoadOrMakeAccount() {

            Account newAccount = null;

            if (!string.IsNullOrWhiteSpace(pk)) {
                newAccount = CreateAccount(pk);
                Debug.Log("Loaded account from pk: " + newAccount.Address);
            } else {
                string loadedOrCreatedKey = uniqueWallets ? Common.GeneratePrivateKey() : Common.GetBurnerPrivateKey();
                newAccount = CreateAccount(loadedOrCreatedKey);
                Debug.Log("Burner wallet created/loaded: " + newAccount.Address);
            }

            await SetAccount(newAccount);
        }

        public static Account CreateAccount(string newPrivateKey) { return Common.CreateAndSaveAccount(newPrivateKey, Instance._chainId);}

        public static async UniTask SetAccount(Account newAccount) { await Instance.InternalSetAccount(newAccount);}
        public async UniTask InternalSetAccount(Account newAccount) {
            
            account = newAccount;
            address = newAccount.Address;
            key = AccountKey(address);

            world = new CreateContract();
            await world.CreateTxExecutor(account, _worldAddress, _rpcUrl, _chainId);

            //drip
            await Drip();

            Debug.Log("Account Setup: " + newAccount.Address);
        }

        public static string AccountKey(string a) { return "0x" + a.Replace("0x", "").PadLeft(64, '0').ToUpper();}

        public async UniTask Drip() {

            if(string.IsNullOrEmpty(activeNetwork.faucetUrl)) {
                return;
            }
            
            Debug.Log($"[Dev Faucet]: Player address -> {address}");
            var balance = await GetBalance(address);
            Debug.Log($"Player balance -> {balance} ETH");
            if (balance < (decimal)0.1)
            {
                Debug.Log("Balance is low, requesting funds from faucet...");
                try {
                    await Common.GetRequestAsync($"{activeNetwork.faucetUrl}?address={address}");
                } catch (Exception exception) {
                    Debug.LogError(exception);
                }   
                
                // Expected output: ReferenceError: nonExistentFunction is not defined
                // (Note: the exact output may be browser-dependent)
            }
        }
    

        public async UniTask Connect() {

            if(Account == null) {Debug.LogError("No account", this); return;}
            if(bs != null) {Debug.LogError("Network Manager: Already connected", this); return;}

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
            
            if (worldBlockNumber < 0) {worldBlockNumber = worldSelector.GetBlockNumber(activeNetwork);}
            if (startingBlockNumber < 0) {await GetStartingBlockNumber().ToUniTask();}
            
            Debug.Log($"Starting sync from {worldBlockNumber}...{startingBlockNumber}");

            sync = new StoreSync().AddTo(_disposables);
            var updateStream =
                sync.StartSync(ds, bs.subject, _worldAddress, _rpcUrl, worldBlockNumber, startingBlockNumber,
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
                    });

            updateStream.Subscribe(onNext: _ => { }, onError: Debug.LogError)
                .AddTo(_disposables);
        }
        
        private IEnumerator GetStartingBlockNumber() {
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

        void Update() {
            wsState = bs?.WS?.GetState() ?? HybridWebSocket.WebSocketState.Closed;
        }

        private void OnDestroy()
        {
            Initialized = false;
            _disposables?.Dispose();
        }
    }
}
