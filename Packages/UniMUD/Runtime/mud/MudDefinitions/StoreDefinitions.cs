#nullable enable
using System.Collections.Generic;

namespace mud
{
    public static partial class MudDefinitions
    {
        public static Dictionary<string, ProtocolParser.Table> DefineStoreConfig(string? address)
        {
            ProtocolParser.Table StoreHooks = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "store",
                Name = "StoreHooks",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "tableId", SchemaAbiTypes.SchemaType.BYTES32 }
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "hooks", SchemaAbiTypes.SchemaType.BYTES21_ARRAY }
                }
            };

            ProtocolParser.Table Tables = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "store",
                Name = "Tables",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "tableId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "fieldLayout", SchemaAbiTypes.SchemaType.BYTES32 },
                    { "keySchema", SchemaAbiTypes.SchemaType.BYTES32 },
                    { "valueSchema", SchemaAbiTypes.SchemaType.BYTES32 },
                    { "abiEncodedKeyNames", SchemaAbiTypes.SchemaType.BYTES },
                    { "abiEncodedFieldNames", SchemaAbiTypes.SchemaType.BYTES }
                }
            };

            ProtocolParser.Table ResourceIds = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "store",
                Name = "ResourceIds",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "resourceId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "exists", SchemaAbiTypes.SchemaType.BOOL }
                }
            };

            ProtocolParser.Table Hooks = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "store",
                Name = "Hooks",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "resourceId", SchemaAbiTypes.SchemaType.BYTES32 }
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "value", SchemaAbiTypes.SchemaType.BYTES21_ARRAY }
                }
            };

            return new Dictionary<string, ProtocolParser.Table>
            {
                { "StoreHooks", StoreHooks },
                { "Tables", Tables },
                { "ResourceIds", ResourceIds },
                { "Hooks", Hooks }
            };
        }
    }
}
