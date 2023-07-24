using System;
using System.Collections.Generic;
using mud.Network;

namespace mud.Client
{
    using Property = Dictionary<string, object>;


    public class Record
    {
        public string table;
        public string key;
        public Property value;

        private static bool PropEquals(Property x, Property y)
        {
            // If the dictionaries have different counts, they're not equal
            if (x.Count != y.Count)
                return false;

            // If any key-value pair is not equal, the dictionaries are not equal
            foreach (var pair in x)
            {
                if (!y.TryGetValue(pair.Key, out var value) || !value.Equals(pair.Value))
                    return false;
            }

            // All key-value pairs are equal
            return true;
        }

        public bool Equals(Record other)
        {
            var valueEquals = PropEquals(value, other.value);
            return table == other.table && key == other.key && valueEquals;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Record)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(table, key, value);
        }

        public Record(string table, string key, Property value)
        {
            this.table = table;
            this.key = key;
            this.value = value;
        }
    }

    public enum UpdateType
    {
        SetRecord,
        SetField,
        DeleteRecord
    }

    public class RecordUpdate
    {
        public UpdateType Type { get; set; }
        public string TableId { get; set; }
        public string TableName { get; set; }
        public string Key { get; set; }
        public Tuple<Property?, Property?> Value { get; set; }
    }


    public class TypedRecordUpdate<T> : RecordUpdate
    {
        public T TypedValue { get; set; }
    }
}
