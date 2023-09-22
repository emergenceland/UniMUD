using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using mud.Client;
// using mud.Client.MudDefinitions;
using mud.Network.schemas;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using UniRx;
using UnityEngine;

namespace v2
{
    public class NetworkManager : MonoBehaviour
    {
        public string pk;
        public Account account;
        public int chainId;
        public string rpcUrl;
        public string wsRpcUrl;
        public bool uniqueWallets;
        public string address;
        public string addressKey;
        public string storeContract;
        private int startingBlockNumber = -1;
        private int streamStartBlockNumber = 0;
        public RxDatastore ds;
        private readonly CompositeDisposable _disposables = new();
        private BlockStream _wsClient;
        public CreateContract world;
        public static NetworkManager Instance { get; private set; }

        // initialization events
        private static bool m_NetworkInitialized = false;
        public event Action<NetworkManager> OnNetworkInitialized = delegate { };
        public static Action OnInitialized;

        protected async void Awake()
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
            /*
             * ==== ACCOUNT CREATION ====
             */
            if (!string.IsNullOrWhiteSpace(pk))
            {
                account = new Account(pk, chainId);
                Debug.Log("Loaded account from pk: " + account.Address);
            }
            else
            {
                // TODO: Unique wallets
                account = new Account(Common.GetBurnerPrivateKey(chainId), chainId);
                address = account.Address;
                Debug.Log("Burner wallet created/loaded: " + address);
                addressKey = "0x" + address.Replace("0x", "").PadLeft(64, '0').ToLower();
            }

            /*
             * ==== PROVIDER ====
             */

            Debug.Log("Creating websocket client...");
            _wsClient = new BlockStream().AddTo(_disposables);

    

            /*
             * ==== CLIENT CACHE ====
             */
            ds = new RxDatastore(); // TODO: add persistence

            // TODO: do this.
            // var types = AppDomain.CurrentDomain.GetAssemblies()
            //     .SelectMany(s => s.GetTypes())
            //     .Where(p => typeof(IMudTable).IsAssignableFrom(p) && p.IsClass);
            // foreach (var t in types)
            // {
            //     //ignore exact IMudTable class
            //     if (t == typeof(IMudTable))
            //     {
            //         continue;
            //     }
            //
            //     Debug.Log($"Registering table: {t.Name}");
            //     if (t.GetField("ID").GetValue(null) is not TableId tableId) return;
            //     ds.RegisterTable(tableId);
            // }
            //
            // WorldDefinitions.DefineDataStoreConfig(ds);
            // StoreDefinitions.DefineDataStoreConfig(ds);
            // ds.RegisterTable(new TableId("mudstore", "schema"));

            /*
             * ==== SYNC ====
             */

            // if (startingBlockNumber < 0)
            // {
            //     Debug.Log("Getting current block number...");
            //     await GetBlockNumber().ToUniTask();
            // }
            startingBlockNumber = 0;

            Debug.Log("Starting sync from block " + startingBlockNumber + "...");

            var storeSync = new StoreSync().AddTo(_disposables);
            var updateStream =
                storeSync.StartSync(_wsClient, storeContract, account.Address, rpcUrl, startingBlockNumber, wsRpcUrl);

            UniRx.ObservableExtensions.Subscribe(updateStream, b => StorageAdapter.ToStorage(ds, b))
                .AddTo(_disposables);
            
            /*
             * ==== TX EXECUTOR ====
             */

            // delay for 3 seconds
            await UniTask.Delay(3000);
            world = new CreateContract();
            await world.CreateTxExecutor(account, storeContract, rpcUrl, chainId);

            // Debug.Log("Sending test tx...");
            // await world.Write<IncrementFunction>();

            /*
             * ==== FAUCET ====
             */

            m_NetworkInitialized = true;

            OnNetworkInitialized(this);
            OnInitialized?.Invoke();
        }

        private IEnumerator GetBlockNumber()
        {
            var blockNumberRequest = new EthBlockNumberUnityRequest(rpcUrl);
            yield return blockNumberRequest.SendRequest();
            startingBlockNumber = (int)blockNumberRequest.Result.Value;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
