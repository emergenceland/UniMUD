using System;
using System.Linq;
using System.Text;

namespace mud.Network.schemas
{
    public static class Common
    {
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

        public static string ByteArrayToHexString(byte[] array)
        {
            var hexBuilder = new StringBuilder("0x");

            foreach (byte x in array)
            {
                hexBuilder.Append(x.ToString("x2"));
            }

            return hexBuilder.ToString();
        }

        public static byte[] StringToBytes16(string str)
        {
            if (str.Length > 16) throw new ArgumentException("String too long");
            var paddedStr = str.PadRight(16, '\0');
            return Enumerable.Range(0, 16).Select(i => (byte)paddedStr[i]).ToArray();
        }

        public static string BytesToString(byte[] bytes)
        {
            var result = Encoding.ASCII.GetString(bytes);
            int nullIndex = result.IndexOf('\0');
            return nullIndex >= 0 ? result.Substring(0, nullIndex) : result;
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
    }
}
