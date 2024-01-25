using System;
using System.Collections.Generic;
using System.Linq;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using UnityEngine;
using static mud.Common;

namespace mud
{
    public partial class ProtocolParser
    {
        public static bool IsTableRegistrationLog(MudLog log)
        {
            var storeConfig = MudDefinitions.DefineStoreConfig(null);
            var schemasTable = storeConfig["Tables"];
            var schemasTableId = ResourceIDToHex(new ResourceID
            {
                Type = schemasTable.OffchainOnly != null ? ResourceType.OffchainTable : ResourceType.Table,
                Namespace = schemasTable.Namespace,
                Name = schemasTable.Name
            });
            if (log.eventName != "Store_SetRecord") return false;
            return string.Equals(log.args.tableId, schemasTableId, StringComparison.CurrentCultureIgnoreCase);
        }

        public static readonly Dictionary<string, SchemaAbiTypes.SchemaType> RegisterValueSchema = new()
        {
            { "fieldLayout", SchemaAbiTypes.SchemaType.BYTES32 },
            { "keySchema", SchemaAbiTypes.SchemaType.BYTES32 },
            { "valueSchema", SchemaAbiTypes.SchemaType.BYTES32 },
            { "abiEncodedKeyNames", SchemaAbiTypes.SchemaType.BYTES },
            { "abiEncodedFieldNames", SchemaAbiTypes.SchemaType.BYTES }
        };

        public static Table LogToTable(MudLog log)
        {
            var keyTuple = log.args.keyTuple;
            if (keyTuple.Length > 1)
                Debug.LogWarning(
                    "registerSchema event is expected to have only one key in key tuple, but got multiple.");

            var table = HexToResourceId(keyTuple[0]);

            var data = ConcatHex(new[] { log.args.staticData, log.args.encodedLengths, log.args.dynamicData });
            var value = DecodeValue(RegisterValueSchema, data);

            var keySchema = HexToSchema(value["keySchema"].ToString());
            var valueSchema = HexToSchema(value["valueSchema"].ToString());

            var paramDecoder = new ParameterDecoder();
            var decodedKeyNames = paramDecoder.DecodeDefaultData(value["abiEncodedKeyNames"].ToString(),
                new Parameter("string[]", "abiEncodedKeyNames", 0));
            var keyNames = decodedKeyNames[0].Result as List<string>;


            var decodedFieldNames = paramDecoder.DecodeDefaultData(value["abiEncodedFieldNames"].ToString(),
                new Parameter("string[]", "abiEncodedFieldNames", 0));
            var fieldNames = decodedFieldNames[0].Result as List<string>;

            var valueAbiTypes = valueSchema.StaticFields.Concat(valueSchema.DynamicFields).ToList();

            var tableKeySchema = keySchema.StaticFields
                .Select((schemaType, index) => new { Key = keyNames[index], Value = schemaType })
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var tableValueSchema = valueAbiTypes
                .Select((schemaType, index) => new { Key = fieldNames[index], Value = schemaType })
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return new Table
            {
                Address = log.address,
                TableId = log.args.tableId,
                Namespace = FormatGetRecordResult(table.Namespace)[0],
                Name = FormatGetRecordResult(table.Name)[0],
                KeySchema = tableKeySchema,
                ValueSchema = tableValueSchema,
            };
        }
    }
}
