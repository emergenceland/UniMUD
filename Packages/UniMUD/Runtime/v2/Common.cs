using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;

namespace v2
{
    public class Common
    {
        public static async UniTask<BigInteger> GetLatestBlockNumber(string rpcUrl)
        {
            var blockNumberRequest = new EthGetBlockWithTransactionsHashesByNumberUnityRequest(rpcUrl);
            await blockNumberRequest.SendRequest(BlockParameter.CreateLatest()).ToUniTask();
            var latestBlock = blockNumberRequest.Result;
            return latestBlock.Number;
        } 
    }
}
