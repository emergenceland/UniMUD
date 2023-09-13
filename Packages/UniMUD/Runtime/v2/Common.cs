using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Unity.Rpc;
using UniRx;

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

        public static BigInteger ConvertFromHexUnsigned(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue)) return BigInteger.Zero;

            if (hexValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexValue = hexValue[2..];
            }

            BigInteger bigIntValue;
            if (hexValue.Length % 2 == 1)
            {
                // If the length is odd, add a leading zero to ensure it's treated as positive.
                hexValue = "0" + hexValue;
            }

            // Now ensure the highest bit isn't set, to prevent the number from being treated as negative.
            if (int.Parse(hexValue[0].ToString(), System.Globalization.NumberStyles.HexNumber) >= 8)
            {
                // If the highest bit is set, prepend "00" to ensure the resulting value is treated as positive.
                hexValue = "00" + hexValue;
            }

            bigIntValue = BigInteger.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
            return bigIntValue;
        }

        public static IObservable<T> AsyncEnumerableToObservable<T>(IAsyncEnumerable<T> source)
        {
            return Observable.Create<T>(observer =>
            {
                var _ = IterateAsync(observer, source);
                return Disposable.Empty;
            });
        }

        private static async UniTask IterateAsync<T>(IObserver<T> observer, IAsyncEnumerable<T> source)
        {
            try
            {
                await foreach (var item in source)
                {
                    observer.OnNext(item);
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }
        
    }
}
