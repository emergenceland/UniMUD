#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

namespace v2
{
    public enum ResourceType
    {
        Table,
        OffchainTable,
        Namespace,
        Module,
        System
    }

    public struct ResourceID
    {
        public string Namespace;
        public string Name;
        public ResourceType Type;
    }

    public class Common
    {
        public static BigInteger HexToBigInt(string hexValue, bool signed = false)
        {
            if (string.IsNullOrEmpty(hexValue)) return BigInteger.Zero;

            if (hexValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexValue = hexValue[2..];
            }

            BigInteger bigIntValue;

            if (!signed)
            {
                if (hexValue.Length % 2 == 1)
                {
                    // If the length is odd, add a leading zero to ensure it's treated as positive
                    hexValue = "0" + hexValue;
                }

                // Ensure the highest bit isn't set to prevent the number from being treated as negative
                if (int.Parse(hexValue[0].ToString(), NumberStyles.HexNumber) >= 8)
                {
                    // If the highest bit is set, prepend "00" to ensure the value is treated as positive.
                    hexValue = "00" + hexValue;
                }

                bigIntValue = BigInteger.Parse(hexValue, NumberStyles.HexNumber);
            }
            else
            {
                // BigInteger.Parse naturally assumes a signed value
                bigIntValue = BigInteger.Parse(hexValue, NumberStyles.HexNumber);
            }

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

        public class JsonEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? x, object? y)
            {
                return JsonConvert.SerializeObject(x) == JsonConvert.SerializeObject(y);
            }

            public int GetHashCode(object obj)
            {
                return JsonConvert.SerializeObject(obj).GetHashCode();
            }
        }

        public static string GetBurnerPrivateKey(int chainId)
        {
            {
                var savedBurnerWallet = PlayerPrefs.GetString("burnerWallet");
                if (!string.IsNullOrWhiteSpace(savedBurnerWallet))
                {
                    return savedBurnerWallet;
                }

                var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                var newPrivateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
                // TODO: Insecure.
                // We can use Nethereum's KeyStoreScryptService
                // However, this requires user to set a password
                PlayerPrefs.SetString("burnerWallet", newPrivateKey);
                PlayerPrefs.Save();
                return newPrivateKey;
            }
        }

        public enum PadDirection
        {
            Left,
            Right
        }

        public static string BytesToHex(byte[] value, int? size = null)
        {
            int targetLength = size.HasValue ? size.Value * 2 : 0;
            return "0x" + value.Aggregate("", (current, b) => current + b.ToString("X2"))
                .PadRight(targetLength, '0');
        }

        public static string Pad(string input, PadDirection direction, int size)
        {
            int paddingSize = size * 2 - input.Length;
            if (paddingSize <= 0) return input;

            string padding = new string('0', paddingSize);
            return direction == PadDirection.Left ? padding + input : input + padding;
        }

        public static string ReadHex(string data, int start, int? end = null)
        {
            data = data.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? data.Substring(2) : data;

            int? endIndex = end.HasValue ? end.Value * 2 : (int?)null;
            string result = data.Substring(start * 2,
                endIndex.HasValue ? endIndex.Value - (start * 2) : data.Length - (start * 2));

            return $"0x{result.PadRight(((end ?? start) - start) * 2, '0')}";
        }

        public static string SliceHex(string value, int? start = null, int? end = null)
        {
            if (!value.StartsWith("0x"))
                value = "0x" + value;

            int startIdx = (start ?? 0) * 2;
            int length;

            if (end.HasValue)
                length = (end.Value * 2) - startIdx;
            else
                length = value.Length - startIdx;

            string result = value.Substring(startIdx + 2, length);
            return "0x" + result;
        }


        public static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString.StartsWith("0x"))
            {
                hexString = hexString.Substring(2);
            }

            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid hex string");
            }

            byte[] result = new byte[hexString.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = byte.Parse(hexString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return result;
        }

        public static string HexToUTF8(string input)
        {
            var hexString = input.StartsWith("0x") ? input.Substring(2) : input;

            int numberOfChars = hexString.Length / 2;
            byte[] bytes = new byte[numberOfChars];

            for (int i = 0; i < numberOfChars; i++)
            {
                string hexPair = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(hexPair, 16);
            }

            string utf8String = Encoding.UTF8.GetString(bytes);
            return utf8String;
        }

        public static string ConcatHex(string[] values)
        {
            return $"0x{values.Aggregate("", (acc, x) => acc + x.Replace("0x", ""))}";
        }

        private static readonly Dictionary<ResourceType, string> ResourceTypeIds = new()
        {
            // keep these in sync with worldResourceTypes.sol
            { ResourceType.Table, "tb" },
            { ResourceType.OffchainTable, "ot" },
            // keep these in sync with worldResourceTypes.sol
            { ResourceType.Namespace, "ns" },
            { ResourceType.Module, "md" },
            { ResourceType.System, "sy" }
        };

        public static string StringToHex(string value, int size)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return BytesToHex(valueBytes, size);
        }

        public static string ResourceIDToHex(ResourceID resourceId)
        {
            var typeId = ResourceTypeIds[resourceId.Type];
            return ConcatHex(new[]
            {
                StringToHex(typeId, 2),
                StringToHex(resourceId.Namespace.Length >= 14 ? resourceId.Namespace[..14] : resourceId.Namespace, 14),
                StringToHex(resourceId.Name.Length >= 16 ? resourceId.Name[..16] : resourceId.Name, 16),
            });
        }

        public static Dictionary<string, ResourceType> ResourceTypeIdToType = ResourceTypeIds
            .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static ResourceType? GetResourceType(string resourceTypeId)
        {
            var type = ResourceTypeIdToType[resourceTypeId];
            return type;
        }

        public static ResourceID HexToResourceId(string hex)
        {
            var resourceTypeId = Regex.Replace(HexToUTF8(SliceHex(hex, 0, 2)), @"\0 +$", "");
            var type = GetResourceType(resourceTypeId);
            var ns = Regex.Replace(HexToUTF8(SliceHex(hex, 2, 16)), @"\0 +$", "");
            var name = Regex.Replace(HexToUTF8(SliceHex(hex, 16, 32)), @"\0 +$", "");

            if (type == null) throw new InvalidCastException($"Unknown resource type: {resourceTypeId}");

            return new ResourceID
            {
                Type = type.Value,
                Namespace = ns,
                Name = name
            };
        }
    }
}
