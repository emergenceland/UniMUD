#nullable enable

using System.Collections.Generic;

namespace mud
{

    public class RxTable
    {
        public string Id { get; set; }
        public Dictionary<string, RxRecord> Values { get; set; } = new();
        public Dictionary<string, SchemaAbiTypes.SchemaType> Schema { get; set; } = new();
    }
}
