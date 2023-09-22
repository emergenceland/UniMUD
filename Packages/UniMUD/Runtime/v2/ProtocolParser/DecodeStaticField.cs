using System;
using System.Numerics;
using UnityEngine;
using static v2.Common;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class ProtocolParser
    {
        public static object DecodeStaticField(SchemaType fieldType, string data)
        {
            if (data.Length > 3 && data.Length % 2 != 0) throw InvalidHexLengthError(data);

            var dataSize = data.StartsWith("0x") ? (data.Length - 2) / 2 : data.Length / 2;
            var staticLength = GetStaticByteLength[fieldType];
            if (dataSize != staticLength) throw InvalidHexLengthForStaticFieldError(fieldType, data);

            switch (fieldType)
            {
                case SchemaType.UINT8:
                case SchemaType.UINT16:
                case SchemaType.UINT24:
                case SchemaType.UINT32:
                case SchemaType.UINT40:
                case SchemaType.UINT48:
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
                case SchemaType.INT8:
                case SchemaType.INT16:
                case SchemaType.INT24:
                case SchemaType.INT32:
                case SchemaType.INT40:
                case SchemaType.INT48:
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
                    var value = HexToBigInt(data, fieldType.ToString().StartsWith("INT"));
                    var defaultValueType = StaticAbiTypeToDefaultValue[fieldType].GetType();
                    if (defaultValueType == typeof(BigInteger))
                    {
                        return value;
                    }

                    if (defaultValueType == typeof(UInt64))
                    {
                        return (UInt64)value;
                    }

                    if (defaultValueType == typeof(Int64))
                    {
                        return (Int64)value;
                    }

                    if (defaultValueType == typeof(UInt32))
                    {
                        return (UInt32)value;
                    }

                    if (defaultValueType == typeof(Int32))
                    {
                        return (Int32)value;
                    }
                    throw new InvalidOperationException("Unsupported type encountered. " + data);
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
                    return data;
                case SchemaType.BOOL:
                    var hex = data.StartsWith("0x") ? data[2..] : data;
                    if (hex == "0x00") return false;
                    if (hex == "0x01") return true;
                    return UnsupportedStaticField(fieldType);
                case SchemaType.ADDRESS:
                    return Pad(data, PadDirection.Right, staticLength);
                case SchemaType.UINT8_ARRAY:
                case SchemaType.UINT16_ARRAY:
                case SchemaType.UINT24_ARRAY:
                case SchemaType.UINT32_ARRAY:
                case SchemaType.UINT40_ARRAY:
                case SchemaType.UINT48_ARRAY:
                case SchemaType.UINT56_ARRAY:
                case SchemaType.UINT64_ARRAY:
                case SchemaType.UINT72_ARRAY:
                case SchemaType.UINT80_ARRAY:
                case SchemaType.UINT88_ARRAY:
                case SchemaType.UINT96_ARRAY:
                case SchemaType.UINT104_ARRAY:
                case SchemaType.UINT112_ARRAY:
                case SchemaType.UINT120_ARRAY:
                case SchemaType.UINT128_ARRAY:
                case SchemaType.UINT136_ARRAY:
                case SchemaType.UINT144_ARRAY:
                case SchemaType.UINT152_ARRAY:
                case SchemaType.UINT160_ARRAY:
                case SchemaType.UINT168_ARRAY:
                case SchemaType.UINT176_ARRAY:
                case SchemaType.UINT184_ARRAY:
                case SchemaType.UINT192_ARRAY:
                case SchemaType.UINT200_ARRAY:
                case SchemaType.UINT208_ARRAY:
                case SchemaType.UINT216_ARRAY:
                case SchemaType.UINT224_ARRAY:
                case SchemaType.UINT232_ARRAY:
                case SchemaType.UINT240_ARRAY:
                case SchemaType.UINT248_ARRAY:
                case SchemaType.UINT256_ARRAY:
                case SchemaType.INT8_ARRAY:
                case SchemaType.INT16_ARRAY:
                case SchemaType.INT24_ARRAY:
                case SchemaType.INT32_ARRAY:
                case SchemaType.INT40_ARRAY:
                case SchemaType.INT48_ARRAY:
                case SchemaType.INT56_ARRAY:
                case SchemaType.INT64_ARRAY:
                case SchemaType.INT72_ARRAY:
                case SchemaType.INT80_ARRAY:
                case SchemaType.INT88_ARRAY:
                case SchemaType.INT96_ARRAY:
                case SchemaType.INT104_ARRAY:
                case SchemaType.INT112_ARRAY:
                case SchemaType.INT120_ARRAY:
                case SchemaType.INT128_ARRAY:
                case SchemaType.INT136_ARRAY:
                case SchemaType.INT144_ARRAY:
                case SchemaType.INT152_ARRAY:
                case SchemaType.INT160_ARRAY:
                case SchemaType.INT168_ARRAY:
                case SchemaType.INT176_ARRAY:
                case SchemaType.INT184_ARRAY:
                case SchemaType.INT192_ARRAY:
                case SchemaType.INT200_ARRAY:
                case SchemaType.INT208_ARRAY:
                case SchemaType.INT216_ARRAY:
                case SchemaType.INT224_ARRAY:
                case SchemaType.INT232_ARRAY:
                case SchemaType.INT240_ARRAY:
                case SchemaType.INT248_ARRAY:
                case SchemaType.INT256_ARRAY:
                case SchemaType.BYTES1_ARRAY:
                case SchemaType.BYTES2_ARRAY:
                case SchemaType.BYTES3_ARRAY:
                case SchemaType.BYTES4_ARRAY:
                case SchemaType.BYTES5_ARRAY:
                case SchemaType.BYTES6_ARRAY:
                case SchemaType.BYTES7_ARRAY:
                case SchemaType.BYTES8_ARRAY:
                case SchemaType.BYTES9_ARRAY:
                case SchemaType.BYTES10_ARRAY:
                case SchemaType.BYTES11_ARRAY:
                case SchemaType.BYTES12_ARRAY:
                case SchemaType.BYTES13_ARRAY:
                case SchemaType.BYTES14_ARRAY:
                case SchemaType.BYTES15_ARRAY:
                case SchemaType.BYTES16_ARRAY:
                case SchemaType.BYTES17_ARRAY:
                case SchemaType.BYTES18_ARRAY:
                case SchemaType.BYTES19_ARRAY:
                case SchemaType.BYTES20_ARRAY:
                case SchemaType.BYTES21_ARRAY:
                case SchemaType.BYTES22_ARRAY:
                case SchemaType.BYTES23_ARRAY:
                case SchemaType.BYTES24_ARRAY:
                case SchemaType.BYTES25_ARRAY:
                case SchemaType.BYTES26_ARRAY:
                case SchemaType.BYTES27_ARRAY:
                case SchemaType.BYTES28_ARRAY:
                case SchemaType.BYTES29_ARRAY:
                case SchemaType.BYTES30_ARRAY:
                case SchemaType.BYTES31_ARRAY:
                case SchemaType.BYTES32_ARRAY:
                case SchemaType.BOOL_ARRAY:
                case SchemaType.ADDRESS_ARRAY:
                case SchemaType.BYTES:
                case SchemaType.STRING:
                default:
                    return UnsupportedStaticField(fieldType);
            }
        }
    }
}
