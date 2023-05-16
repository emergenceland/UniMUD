using System.Collections.Generic;
using mud.Client;
using mud.Network.schemas;

namespace mud.Network.MudDefinitions
{
    public class StoreDefinitions
    {
        public static void DefineDataStoreConfig(Datastore dataStore)
        {
            TableId hooksId = new TableId("mudstore", "Hooks");
            TableId callbacksId = new TableId("mudstore", "Callbacks");
            TableId storeMetadataId = new TableId("mudstore", "StoreMetadata");
            TableId mixedId = new TableId("mudstore", "Mixed");

            dataStore.RegisterTable(
                hooksId.ToString(),
                "Hooks",
                new Dictionary<string, Types.Type> { { "value", Types.Type.StringArray } }
            );

            dataStore.RegisterTable(
                callbacksId.ToString(),
                "Callbacks",
                new Dictionary<string, Types.Type>
                {
                    { "value", Types.Type.BigIntArray }
                }
            );

            dataStore.RegisterTable(
                storeMetadataId.ToString(),
                "StoreMetadata",
                new Dictionary<string, Types.Type>
                {
                    { "tableName", Types.Type.String },
                    { "abiEncodedFieldNames", Types.Type.String }
                }
            );

            dataStore.RegisterTable(
                mixedId.ToString(),
                "Mixed",
                new Dictionary<string, Types.Type>
                {
                    { "u32", Types.Type.Number },
                    { "u128", Types.Type.BigInt },
                    { "a32", Types.Type.NumberArray },
                    { "s", Types.Type.String },
                }
            );
        }
    }
}
