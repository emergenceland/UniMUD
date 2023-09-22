using System.Collections.Generic;
using System.Linq;
using mud.Network.schemas;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;
using v2.IStore.ContractDefinition;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class ProtocolParser
    {
        public struct Table
        {
            public string Address;
            public string TableId;
            public string Namespace;

            public string Name;

            public Dictionary<string, SchemaType> KeySchema;
            public Dictionary<string, SchemaType> ValueSchema;
        }

        public static bool IsTableRegistrationLog(FilterLog log)
        {
            // TODO: make this configurable via storeConfig
            var schemasTableId = Common.ResourceIDToHex(new ResourceID
            {
                Type = ResourceType.Table,
                Namespace = "mudstore",
                Name = "Tables"
            });

            var storeSetRecordSignature =
                new StoreSetRecordEventDTO().GetEventABI().Sha3Signature;
            if (!log.IsLogForEvent(storeSetRecordSignature)) return false;
            var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log);
            var tableId = TableId.FromBytes32(decoded.Event.TableId);
            return tableId.ToHexString() == schemasTableId.ToLower();
        }

        public static readonly Dictionary<string, SchemaAbiTypes.SchemaType> RegisterValueSchema = new()
        {
            { "fieldLayout", SchemaType.BYTES32 },
            { "keySchema", SchemaType.BYTES32 },
            { "valueSchema", SchemaType.BYTES32 },
            { "abiEncodedKeyNames", SchemaType.BYTES },
            { "abiEncodedFieldNames", SchemaType.BYTES }
        };

        public static Table LogToTable(FilterLog log)
        {
            var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log);
            var keyTuple = decoded.Event.KeyTuple.Select(key => TableId.FromBytes32(key).ToHexString()).ToList();
            if (keyTuple.Count > 1)
                Debug.LogWarning(
                    "registerSchema event is expected to have only one key in key tuple, but got multiple.");
            // var table = TableId.FromHexString(keyTuple[0]);
            var table = Common.HexToResourceId(keyTuple[0]);

            Debug.Log("Parsing table: " + table.Name);
            var staticData = Common.BytesToHex(decoded.Event.StaticData);
            var encodedLengths = Common.BytesToHex(decoded.Event.EncodedLengths);
            var dynamicData = Common.BytesToHex(decoded.Event.DynamicData);

            var data = Common.ConcatHex(new[] { staticData, encodedLengths, dynamicData });
            var value = ProtocolParser.DecodeValue(RegisterValueSchema, data);

            var result = new Dictionary<string, object>();
            int i = 0;
            foreach (var key in RegisterValueSchema.Keys)
            {
                result.Add(key, value[i]);
                i++;
            }

            var keySchema = ProtocolParser.HexToSchema(result["keySchema"].ToString());
            var valueSchema = ProtocolParser.HexToSchema(result["valueSchema"].ToString());

            var paramDecoder = new ParameterDecoder();
            var decodedKeyNames = paramDecoder.DecodeDefaultData(result["abiEncodedKeyNames"].ToString(),
                new Parameter("string[]", "abiEncodedKeyNames", 0));
            var keyNames = decodedKeyNames[0].Result as List<string>;


            var decodedFieldNames = paramDecoder.DecodeDefaultData(result["abiEncodedFieldNames"].ToString(),
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
                Address = log.Address,
                TableId = keyTuple[0],
                Namespace = table.Namespace,
                Name = table.Name,
                KeySchema = tableKeySchema,
                ValueSchema = tableValueSchema,
            };
        }
    }
}
