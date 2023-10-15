using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static mud.Common;

namespace mud
{
    public partial class ProtocolParser
    {
        public static int StaticDataLength(IEnumerable<SchemaAbiTypes.SchemaType> staticFields)
        {
            return staticFields.Aggregate(0,
                (length, fieldType) => length + SchemaAbiTypes.GetStaticByteLength[fieldType]);
        }

        public static Dictionary<string, object> DecodeValueArgs(
            Dictionary<string, SchemaAbiTypes.SchemaType> valueSchema,
            string staticData,
            string encodedLengths,
            string dynamicData
        )
        {
            Debug.Log($"ValueSchema: {JsonConvert.SerializeObject(valueSchema)}");
            return DecodeValue(
                valueSchema,
                ConcatHex(new[]
                {
                    ReadHex(staticData, 0,
                        StaticDataLength(valueSchema.Values.Where(s => SchemaAbiTypes.StaticAbiTypes.Contains(s)))),
                    encodedLengths,
                    dynamicData,
                })
            );
        }
    }
}
