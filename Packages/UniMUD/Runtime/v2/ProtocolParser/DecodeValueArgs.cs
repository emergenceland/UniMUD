using System.Collections.Generic;
using System.Linq;
using static v2.Common;

namespace v2
{
    public partial class ProtocolParser
    {
        public static Dictionary<string, object> DecodeValueArgs(
            Dictionary<string, SchemaAbiTypes.SchemaType> valueSchema,
            string staticData,
            string encodedLengths,
            string dynamicData
        )
        {
            var staticDataLength =
                valueSchema.Values.Where(v => SchemaAbiTypes.StaticAbiTypes.Contains(v)).Aggregate(0,
                    (length, fieldType) => length + SchemaAbiTypes.GetStaticByteLength[fieldType]);
            return DecodeValue(
                valueSchema,
                ConcatHex(new[]
                {
                    ReadHex(staticData, 0, staticDataLength),
                    encodedLengths,
                    dynamicData,
                })
            );
        }
    }
}
