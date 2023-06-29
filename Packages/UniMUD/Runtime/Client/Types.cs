#nullable enable

using System.Collections.Generic;

namespace mud.Client
{

    public class Table
    {
        public Dictionary<string, Record> Records { get; set; }
        
        public Table()
        {
            Records = new Dictionary<string, Record>();
        }
    }
    
    public class Types
    {
        public enum Type
        {
            Boolean,
            Number,
            OptionalNumber,
            BigInt,
            OptionalBigInt,
            String,
            OptionalString,
            NumberArray,
            OptionalNumberArray,
            BigIntArray,
            OptionalBigIntArray,
            StringArray,
            OptionalStringArray,
            Entity,
            OptionalEntity,
            EntityArray,
            OptionalEntityArray,
            T,
            OptionalT,
        }
    }
}
