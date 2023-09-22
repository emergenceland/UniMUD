using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using mud.Network;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionTypes;
using Nethereum.Unity.Rpc;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using UnityEngine;

namespace v2
{
    public class CreateContract
    {
        private Account _signer;
        private string _rpcUrl;
        private int _chainId;
        private HexBigInteger _currentNonce = new(0);
        private string _contractAddress;
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
                Debug.Log(JsonConvert.SerializeObject(latestBlock));

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
                

                Debug.Log("Gas limit: " + gasLimit.Value);

                functionMessage.TransactionType = TransactionType.EIP1559.AsByte();
                functionMessage.MaxPriorityFeePerGas = new HexBigInteger(GasConfig.MaxPriorityFeePerGas);
                functionMessage.MaxFeePerGas = new HexBigInteger(GasConfig.MaxFeePerGas);
                functionMessage.Gas = gasLimit;
                functionMessage.FromAddress = _signer.Address;
                functionMessage.Nonce = _currentNonce;
                
                Debug.Log($"executing transaction with nonce {_currentNonce}");
                Debug.Log("TxInput: " + JsonConvert.SerializeObject(functionMessage));
                
                var txRequest = new TransactionSignedUnityRequest(_rpcUrl, _signer.PrivateKey, _chainId);
                await txRequest.SignAndSendTransaction(functionMessage, _contractAddress).ToUniTask();
                var txHash = txRequest.Result;
                Debug.Log("TxHash: " + txHash);
                if (txHash == null)
                {
                    return false;
                }
                
                var pollingService = new TransactionReceiptPollingRequest(_rpcUrl);
                await pollingService.PollForReceipt(txHash, 2).ToUniTask();
                var transferReceipt = pollingService.Result;
                Debug.Log("Tx Receipt: " + JsonConvert.SerializeObject(transferReceipt));

                _currentNonce = new HexBigInteger(BigInteger.Add(BigInteger.One, _currentNonce.Value));

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        private async UniTask<(BigInteger, BigInteger)> UpdateFeePerGas(int multiplier)
        {
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