using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using static mud.Common;
using static mud.SchemaAbiTypes;

namespace mud
{
    public partial class ProtocolParser
    {
        public static Dictionary<string, object> DecodeValue(Dictionary<string, SchemaAbiTypes.SchemaType> valueSchema, string data)
        {
            var staticFields = valueSchema.Values.Where(value => StaticAbiTypes.Contains(value))
                .ToList();
            var dynamicFields = valueSchema.Values.Where(value => DynamicAbiTypes.Contains(value))
                .ToList();

            var values = new List<object>();

            var bytesOffset = 0;
            staticFields.ForEach(fieldType =>
            {
                var fieldByteLength = GetStaticByteLength[fieldType];
                var value = DecodeStaticField(fieldType,
                    ReadHex(data, bytesOffset, bytesOffset + fieldByteLength));
                bytesOffset += fieldByteLength;
                values.Add(value);
            });

            var schemaStaticDataLength =
                staticFields.Aggregate(0,
                    (length, fieldType) => length + GetStaticByteLength[fieldType]);
            var actualStaticDataLength = bytesOffset;
            if (actualStaticDataLength != schemaStaticDataLength)
            {
                Debug.LogWarning(
                    $"Static data length mismatch. Expected {schemaStaticDataLength}, got {actualStaticDataLength}");
            }

            if (dynamicFields.Count > 0)
            {
                var dataLayout = HexToPackedCounter(ReadHex(data, bytesOffset, bytesOffset + 32));
                bytesOffset += 32;

                for (var i = 0; i < dynamicFields.Count; i++)
                {
                    var dataLength = dataLayout.FieldByteLengths[i];
                    var fieldType = dynamicFields[i];
                    if (dataLength > 0)
                    {
                        var value = DecodeDynamicField(fieldType,
                            ReadHex(data, bytesOffset, bytesOffset + (int)dataLength));
                        bytesOffset += (int)dataLength;
                        values.Add(value);
                    }
                    else
                    {
                        values.Add(DynamicAbiTypeToDefaultValue[fieldType]);
                    }
                }

                var actualDynamicDataLength = bytesOffset - 32 - actualStaticDataLength;
                if ((BigInteger)actualDynamicDataLength != dataLayout.TotalByteLength)
                {
                    Debug.LogWarning(
                        "Decoded dynamic data length does not match data layout's expected data length. Data may get corrupted. Did the data layout change?");
                }
            }

            var result = new Dictionary<string, object>();
            int j = 0;
            foreach (var key in valueSchema)
            {
                result.Add(key.Key, values[j]);
                j++;
            }

            return result;
        }
    }
}
