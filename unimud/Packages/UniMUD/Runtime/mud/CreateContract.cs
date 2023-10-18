using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionTypes;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace mud
{
    
    public class GasConfig
    {
        public BigInteger MaxPriorityFeePerGas { get; set; } = 0;
        public BigInteger MaxFeePerGas { get; set; } = 0;
    }
    
    [System.Serializable]
    public class CreateContract
    {
        private Account _signer;
        [SerializeField] string _rpcUrl;
        [SerializeField] int _chainId;
        [SerializeField] string _contractAddress;
        private HexBigInteger _currentNonce = new(0);
        private GasConfig GasConfig { get; set; }
        private int PriorityFeeMultiplier { get; set; }


        public async UniTask CreateTxExecutor(Account signer, string contractAddress, string rpcUrl, int chainId,
            int? priorityFeeMultiplier = null)
        {
            _signer = signer;
            _rpcUrl = rpcUrl;
            _chainId = chainId;
            _contractAddress = contractAddress;
            GasConfig = new GasConfig();
            PriorityFeeMultiplier = priorityFeeMultiplier ?? 1;
            var (maxPriorityFee, maxFee) = await UpdateFeePerGas(PriorityFeeMultiplier);
            GasConfig.MaxPriorityFeePerGas = maxPriorityFee;
            GasConfig.MaxFeePerGas = maxFee;
        }

        public async UniTask<bool> Write<TFunction>(params object[] functionParameters)
            where TFunction : FunctionMessage, new()
        {
            try
            {
                var blockNumberRequest =
                    new EthGetBlockWithTransactionsHashesByNumberUnityRequest(_rpcUrl);
                await blockNumberRequest.SendRequest(BlockParameter.CreateLatest()).ToUniTask();
                var latestBlock = blockNumberRequest.Result;
                if(NetworkManager.Verbose) Debug.Log(JsonConvert.SerializeObject(latestBlock));

                if (_currentNonce == new HexBigInteger(0))
                {
                    var nonceRequest = new EthGetTransactionCountUnityRequest(_rpcUrl);
                    await nonceRequest.SendRequest(_signer.Address, BlockParameter.CreateLatest()).ToUniTask();
                    _currentNonce = nonceRequest.Result;
                }

                var functionMessage = new TFunction();
                if (functionParameters.Length > 0)
                {
                    var properties = typeof(TFunction).GetProperties();
                    for (var i = 0; i < properties.Length && i < functionParameters.Length; i++)
                    {
                        properties[i].SetValue(functionMessage, functionParameters[i]);
                    }
                }

                var gasLimit = latestBlock.GasLimit;

                if (gasLimit == null)
                {
                    return false;
                }
                

                functionMessage.TransactionType = TransactionType.EIP1559.AsByte();
                functionMessage.MaxPriorityFeePerGas = new HexBigInteger(GasConfig.MaxPriorityFeePerGas);
                functionMessage.MaxFeePerGas = new HexBigInteger(GasConfig.MaxFeePerGas);
                functionMessage.Gas = gasLimit;
                functionMessage.FromAddress = _signer.Address;
                functionMessage.Nonce = _currentNonce;
                
                if(NetworkManager.Verbose) Debug.Log($"Tx Executing transaction with nonce {_currentNonce}");
                if(NetworkManager.Verbose) Debug.Log("TxInput: " + JsonConvert.SerializeObject(functionMessage));
                
                var txRequest = new TransactionSignedUnityRequest(_rpcUrl, _signer.PrivateKey, _chainId);
                await txRequest.SignAndSendTransaction(functionMessage, _contractAddress).ToUniTask();
                var txHash = txRequest.Result;
                if(NetworkManager.Verbose) Debug.Log("TxHash: " + txHash);
                if (txHash == null)
                {
                    Debug.LogError("Tx Failed");
                    return false;
                }
                
                var pollingService = new TransactionReceiptPollingRequest(_rpcUrl);
                await pollingService.PollForReceipt(txHash, 0.25f).ToUniTask();
                var transferReceipt = pollingService.Result;
                
                JObject j = JObject.Parse(JsonConvert.SerializeObject(transferReceipt));
                if(NetworkManager.Verbose) {Debug.Log($"Tx Receipt: {j}");}

                //tx was not successful
                if(transferReceipt == null || (string)j["status"] == "0x0") {return false;}

                _currentNonce = new HexBigInteger(BigInteger.Add(BigInteger.One, _currentNonce.Value));

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Tx Receipt: " + ex);
                return false;
            }
        }

        private async UniTask<(BigInteger, BigInteger)> UpdateFeePerGas(int multiplier)
        {
            await UniTask.SwitchToMainThread();
            var blockNumberRequest = new EthGetBlockWithTransactionsHashesByNumberUnityRequest(_rpcUrl);
            await blockNumberRequest.SendRequest(BlockParameter.CreateLatest()).ToUniTask();
            var latestBlock = blockNumberRequest.Result;
            var baseFeePerGas = latestBlock.BaseFeePerGas.Value;
            var maxPriorityFeePerGas =
                baseFeePerGas == 0 ? 0 : (BigInteger)Math.Floor((double)(1_500_000_000 * multiplier));
            var maxFeePerGas = baseFeePerGas * 2 + maxPriorityFeePerGas;
            return (maxPriorityFeePerGas, maxFeePerGas);
        }
    }
}
