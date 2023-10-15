using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using static mud.Common;
using static mud.SchemaAbiTypes;

namespace mud
{
    public static partial class ProtocolParser
    {
        public static PackedCounter HexToPackedCounter(string data)
        {
            Debug.Log($"DECODING: {data}");
            if (data.Length != 66) throw InvalidHexLengthForPackedCounterError(data);

            // Last 7 bytes (uint56) are used for the total byte length of the dynamic data
            var totalByteLength = (BigInteger)DecodeStaticField(SchemaType.UINT56,
                ReadHex(data, 32 - 7, 32));

            // The next 5 byte (uint40) sections are used for the byte length of each field, indexed from right to left
            var dynamicDataLength = DecodeDynamicField(
                SchemaType.UINT40_ARRAY,
                ReadHex(data, 0, 32 - 7));
            
            UInt64[] reversedFieldByteLengths;
            
            if (dynamicDataLength is IEnumerable<object> objects)
            {
                reversedFieldByteLengths = objects
                    // we cast to Uint64 because uint40 fits in a regular int
                    // don't need a bigint
                    .Select(item => (UInt64)item)
                    .ToArray();
            }
            else
            {
                throw new Exception("Result is not BigInteger[]");
            }

            var fieldByteLengths = new List<UInt64>(reversedFieldByteLengths);
            fieldByteLengths.Reverse();
            var accumulatedLength = fieldByteLengths.Aggregate((UInt64)0, (total, length) => total + length);
            if (accumulatedLength != totalByteLength)
                throw PackedCounterLengthMismatchError(data, totalByteLength,
                    accumulatedLength);
            return new PackedCounter
            {
                TotalByteLength = totalByteLength,
                FieldByteLengths = fieldByteLengths.ToArray()
            };
            
        }
    }
}
