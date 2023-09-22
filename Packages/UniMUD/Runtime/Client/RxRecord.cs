using System;
using System.Collections.Generic;
using mud.Network;

namespace mud.Client
{
    using Property = Dictionary<string, object>;


    public class RxRecord
    {
        public string tableId;
        public string key;
        public Property value;

        private static bool PropEquals(Property x, Property y)
        {
            if (x.Count != y.Count) return false;

            foreach (var pair in x)
            {
                if (!y.TryGetValue(pair.Key, out var value) || !value.Equals(pair.Value)) return false;
            }

            return true;
        }

        public bool Equals(RxRecord other)
        {
            var valueEquals = PropEquals(value, other.value);
            return tableId == other.tableId && key == other.key && valueEquals;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RxRecord)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(tableId, key, value);
        }

        public RxRecord(string tableId, string key, Property value)
        {
            this.tableId = tableId;
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
