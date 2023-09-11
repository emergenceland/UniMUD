using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using mud.Client;
using mud.Client.MudDefinitions;
using mud.Network;
using mud.Network.schemas;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using UniRx;
using UnityEngine;
using HybridWebSocket;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using Newtonsoft.Json;


namespace v2
{
    public class NetworkManager : MonoBehaviour
    {
        public string pk;
        public Account account;
        public int chainId;
        public string rpcUrl;
        public bool uniqueWallets;
        public string address;
        public string addressKey;
        public string storeContract;
        private int startingBlockNumber = -1;
        public Datastore ds;
        private readonly CompositeDisposable _disposables = new();
        
        // initialization events
        private static bool m_NetworkInitialized = false;
        public event Action<NetworkManager> OnNetworkInitialized = delegate { };
        public static Action OnInitialized;
        
        private async void Start()
        {
            /*
             * ==== ACCOUNT CREATION ====
             */
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

            /*
             * ==== FAUCET ====
             */

            /*
             * ==== TX EXECUTOR ====
             */

            var world = new CreateContract();
            await world.CreateTxExecutor(account, storeContract, rpcUrl, chainId);

            /*
             * ==== CLIENT CACHE ====
             */
            ds = new Datastore(); // TODO: add persistence

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IMudTable).IsAssignableFrom(p) && p.IsClass);
            foreach (var t in types)
            {
                //ignore exact IMudTable class
                if (t == typeof(IMudTable))
                {
                    continue;
                }

                Debug.Log($"Registering table: {t.Name}");
                if (t.GetField("ID").GetValue(null) is not TableId tableId) return;
                ds.RegisterTable(tableId);
            }

            WorldDefinitions.DefineDataStoreConfig(ds);
            StoreDefinitions.DefineDataStoreConfig(ds);
            ds.RegisterTable(new TableId("mudstore", "schema"));

            /*
             * ==== SYNC ====
             */

            if (startingBlockNumber < 0)
            {
                Debug.Log("Getting current block number...");
                await GetBlockNumber().ToUniTask();
            }
            
            Debug.Log("Starting sync from block " + startingBlockNumber + "...");

            // var blockStream = new BlockStream().AddTo(_disposables);
            // UniRx.ObservableExtensions.Subscribe<Block>(blockStream.WatchBlocks(), (block) => 
            //     {
            //         Debug.Log($"Received block with number: {block.@params.result.number}");
            //     },
            //     error => 
            //     {
            //         Debug.LogError($"Error: {error.Message}");
            //     });
            //
            var syncWorker = new Sync();
            await syncWorker.StartSync(storeContract, account.Address, rpcUrl, startingBlockNumber);

            // await Sync.FetchLogs(storeContract, account.Address, rpcUrl, 0, 123);
            // await syncWorker.StartSync(storeContract, account.Address, rpcUrl,  startingBlockNumber);
            //
            // UniRx.ObservableExtensions
            // .Subscribe(syncWorker.OutputStream, (update) => { NetworkUpdates.ApplyNetworkUpdates(update, ds); })
            // .AddTo(_disposables);
            
            Debug.Log("Sending test tx...");
            await world.Write<IncrementFunction>();
            
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
