using System;
using System.Collections.Generic;

namespace mud
{
    using Property = Dictionary<string, object>;


    public class RxRecord : IRxRecord
    {
        public string TableId { get; set; }
        public string Key { get; set; }
        public Property RawValue { get; set; }

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
            var valueEquals = PropEquals(RawValue, other.RawValue);
            return TableId == other.TableId && Key == other.Key && valueEquals;
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
            return HashCode.Combine(TableId, Key, RawValue);
        }

        public RxRecord(string tableId, string key, Property value)
        {
            this.TableId = tableId;
            this.Key = key;
            this.RawValue = value;
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
        public UpdateType Type;
        public RxTable Table;
        public object CurrentValue;
        public object PreviousValue;
        public string CurrentRecordKey { get; set; }
        public string PreviousRecordKey { get; set; }
    }
    
    public interface IRxRecord
    {
        string TableId { get; }
        string Key { get; }
        Dictionary<string, object> RawValue { get; }
    }
}
