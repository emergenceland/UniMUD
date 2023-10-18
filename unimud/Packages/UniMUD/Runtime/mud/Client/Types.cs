#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace mud
{

    public class RxTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, RxRecord> Entries { get; set; } = new();
        public Dictionary<string, SchemaAbiTypes.SchemaType> Schema { get; set; } = new();
        
        public RxTable(string id, string name, Dictionary<string, SchemaAbiTypes.SchemaType> schema) 
        {
            Id = id;
            Name = name;
            Entries = new Dictionary<string, RxRecord>();
            Schema = schema;
        }
        
        public void Set(string key, object value)
        {
            bool exists = Entries.ContainsKey(key);
            
            var record = new RxRecord(Id, key, (Dictionary<string, object>)value); // TODO: this is a hack

            if(exists) { Entries[key] = record; } 
            else { Entries.Add(key, record); }
        }

        public RxRecord? Update(string key, object value)
        {
            var oldValue = Delete(key);
            Set(key, value);
            return oldValue;
        }
        
        public RxRecord? Delete(string key)
        {
            if (Entries.TryGetValue(key, out var previousValue))
            {
                Entries.Remove(key);
                return previousValue;
            }

            return null;
        }
        
        public RxRecord? GetValue(string key) => Entries.TryGetValue(key, out var value) ? value : null;
    }
}
