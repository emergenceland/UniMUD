using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.RPC.TransactionTypes;
using Nethereum.Web3;
using Newtonsoft.Json;
using UnityEngine;
using Account = Nethereum.Web3.Accounts.Account;

namespace mud.Network
{
    public class GasConfig
    {
        public BigInteger MaxPriorityFeePerGas { get; set; } = 0;
        public BigInteger MaxFeePerGas { get; set; } = 0;
    }

    public class TxExecutor
    {
        private Web3 _provider;
        private Account _signer;
        private HexBigInteger _currentNonce = new(0);
        private string _contractAddress;
        private GasConfig GasConfig { get; set; }
        private ContractHandler ContractHandler { get; set; }

        public async Task CreateTxExecutor(Account signer, Web3 provider, string contractAddress)
        {
            _provider = provider;
            _signer = signer;
            var contractHandler = provider.Eth.GetContractHandler(contractAddress);
            _contractAddress = contractAddress;
            ContractHandler = contractHandler;
            GasConfig = new GasConfig();
            var (maxPriorityFee, maxFee) = await UpdateFeePerGas(1);
            GasConfig.MaxPriorityFeePerGas = maxPriorityFee;
            GasConfig.MaxFeePerGas = maxFee;
        }

        public async Task TxExecute<TFunction>(params object[] functionParameters)
            where TFunction : FunctionMessage, new()
        {
            if (_currentNonce == new HexBigInteger(0))
            {
                _currentNonce = await _provider.Eth.Transactions.GetTransactionCount.SendRequestAsync(_signer.Address);
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

            var gasLimit = await _provider.Eth.GetContractTransactionHandler<TFunction>()
                .EstimateGasAsync(_contractAddress, functionMessage);

            functionMessage.TransactionType = TransactionType.EIP1559.AsByte();
            functionMessage.MaxPriorityFeePerGas = new HexBigInteger(GasConfig.MaxPriorityFeePerGas);
            functionMessage.MaxFeePerGas = new HexBigInteger(GasConfig.MaxFeePerGas);
            functionMessage.Gas = gasLimit;
            functionMessage.FromAddress = _signer.Address;
            functionMessage.Nonce = _currentNonce;

            Debug.Log($"executing transaction with nonce {_currentNonce}");

            // try
            // {
            var txHash = await _provider.Eth.GetContractTransactionHandler<TFunction>()
                .SendRequestAsync(_contractAddress, functionMessage);

            Debug.Log("TxRequest: " + txHash);
            var pollingService = new TransactionReceiptPollingService(_provider.TransactionManager);
            var transactionReceipt = await pollingService.PollForReceiptAsync(txHash);
            Debug.Log("Tx receipt: " + JsonConvert.SerializeObject(transactionReceipt));

            _currentNonce = new HexBigInteger(BigInteger.Add(BigInteger.One, _currentNonce.Value));
            // }
            // catch (Exception error)
            // {
            //     if (error.Message.Contains("transaction already imported"))
            //     {
            //         // if (options.retryCount == 0) TODO
            //         {
            //             // UpdateFeePerGas(globalOptions.priorityFeeMultiplier * 1.1);
            //             // return fastTxExecute(contract, func, args,  {
            //             //     retryCount:
            //             //     options.retryCount++
            //             // });
            //         }
            //     }
            // }
        }

        private async Task<(BigInteger, BigInteger)> UpdateFeePerGas(int multiplier)
        {
            var latestBlock =
                await _provider.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(
                    BlockParameter.CreateLatest());
            var baseFeePerGas = latestBlock.BaseFeePerGas.Value;
            var maxPriorityFeePerGas =
                baseFeePerGas == 0 ? 0 : (BigInteger)Math.Floor((double)(1_500_000_000 * multiplier));
            var maxFeePerGas = baseFeePerGas * 2 + GasConfig.MaxPriorityFeePerGas;
            return (maxPriorityFeePerGas, maxFeePerGas);
        }
    }
}
