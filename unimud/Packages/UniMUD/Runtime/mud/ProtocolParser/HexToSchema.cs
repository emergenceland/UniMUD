using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static mud.Common;
using static mud.SchemaAbiTypes;

namespace mud
{
    public partial class ProtocolParser
    {
        public struct TableSchema
        {
            public List<SchemaType> StaticFields;
            public List<SchemaType> DynamicFields;
        }

        public static TableSchema HexToSchema(string data)
        {
            if (data.Length != 66) throw InvalidHexLengthForSchemaError(data);

            var staticDataLength = (int)HexToBigInt(SliceHex(data, 0, 2));
            var numStaticFields = (int)HexToBigInt(SliceHex(data, 2, 3));
            var numDynamicFields = (int)HexToBigInt(SliceHex(data, 3, 4));

            var staticFields = new List<SchemaType>();
            var dynamicFields = new List<SchemaType>();

            for (int i = 4; i < 4 + numStaticFields; i++)
            {
                var schemaTypeIndex = (int)HexToBigInt(SliceHex(data, i, i + 1));
                staticFields.Add(SchemaTypesArray[schemaTypeIndex]);
            }

            for (int i = 4 + numStaticFields; i < 4 + numStaticFields + numDynamicFields; i++)
            {
                var schemaTypeIndex = (uint)HexToBigInt(SliceHex(data, i, i + 1));
                dynamicFields.Add(SchemaTypesArray[schemaTypeIndex]);
            }

            var actualStaticDataLength =
                staticFields.Aggregate(0, (acc, fieldType) => acc + GetStaticByteLength[fieldType]);

            if (actualStaticDataLength != staticDataLength)
            {
                Debug.LogWarning(
                    $"Schema {data} static data length ({staticDataLength}) did not match the summed length of all static fields ({actualStaticDataLength}), Is \\`staticAbiTypeToByteLength\\` up to date with Solidity schema types?");
                throw SchemaStaticLengthMismatchError(data, staticDataLength, actualStaticDataLength);
            }

            return new TableSchema
            {
                DynamicFields = dynamicFields,
                StaticFields = staticFields
            };
        }
    }
}
