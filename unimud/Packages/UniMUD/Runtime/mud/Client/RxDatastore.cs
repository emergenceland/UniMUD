#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mud
{
    using Property = Dictionary<string, object>;

    public class RxDatastore
    {
        // Table name -> table
        public readonly Dictionary<string, RxTable> store = new();
        
        public void RegisterTable(string id, string name, Dictionary<string, SchemaAbiTypes.SchemaType> schema)
        {
            if (store.TryGetValue(name, out _))
            {
                Debug.LogWarning($"Already registered table: {name} with {id}");
                return;
            }

            if(NetworkManager.Verbose) {
                Debug.Log($"RxTable Registered: {name} with {id}");
            }

            store.TryAdd(name, new RxTable(id, name, schema));
        }

        public RxTable CreateTable(string tNamespace, string tName,
            Dictionary<string, SchemaAbiTypes.SchemaType> schema, bool offchain = false)
        {
            var id = Common.ResourceIDToHex(new ResourceID
            (
                tNamespace,
                tName,
                offchain ? ResourceType.OffchainTable : ResourceType.Table
            ));
            return new RxTable(id, tName, schema);
        }

        public RxTable? TryGetTable(string name)
        {
            if (store.TryGetValue(name, out var table))
            {
                return table;
            }

            return null;
        }
        
        public RxTable? TryGetTableById(string id)
        {
            return store.Values.FirstOrDefault(table => string.Equals(table.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<IRxRecord> RunQuery(Query query)
        {
            return query.Run(store);
        }
    }
}
