using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using static mud.Network.schemas.SchemaTypes;

namespace mud.Network.schemas
{
    public static class StaticFieldUtils
    {
        
        private static string BytesToHex(byte[] value)
        {
            var hexString = value.Aggregate("", (current, b) => current + b.ToString("X2"));
            return $"0x{hexString}";
        }
        
        public static object DecodeStaticField(SchemaType fieldType, byte[] bytes, int bytesOffset)

        {
            var staticLength = Schema.GetStaticByteLength(fieldType);
            byte[] slice = bytes.Skip(bytesOffset).Take(staticLength).ToArray();
            var hex = BytesToHex(slice);
            var numberHex = hex == "0x" ? "0x0" : hex;
            if (numberHex != "0x0" && numberHex.Length < bytes.Length)
            {
                var noZero = numberHex.Substring(2);
                var padded = noZero.PadLeft(32, '0');
                numberHex = "0x" + padded;
            }
            switch (fieldType)
            {
                case SchemaType.BOOL:
                    return Convert.ToUInt64(numberHex, 16) != 0;
                case SchemaType.UINT8:
                case SchemaType.UINT16:
                case SchemaType.UINT24:
                case SchemaType.UINT32:
                case SchemaType.UINT40:
                case SchemaType.UINT48:
                    return Convert.ToUInt64(numberHex, 16);
                case SchemaType.UINT56:
                case SchemaType.UINT64:
                case SchemaType.UINT72:
                case SchemaType.UINT80:
                case SchemaType.UINT88:
                case SchemaType.UINT96:
                case SchemaType.UINT104:
                case SchemaType.UINT112:
                case SchemaType.UINT120:
                case SchemaType.UINT128:
                case SchemaType.UINT136:
                case SchemaType.UINT144:
                case SchemaType.UINT152:
                case SchemaType.UINT160:
                case SchemaType.UINT168:
                case SchemaType.UINT176:
                case SchemaType.UINT184:
                case SchemaType.UINT192:
                case SchemaType.UINT200:
                case SchemaType.UINT208:
                case SchemaType.UINT216:
                case SchemaType.UINT224:
                case SchemaType.UINT232:
                case SchemaType.UINT240:
                case SchemaType.UINT248:
                case SchemaType.UINT256:
                    return BigInteger.Parse(numberHex.Replace("0x",""), NumberStyles.HexNumber);
                case SchemaType.INT8:
                case SchemaType.INT16:
                case SchemaType.INT24:
                case SchemaType.INT32:
                case SchemaType.INT40:
                case SchemaType.INT48:
                {
                    long max = (long)(1UL << (staticLength * 8));
                    long num = (long)Convert.ToUInt64(numberHex, 16);
                    return num < max / 2 ? num : num - max;
                }
                case SchemaType.INT56:
                case SchemaType.INT64:
                case SchemaType.INT72:
                case SchemaType.INT80:
                case SchemaType.INT88:
                case SchemaType.INT96:
                case SchemaType.INT104:
                case SchemaType.INT112:
                case SchemaType.INT120:
                case SchemaType.INT128:
                case SchemaType.INT136:
                case SchemaType.INT144:
                case SchemaType.INT152:
                case SchemaType.INT160:
                case SchemaType.INT168:
                case SchemaType.INT176:
                case SchemaType.INT184:
                case SchemaType.INT192:
                case SchemaType.INT200:
                case SchemaType.INT208:
                case SchemaType.INT216:
                case SchemaType.INT224:
                case SchemaType.INT232:
                case SchemaType.INT240:
                case SchemaType.INT248:
                case SchemaType.INT256:
                {
                    BigInteger max = BigInteger.Pow(2, staticLength * 8);
                    BigInteger num = BigInteger.Parse(numberHex, NumberStyles.HexNumber);
                    return num < max / 2 ? num : num - max;
                }
                case SchemaType.BYTES1:
                case SchemaType.BYTES2:
                case SchemaType.BYTES3:
                case SchemaType.BYTES4:
                case SchemaType.BYTES5:
                case SchemaType.BYTES6:
                case SchemaType.BYTES7:
                case SchemaType.BYTES8:
                case SchemaType.BYTES9:
                case SchemaType.BYTES10:
                case SchemaType.BYTES11:
                case SchemaType.BYTES12:
                case SchemaType.BYTES13:
                case SchemaType.BYTES14:
                case SchemaType.BYTES15:
                case SchemaType.BYTES16:
                case SchemaType.BYTES17:
                case SchemaType.BYTES18:
                case SchemaType.BYTES19:
                case SchemaType.BYTES20:
                case SchemaType.BYTES21:
                case SchemaType.BYTES22:
                case SchemaType.BYTES23:
                case SchemaType.BYTES24:
                case SchemaType.BYTES25:
                case SchemaType.BYTES26:
                case SchemaType.BYTES27:
                case SchemaType.BYTES28:
                case SchemaType.BYTES29:
                case SchemaType.BYTES30:
                case SchemaType.BYTES31:
                case SchemaType.BYTES32:
                case SchemaType.ADDRESS:
                    return Pad(hex, PadDirection.Right, staticLength);
                default:
                    return UnsupportedStaticField(fieldType);
            }
        }

        private enum PadDirection
        {
            Left,
            Right
        }

        private static string Pad(string input, PadDirection direction, int size)
        {
            int paddingSize = size * 2 - input.Length;
            if (paddingSize <= 0) return input;

            string padding = new string('0', paddingSize);
            return direction == PadDirection.Left ? padding + input : input + padding;
        }

        public static T UnsupportedStaticField<T>(T fieldType) where T : Enum
        {
            string fieldName = Enum.GetName(typeof(T), fieldType) ?? fieldType.ToString();
            throw new InvalidOperationException($"Unsupported static field type: {fieldName}");
        }
    }
}
