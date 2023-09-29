using System.Collections.Generic;
using System.Linq;
using mud.Network.schemas;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using UnityEngine;
using v2.IStore.ContractDefinition;
using static v2.Common;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class ProtocolParser
    {
        public static bool IsTableRegistrationLog(FilterLog log)
        {
            // TODO: make this configurable via storeConfig
            var schemasTableId = ResourceIDToHex(new ResourceID
            {
                Type = ResourceType.Table,
                Namespace = "store",
                Name = "Tables"
            });

            var storeSetRecordSignature =
                new StoreSetRecordEventDTO().GetEventABI().Sha3Signature;
            if (!log.IsLogForEvent(storeSetRecordSignature)) return false;
            var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log);
            var tableId = TableId.FromBytes32(decoded.Event.TableId);
            return tableId.ToHexString() == schemasTableId.ToLower();
        }

        public static readonly Dictionary<string, SchemaType> RegisterValueSchema = new()
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
            var keyTuple = decoded.Event.KeyTuple.Select(key => BytesToHex(key)).ToList();
            if (keyTuple.Count > 1)
                Debug.LogWarning(
                    "registerSchema event is expected to have only one key in key tuple, but got multiple.");

            var table = HexToResourceId(keyTuple[0]);

            var staticData = BytesToHex(decoded.Event.StaticData);
            var encodedLengths = BytesToHex(decoded.Event.EncodedLengths);
            var dynamicData = BytesToHex(decoded.Event.DynamicData);

            var data = ConcatHex(new[] { staticData, encodedLengths, dynamicData });
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
                Address = log.Address,
                TableId = keyTuple[0],
                Namespace = FormatGetRecordResult(table.Namespace)[0],
                Name = FormatGetRecordResult(table.Name)[0],
                KeySchema = tableKeySchema,
                ValueSchema = tableValueSchema,
            };
        }
    }
}
