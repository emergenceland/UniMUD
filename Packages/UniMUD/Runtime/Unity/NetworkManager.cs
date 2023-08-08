using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Faucet;
using mud.Client;
using mud.Client.MudDefinitions;
using mud.Network;
using mud.Network.IStore;
using mud.Network.schemas;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.Blocks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Network;
using Newtonsoft.Json;
using NLog;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;


namespace mud.Unity
{
    public enum NetworkType {Local,Testnet,Mainnet}
    public class LocalDeploy
    {
        public string worldAddress;
        public int blockNumber;
    }

    public record NetworkConfig(string JsonRpcUrl, string WsRpcUrl, string Pk, int ChainId, string WorldAddress,
        bool DisableCache)
    {
        public string JsonRpcUrl { get; set; } = JsonRpcUrl;
        public string WsRpcUrl { get; set; } = WsRpcUrl;
        public string Pk { get; set; } = Pk;
        public int ChainId { get; set; } = ChainId;
        public string WorldAddress { get; set; } = WorldAddress;
        public bool DisableCache { get; set; } = DisableCache;
    }

    public class NetworkManager : MonoBehaviour
    {
        public static Action OnInitialized;
        public static string LocalAddress {get{return Instance.addressKey;}}
        public static string LocalAddressNotKey {get{return Instance.address;}}
        public static string WorldAddress {get{return Instance.worldAddress;}}
        public static NetworkData Network {get{return Instance.activeNetwork;}}
        public NetworkType networkType;
        public NetworkData local;
        public NetworkData testnet;
        public NetworkData mainnet;


        public string worldAddress;
        public string faucetUrl;

        [Header("Dev settings")] 
        public string pk;

        [Tooltip("Generate new wallet every time instead of loading from PlayerPrefs")]
        public bool uniqueWallets = true;
        public bool disableCache = true;

        [Header("Debug")]
        public string address;
        public string addressKey;

        public readonly TxExecutor worldSend = new();
        public Datastore ds;
        private SyncWorker _syncWorker;
        private Web3 _provider;
        public Account account;


        private StreamingWebSocketClient _client;

        private readonly CompositeDisposable _disposables = new();
        public event Action<NetworkManager> OnNetworkInitialized = delegate { };
        public static NetworkManager Instance { get; private set; }
        public static bool NetworkInitialized {get{return m_NetworkInitialized;}}
        private static bool m_NetworkInitialized = false;
        private CancellationTokenSource _cts;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private NetworkData activeNetwork;

        protected async void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Already have a NetworkManager instance");
                return;
            }

            Instance = this;

            if(networkType == NetworkType.Local) {
                activeNetwork = local;
            } else if(networkType == NetworkType.Testnet) {
                activeNetwork = testnet;
            } else if(networkType == NetworkType.Mainnet) {
                activeNetwork = mainnet;
            }

            faucetUrl = activeNetwork.faucetUrl;

            if(activeNetwork == null) {
                Debug.LogError("No network data", this);
            }

            var config = new NetworkConfig(activeNetwork.jsonRpcUrl, activeNetwork.wsRpcUrl, activeNetwork.pk, activeNetwork.chainId, activeNetwork.contractAddress, activeNetwork.disableCache);
            var logUnity = new UnityLogger { Layout = "${message} ${exception:format=tostring}" };
            LogManager.Setup().SetupExtensions(s => s.RegisterTarget<UnityLogger>("UnityLogger"))
                .LoadConfiguration(builder => builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteTo(logUnity));

