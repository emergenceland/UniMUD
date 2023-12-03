using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using static mud.Common;
using static mud.SchemaAbiTypes;

namespace mud
{
    public partial class ProtocolParser 
    {
        public static object DecodeDynamicField(SchemaType fieldType, string data)
        {
            if (fieldType == SchemaType.BYTES)
            {
                return data;
            }

            if (fieldType == SchemaType.STRING)
            {
                return HexToUTF8(data);
            }

            if (data.Length > 3 && data.Length % 2 != 0)
            {
                throw InvalidHexLengthError(data);
            }

            var dataSize = (data.Length - 2) / 2;

            switch (fieldType)
            {
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
                {
                    var staticAbiType = fieldType.ToString().Replace("_ARRAY", "");

                    if (Enum.TryParse(staticAbiType, out SchemaType abiType))
                    {
                        var defaultArrayType = DynamicAbiTypeToDefaultValue[fieldType].GetType();
                        var defaultValueType = StaticAbiTypeToDefaultValue[abiType].GetType();
                        // UnityEngine.Debug.Log($"ARRAY: {defaultArrayType} VALUE: {defaultValueType}");
                        var itemByteLength = GetStaticByteLength[abiType];
                        if (dataSize % itemByteLength != 0) throw InvalidHexLengthForArrayFieldError(abiType, data);
                                                             
                        return Enumerable.Range(0, dataSize / itemByteLength)
                            .Select(i =>
                            {
                                var itemData = SliceHex(data, i * itemByteLength, (i + 1) * itemByteLength);
                                return DecodeStaticField(abiType, itemData);
                            }).ToArray();
                    }

               
                    return UnsupportedDynamicField(fieldType);
                }
                default:
                    return UnsupportedDynamicField(fieldType);
            }
        } 
    }
}
