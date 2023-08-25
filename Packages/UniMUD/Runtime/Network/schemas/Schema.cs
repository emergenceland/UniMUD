#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using mud.Network.IStore;
using mud.Network.IStore.ContractDefinition;
using Nethereum.Unity.Rpc;
using Newtonsoft.Json;
using NLog;
using UnityEngine;
using static mud.Network.schemas.SchemaTypes;
using static mud.Network.schemas.Common;
using Logger = NLog.Logger;

namespace mud.Network.schemas
{
    public class Schema
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ConcurrentDictionary<string, Lazy<TableSchema>> _schemaCache =
            new();

        public TableSchema? GetSchema(string worldAddress, TableId table)
        {
            var cacheKey = $"{worldAddress}:{table.ToHexString()}";
            return _schemaCache[cacheKey].Value;
        }

        public static async UniTask<TableSchema> RegisterSchema(string storeContractAddress, string account, TableId table,
            string? rawSchema = null)
        {
            var cacheKey = $"{storeContractAddress}:{table.ToHexString()}";
            var existingSchema = _schemaCache.ContainsKey(cacheKey);
            if (existingSchema)
            {
                if (rawSchema != null)
                {
                    var schema = _schemaCache[cacheKey];
                    if (ByteArrayToHexString(schema.Value.RawSchema) != rawSchema)
                    {
                        Logger.Warn("A different schema was already registered for this table");
                    }
                }

                return _schemaCache[cacheKey].Value;
            }

            if (rawSchema != null)
            {
                Logger.Debug("Registering schema for table... " + table);
                var schema = DecodeSchema(HexStringToByteArray(rawSchema));
                _schemaCache.AddOrUpdate(cacheKey, new Lazy<TableSchema>(() => schema), (key, oldValue) =>
                    new Lazy<TableSchema>(
                        () =>
                            schema
                    ));
                return schema;
            }

            var schemaResult = await FetchSchema(storeContractAddress, account, table);

            var decodedSchema = DecodeSchema(schemaResult);
            if (decodedSchema.IsEmpty)
            {
                Logger.Warn($"Schema not found for table: {table}");
            }

            _schemaCache.AddOrUpdate(cacheKey, new Lazy<TableSchema>(() => decodedSchema), (key, oldValue) =>
                new Lazy<TableSchema>(
                    () =>
                        decodedSchema
                ));
            return decodedSchema;
        }

        private static async UniTask<byte[]> FetchSchema(string storeContractAddress, string account, TableId table)
        {
            Logger.Debug($"Fetching schema for table: {table}, world: {storeContractAddress}");
            var getSchema = new GetSchemaFunction
            {
                Table = table.ToBytes()
            };
            
            var schemaRequest = new QueryUnityRequest<GetSchemaFunction, GetSchemaOutputDTO>("http://localhost:8545", account);
            Debug.Log(JsonConvert.SerializeObject(schemaRequest));
            await schemaRequest.Query(getSchema, storeContractAddress).ToUniTask();
            Debug.Log(JsonConvert.SerializeObject(schemaRequest));
            return schemaRequest.Result.Schema;
            // var schemaResult = await store.ContractHandler.QueryAsync<GetSchemaFunction, byte[]>(getSchema);
            // return schemaResult;
        }

        public static TableSchema DecodeSchema(byte[] rawSchema)
        {
            var isEmpty = rawSchema.Length == 0 || ByteArrayToHexString(rawSchema) == "0x";
            byte[] schemaByteArray = isEmpty ? new byte[32] : rawSchema;
            var staticDataLength = BinaryPrimitives.ReadUInt16BigEndian(schemaByteArray.AsSpan(0, 2));
            var numStaticFields = schemaByteArray[2];
            var numDynamicFields = schemaByteArray[3];
            var staticFields = new List<SchemaType>();
            var dynamicFields = new List<SchemaType>();

            for (int i = 4; i < 4 + numStaticFields; i++)
            {
                staticFields.Add((SchemaType)schemaByteArray[i]);
            }

            for (int i = 4 + numStaticFields; i < 4 + numStaticFields + numDynamicFields; i++)
            {
                dynamicFields.Add((SchemaType)schemaByteArray[i]);
            }

            var actualStaticDataLength =
                staticFields.Aggregate(0, (acc, fieldType) => acc + GetStaticByteLength(fieldType));

            if (actualStaticDataLength != staticDataLength)
            {
                Logger.Error(
                    $"Schema static data length mismatch! Is `GetStaticByteLength` outdated? schemaStaticDataLength: {staticDataLength}, actualStaticDataLength: {actualStaticDataLength}, rawSchema: {rawSchema}");
                throw new InvalidOperationException(
                    "Schema static data length mismatch! Is `GetStaticByteLength` outdated?");
            }

            var abiTypes = staticFields.Concat(dynamicFields).Select(type => Map[type]).ToList();
            var abi = $"({string.Join(",", abiTypes)})";

            return new TableSchema(staticDataLength, staticFields, dynamicFields, rawSchema, abi, isEmpty);
        }

        public static int GetStaticByteLength(SchemaType schemaType)
        {
            var val = (int)schemaType;
            if (val < 32)
            {
                // uint8-256
                return val + 1;
            }

            if (val < 64)
            {
                // int8-256, offset by 32
                return val + 1 - 32;
            }

            if (val < 96)
            {
                // bytes1-32, offset by 64
                return val + 1 - 64;
            }

            // Other static types
            if (schemaType == SchemaType.BOOL)
            {
                return 1;
            }

            if (schemaType == SchemaType.ADDRESS)
            {
                return 20;
            }

            // Return 0 for all dynamic types
            return 0;
        }
    }

    public struct TableSchema
    {
        public int StaticDataLength { get; }
        public List<SchemaType> StaticFields { get; }
        public List<SchemaType> DynamicFields { get; }
        public byte[] RawSchema { get; }
        public string Abi { get; }
        public bool IsEmpty { get; }

        public TableSchema(int staticDataLength, List<SchemaType> staticFields, List<SchemaType> dynamicFields,
            byte[] rawSchema, string abi, bool isEmpty)
        {
            StaticDataLength = staticDataLength;
            StaticFields = staticFields;
            DynamicFields = dynamicFields;
            RawSchema = rawSchema;
            Abi = abi;
            IsEmpty = isEmpty;
        }
    }
}
