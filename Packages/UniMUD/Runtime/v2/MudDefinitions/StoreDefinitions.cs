using System.Collections.Generic;
using mud.Client;
using static v2.ProtocolParser;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class MudDefinitions
    {
        public static void DefineStoreConfig(string address, RxDatastore ds)
        {
            Table StoreHooks = new Table
            {
                Address = address,
                Namespace = "store",
                Name = "StoreHooks",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "tableId", SchemaType.BYTES32 }
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "hooks", SchemaType.BYTES21_ARRAY }
                }
            };

            Table Tables = new Table
            {
                Address = address,
                Namespace = "store",
                Name = "Tables",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "tableId", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "fieldLayout", SchemaType.BYTES32 },
                    { "keySchema", SchemaType.BYTES32 },
                    { "valueSchema", SchemaType.BYTES32 },
                    { "abiEncodedKeyNames", SchemaType.BYTES },
                    { "abiEncodedFieldNames", SchemaType.BYTES }
                }
            };

            Table ResourceIds = new Table
            {
                Address = address,
                Namespace = "store",
                Name = "ResourceIds",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "resourceId", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "exists", SchemaType.BOOL }
                }
            };

            Table Hooks = new Table
            {
                Address = address,
                Namespace = "store",
                Name = "Hooks",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "resourceId", SchemaType.BYTES32 }
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "value", SchemaType.BYTES21_ARRAY }
                }
            };

            var tables = new List<Table>
            {
                StoreHooks,
                Tables,
                ResourceIds,
                Hooks
            };

            tables.ForEach(table =>
            {
                var newRxTable = ds.CreateTable(table.Namespace, table.Name, table.ValueSchema);
                if (ds.registeredTables.Contains(newRxTable.Id)) return;
                // var tableName = $"{newTable.Namespace}:{newTable.Name}";
                // TODO: figure out what to do with namespaces
                var tableName = $"{table.Name}";
                ds.RegisterTable(newRxTable, tableName);
            });
        }
    }
}
