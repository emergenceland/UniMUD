#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;

namespace mud
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
            public string? Address;
            public string TableId;
            public string Namespace;

            public string Name;

            public Dictionary<string, SchemaAbiTypes.SchemaType> KeySchema;
            public Dictionary<string, SchemaAbiTypes.SchemaType> ValueSchema;
            public bool? OffchainOnly;
        }
    }
}
