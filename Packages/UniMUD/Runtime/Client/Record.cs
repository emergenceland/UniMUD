using System;
using System.Collections.Generic;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public class Record
    {
        public bool Equals(Record other)
        {
            return table == other.table && key == other.key && attribute == other.attribute &&
                   value.Equals(other.value);
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
            return HashCode.Combine(table, key, attribute, value);
        }

        public string table;
        public string key;
        public string attribute;
        public object value;

        public Record(string table, string key, string attribute, object value)
        {
            this.table = table;
            this.key = key;
            this.attribute = attribute;
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
