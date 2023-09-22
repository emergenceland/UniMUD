using System;
using System.Collections.Generic;
using System.Numerics;

namespace v2
{
    public struct PackedCounter
    {
        public BigInteger TotalByteLength;
        public UInt64[] FieldByteLengths;
    }

    public partial class ProtocolParser
    {
        public struct Table
        {
            public string Address;
            public string TableId;
            public string Namespace;

            public string Name;

            public Dictionary<string, SchemaAbiTypes.SchemaType> KeySchema;
            public Dictionary<string, SchemaAbiTypes.SchemaType> ValueSchema;
        }
    }
}