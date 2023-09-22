using System.Collections.Generic;
using mud.Client;
using mud.Network.schemas;
using static v2.ProtocolParser;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class MudDefinitions
    {
        public static void DefineStoreConfig(string address)
        {
            Table StoreHooks = new Table
            {
                Address = address,
                Namespace = "mudstore",
                Name = "StoreHooks",
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "value", SchemaType.BYTES21_ARRAY }
                }
            };

            Table Callbacks = new Table
            {
                Address = address,
                Namespace = "mudstore",
                Name = "Callbacks",
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "value", SchemaType.BYTES21_ARRAY }
                }
            };

            Table Tables = new Table
            {
                Address = address,
                Namespace = "mudstore",
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
                Namespace = "mudstore",
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
                Namespace = "mudstore",
                Name = "Hooks",
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "value", SchemaType.BYTES21_ARRAY }
                }
            };
        }
    }
}