            await SetupNetwork(config);
        }

        private async Task SetupNetwork(NetworkConfig config)
        {

            var (jsonRpcUrl, wsRpcUrl, pk, chainId, contractAddress, disableCache) = config;

            if (!string.IsNullOrWhiteSpace(pk))
            {
                account = new Account(pk, chainId);
            }
            else
            {
                var savedBurnerWallet = PlayerPrefs.GetString("burnerWallet");
                if (!string.IsNullOrWhiteSpace(savedBurnerWallet) && !uniqueWallets)
                {
                    account = new Account(savedBurnerWallet, chainId);
                    Debug.Log("Loaded burner wallet: " + account.Address);
                }
                else
                {
                    var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                    var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
                    account = new Account(privateKey, chainId);
                    Debug.Log("New burner wallet created: " + account.Address);
                    // TODO: Insecure.
                    // We can use Nethereum's KeyStoreScryptService
                    // However, this requires user to set a password
                    if (!uniqueWallets)
                    {
                        PlayerPrefs.SetString("burnerWallet", privateKey);
                        PlayerPrefs.Save();
                    }
                }

                address = account.Address;
                addressKey = "0x" + address.Replace("0x", "").PadLeft(64, '0').ToLower();
            }


            var providerConfig = new ProviderConfig
            {
                JsonRpcUrl = jsonRpcUrl,
                WsRpcUrl = wsRpcUrl
            };

            _cts = new CancellationTokenSource();

            var (prov, wsClient) = await Providers.CreateReconnectingProviders(account, providerConfig, _cts.Token);
            _provider = prov;
            _client = wsClient;

            if (!string.IsNullOrWhiteSpace(faucetUrl))
            {
                Debug.Log("[Dev Faucet]: Player address -> " + address);
                var faucet = new FaucetClient(faucetUrl);
                _disposables.Add(faucet);
                var balance = await _provider.Eth.GetBalance.SendRequestAsync(address);
                Debug.Log(JsonConvert.SerializeObject(balance));
                Debug.Log("Current balance: " + balance.Value);
                var lowBalance = balance.Value <= BigInteger.Parse("1000000000000000000");
                if (lowBalance)
                {
                    Debug.Log("[Dev Faucet]: Balance is low, dripping funds to player");
                    var d = await faucet.Drip(address);
                    Debug.Log(JsonConvert.SerializeObject(d));
                    var newBalance = await _provider.Eth.GetBalance.SendRequestAsync(address);
                    Debug.Log(JsonConvert.SerializeObject(newBalance));
                    Debug.Log("[Dev Faucet]: New balance: " + newBalance.Value);
                }
            }


            var startingBlockNumber = -1;


            if (string.IsNullOrEmpty(contractAddress))
            {
                var jsonFile = Resources.Load<TextAsset>("latest");

                if (jsonFile == null)
                {
                    throw new ArgumentException("Contract address must be provided.");
                }

                var data = JsonUtility.FromJson<LocalDeploy>(jsonFile.text);
                contractAddress = data.worldAddress;
                startingBlockNumber = data.blockNumber;
                worldAddress = contractAddress;
                Debug.Log("Loaded local deploy: " + contractAddress + " at block " + startingBlockNumber);
            }

            if (startingBlockNumber < 0)
            {
                startingBlockNumber = (int)await GetCurrentBlockNumberAsync(_provider);
            }

            Debug.Log("Starting sync from block " + startingBlockNumber + "...");

            var storeContract = new IStoreService(_provider, contractAddress);

            var jsonStore = new JsonDataStorage(Application.persistentDataPath + contractAddress + ".json");
            // ds = new Datastore(jsonStore);
            ds = new Datastore(); // TODO: add persistence

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IMudTable).IsAssignableFrom(p) && p.IsClass);
            foreach (var t in types)
            {

                //ignore exact IMudTable class
                if(t == typeof(IMudTable)) {
                    continue;
                }

                Debug.Log($"Registering table: {t.Name}");
                if (t.GetField("ID").GetValue(null) is not TableId tableId) return;
                ds.RegisterTable(tableId.ToString(), tableId.name);
            }

            WorldDefinitions.DefineDataStoreConfig(ds);
            StoreDefinitions.DefineDataStoreConfig(ds);

            ds.RegisterTable(new TableId("mudstore", "schema").ToString(), "Schema");

            // TODO
            // if (!disableCache)
            // {
            //     ds.LoadCache(); // TODO: this does not update the components
            //     startingBlockNumber = ds.GetCachedBlockNumber() ?? startingBlockNumber;
            // }

            await worldSend.CreateTxExecutor(account, _provider, contractAddress);

            _syncWorker = new SyncWorker();
            var currentBlockNumber = (int)await GetCurrentBlockNumberAsync(_provider);
            await _syncWorker.StartSync(storeContract, _client, startingBlockNumber, currentBlockNumber);

            UniRx.ObservableExtensions
                .Subscribe(_syncWorker.OutputStream, (update) => { NetworkUpdates.ApplyNetworkUpdates(update, ds); })
                .AddTo(_disposables);

            m_NetworkInitialized = true;

            OnNetworkInitialized(this);
            OnInitialized?.Invoke();
        }

        private static async Task<BigInteger> GetCurrentBlockNumberAsync(Web3 provider)
        {
            EthBlockNumber ethBlockNumber = new EthBlockNumber(provider.Client);
            var blockNumber = await ethBlockNumber.SendRequestAsync();
            return blockNumber.Value;
        }

        private void OnDestroy()
        {
            _syncWorker?.Dispose();
            _client?.Dispose();
            _disposables?.Dispose();
            _cts.Cancel();
            LogManager.Shutdown();
        }
    }
}
