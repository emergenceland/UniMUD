using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nethereum.Util;
using UnityEngine;
using static mud.Network.schemas.SchemaTypes;
using static mud.Network.schemas.Common;


namespace mud.Network.schemas
{
    public static class DataDecoder
    {
        public static Dictionary<int, object> DecodeData(TableSchema schema, string hexData)
        {
            var data = new Dictionary<int, object>();
            var bytes = HexStringToByteArray(hexData);
            
            var bytesOffset = 0;
            for (var i = 0; i < schema.StaticFields.Count; i++)
            {
                var fieldType = schema.StaticFields[i];
                var staticByteLength = Schema.GetStaticByteLength(fieldType);
                var decoded = StaticFieldUtils.DecodeStaticField(fieldType, bytes, bytesOffset);
                data.Add(i, decoded);
                bytesOffset += staticByteLength;
            }

            var actualStaticDataLength = bytesOffset;
            if (actualStaticDataLength != schema.StaticDataLength)
            {
                throw new Exception(
                    $"Static data length mismatch. Expected {schema.StaticDataLength}, got {actualStaticDataLength}");
            }

            if (schema.DynamicFields.Count > 0)
            {
                byte[] dynamicDataLayout = new byte[32];
                Array.Copy(bytes, schema.StaticDataLength, dynamicDataLayout, 0, 32);
                bytesOffset += 32;
                var dynamicDataLength = BinaryPrimitives.ReadUInt32BigEndian(dynamicDataLayout.AsSpan());

                for (var i = 0; i < schema.DynamicFields.Count; i++)
                {
                    var fieldType = schema.DynamicFields[i];
                    var dataLength = BinaryPrimitives.ReadUInt16BigEndian(dynamicDataLayout.AsSpan(4 + i * 2));
                    var value = DecodeDynamicField(fieldType, bytes.Slice(bytesOffset, bytesOffset + dataLength));
                    bytesOffset += dataLength;
                    data[schema.StaticFields.Count + i] = value;
                }

                var actualDynamicDataLength = bytesOffset - 32 - schema.StaticDataLength;
                if (actualDynamicDataLength != dynamicDataLength)
                {
                    Debug.LogWarning(
                        "Decoded dynamic data length does not match data layout's expected data length. Data may get corrupted. Did the data layout change?");
                }
            }

            return data;
        }

        public static Dictionary<int, object> DecodeField(TableSchema schema, int schemaIndex, string hexData)
        {
            var data = new Dictionary<int, object>();
            var bytes = HexStringToByteArray(hexData);

            for (var i = 0; i < schema.StaticFields.Count; i++)
            {
                var fieldType = schema.StaticFields[i];
                if (i == schemaIndex)
                {
                    data[schemaIndex] = StaticFieldUtils.DecodeStaticField(fieldType, bytes, 0);
                }
            }

            if (schema.DynamicFields.Count > 0)
            {
                for (var i = 0; i < schema.DynamicFields.Count; i++)
                {
                    var fieldType = schema.DynamicFields[i];
                    var index = schema.StaticFields.Count * i;
                    if (index == schemaIndex)
                    {
                        data[schemaIndex] = DecodeDynamicField(fieldType, bytes);
                    }
                }
            }

            return data;
        }

        public static object DecodeDynamicField(SchemaTypes.SchemaType fieldType, byte[] bytes)
        {
            if (fieldType == SchemaTypes.SchemaType.BYTES)
            {
                return ByteArrayToHexString(bytes);
            }

            if (fieldType == SchemaTypes.SchemaType.STRING)
            {
                return Encoding.UTF8.GetString(bytes);
            }

            var staticTypeExists = SchemaTypeArrayToElement.TryGetValue(fieldType, out var staticType);
            if (!staticTypeExists) return UnsupportedDynamicField(fieldType);
            var fieldLength = Schema.GetStaticByteLength(staticType);
            var arrayLength = bytes.Length / fieldLength;
            return Enumerable.Range(0, arrayLength)
                .Select(i => StaticFieldUtils.DecodeStaticField(staticType, bytes, i * fieldLength)).ToArray();
        }

        public static T UnsupportedDynamicField<T>(T fieldType) where T : Enum
        {
            throw new InvalidOperationException("Unsupported dynamic field type");
        }
    }
}
