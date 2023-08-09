#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;

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

        public System.Type GetAbiType(Type t)
        {
            return t switch
            {
                Type.String => typeof(string),
                Type.Number => typeof(int),
                Type.Boolean => typeof(bool),
                Type.OptionalNumber => typeof(int?),
                Type.BigInt => typeof(BigInteger),
                Type.OptionalBigInt => typeof(BigInteger?),
                Type.OptionalString => typeof(string),
                Type.NumberArray => typeof(int[]),
                Type.OptionalNumberArray => typeof(int[]),
                Type.BigIntArray => typeof(BigInteger[]),
                Type.OptionalBigIntArray => typeof(BigInteger[]),
                Type.StringArray => typeof(string[]),
                Type.OptionalStringArray => typeof(string[]),
                Type.Entity => typeof(string),
                Type.OptionalEntity => typeof(string),
                Type.EntityArray => typeof(string[]),
                Type.OptionalEntityArray => typeof(string[]),
                Type.T => typeof(object),
                Type.OptionalT => typeof(object),
                _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
            };
        }
    }
}
