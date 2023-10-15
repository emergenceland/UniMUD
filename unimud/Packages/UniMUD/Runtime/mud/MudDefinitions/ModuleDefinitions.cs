#nullable enable
using System.Collections.Generic;

namespace mud
{
    public static partial class MudDefinitions
    {
        public static Dictionary<string, ProtocolParser.Table> DefineModuleConfig(string? address)
        {
            ProtocolParser.Table KeysWithValue = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "KeysWithValue",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "valueHash", SchemaAbiTypes.SchemaType.BYTES32 }
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "keysWithValue", SchemaAbiTypes.SchemaType.BYTES32_ARRAY }
                }
            };

            ProtocolParser.Table KeysInTable = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "KeysInTable",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "sourceTableId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "keys0", SchemaAbiTypes.SchemaType.BYTES32_ARRAY },
                    { "keys1", SchemaAbiTypes.SchemaType.BYTES32_ARRAY},
                    { "keys2", SchemaAbiTypes.SchemaType.BYTES32_ARRAY},
                    { "keys3", SchemaAbiTypes.SchemaType.BYTES32_ARRAY},
                    { "keys4", SchemaAbiTypes.SchemaType.BYTES32_ARRAY}
                }
            };

            ProtocolParser.Table UsedKeysIndex = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "UsedKeysIndex",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "sourceTableId", SchemaAbiTypes.SchemaType.BYTES32 },
                    { "keysHash", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "has", SchemaAbiTypes.SchemaType.BOOL },
                    { "index", SchemaAbiTypes.SchemaType.UINT40 }
                }
            };

            ProtocolParser.Table UniqueEntity = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "UniqueEntity",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>(),
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "value", SchemaAbiTypes.SchemaType.UINT256 }
                }
            };
            
            ProtocolParser.Table CallboundDelegations = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "CallboundDelegations",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    {"delegator", SchemaAbiTypes.SchemaType.ADDRESS},
                    {"delegatee", SchemaAbiTypes.SchemaType.ADDRESS},
                    {"systemId", SchemaAbiTypes.SchemaType.BYTES32},
                    {"callDataHash", SchemaAbiTypes.SchemaType.BYTES32}
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "availableCalls", SchemaAbiTypes.SchemaType.UINT256 }
                }
            };
            
            ProtocolParser.Table TimeboundDelegations = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "TimeboundDelegations",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    {"delegator", SchemaAbiTypes.SchemaType.ADDRESS},
                    {"delegatee", SchemaAbiTypes.SchemaType.ADDRESS},
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "maxTimestamp", SchemaAbiTypes.SchemaType.UINT256 }
                }
            };

            return new Dictionary<string, ProtocolParser.Table>
            {
                { "KeysWithValue", KeysWithValue },
                {"KeysInTable", KeysInTable},
                {"UsedKeysIndex", UsedKeysIndex},
                {"UniqueEntity", UniqueEntity},
                {"CallboundDelegations", CallboundDelegations},
                {"TimeboundDelegations", TimeboundDelegations}
            };
        }
    }
}
