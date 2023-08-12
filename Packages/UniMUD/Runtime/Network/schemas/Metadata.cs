#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using mud.Network.IStore;
using mud.Network.IStore.ContractDefinition;
using Newtonsoft.Json;
using NLog;
using static mud.Network.schemas.Common;

namespace mud.Network.schemas
{
    public class TableMetadata
    {
        public string TableName { get; set; }
        public List<string> FieldNames { get; set; }
    }

    public static class Metadata
    {
        private static readonly Dictionary<string, TableMetadata> MetadataCache = new();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async UniTask<TableMetadata?> RegisterMetadata(IStoreService store, TableId table,
            TableMetadata? metadata = null)
        {
            var cacheKey = $"{store.ContractHandler.ContractAddress}:{table.ToHexString()}";

            var metadataIsCached = MetadataCache.TryGetValue(cacheKey, out var cachedMetadata);
            if (metadataIsCached)
            {
                if (metadata == null) return cachedMetadata;
                if (JsonConvert.SerializeObject(cachedMetadata) != JsonConvert.SerializeObject(metadata))
                {
                    Logger.Warn($"Different metadata already registered for this table, {table}");
                }

                return cachedMetadata;
            }

            if (metadata != null)
            {
                Logger.Debug($"Registering medata for table {table}");
                MetadataCache[cacheKey] = metadata;
                return metadata;
            }

            if (table.ToHexString() == DecodeStore.SchemaTableId.ToHexString() ||
                table.ToHexString() == DecodeStore.MetadataTableId.ToHexString())
            {
                return null;
            }

            var metadataSchema = await Schema.RegisterSchema(store, DecodeStore.MetadataTableId);

            if (metadataSchema.IsEmpty)
            {
                Logger.Warn(
                    $"Metadata schema not found: {DecodeStore.MetadataTableId}, {store.ContractHandler.ContractAddress}");
            }

            var metadataRecord = await FetchMetadata(store, table);

            if (metadataRecord == null || ByteArrayToHexString(metadataRecord) == "0x")
            {
                Logger.Warn(
                    $"Metadata not found for table: {table}, {store.ContractHandler.ContractAddress}");
            }

            var decoded = DataDecoder.DecodeData(metadataSchema, ByteArrayToHexString(metadataRecord));
            var tableName = (string)decoded[0];
            var fieldNames = DecodeGetRecordResult((string)decoded[1]);

            if (tableName != table.name)
            {
                Logger.Warn(
                    $"Metadata table name: {tableName} does not match ID {table.name}");
            }

            var md = new TableMetadata
            {
                TableName = tableName,
                FieldNames = fieldNames
            };

            MetadataCache[cacheKey] = md;
            return md;
        }

        private static async UniTask<byte[]?> FetchMetadata(IStoreService store, TableId table)
        {
            Logger.Debug($"Fetching metadata for table {table}, world: {store.ContractHandler.ContractAddress}");
            var getMetadata = new GetRecordFunction
            {
                Table = DecodeStore.MetadataTableId.ToBytes(),
                Key = new List<byte[]> { table.ToBytes() }
            };
            var metadataRecord = await store.ContractHandler.QueryAsync<GetRecordFunction, byte[]>(getMetadata);
            return metadataRecord;
        }

        public static List<string> DecodeGetRecordResult(string input)
        {
            var decodedString = HexToUTF8(input);
            
            var parts = Regex.Split(decodedString, @"[\x00-\x1F]|[\uFFFD]|[\s@`]")
                .Where(part => !string.IsNullOrWhiteSpace(part)).ToList();

            return parts;
        }
    }
}
