using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace mud
{
    public static partial class Common
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

        public static string BytesToHex(byte[] value, int? size = null)
        {
            int targetLength = size.HasValue ? size.Value * 2 : 0;
            return "0x" + value.Aggregate("", (current, b) => current + b.ToString("X2"))
                .PadRight(targetLength, '0');
        }

        public static string ReadHex(string data, int start, int? end = null)
        {
            data = data.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? data.Substring(2) : data;

            if (start * 2 >= data.Length) return data;

            int? endIndex = end * 2;
            string result = data.Substring(start * 2,
                endIndex.HasValue ? endIndex.Value - start * 2 : data.Length - start * 2);

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
                result[i] = byte.Parse(hexString.Substring(i * 2, 2), NumberStyles.HexNumber);
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

        public static string SpliceHex(string data, int start, int deleteCount = 0, string newData = "0x")
        {
            return ConcatHex(new[]
            {
                ReadHex(data, 0, start),
                newData,
                ReadHex(data, start + deleteCount)
            });
        }

        public static int Size(string value)
        {
            if (value.StartsWith("0x"))
            {
                return (int)Math.Ceiling(((double)value.Length - 2) / 2);
            }

            return value.Length;
        }

        public static string StringToHex(string value, int size)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return BytesToHex(valueBytes, size);
        }
    }
}
