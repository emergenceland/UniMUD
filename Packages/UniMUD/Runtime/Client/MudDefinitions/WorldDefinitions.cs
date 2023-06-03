using System.Collections.Generic;
using mud.Client;
using mud.Network.schemas;

namespace mud.Client.MudDefinitions
{
    public class WorldDefinitions
    {
        public static void DefineDataStoreConfig(Datastore dataStore)
        {
            TableId nsOwnerId = new TableId("", "NamespaceOwner");
            TableId resourceAccessId = new TableId("", "ResourceAccess");
            TableId installedModulesId = new TableId("", "InstalledModules");
            TableId systemsId = new TableId("", "Systems");
            TableId systemRegistryId = new TableId("", "SystemRegistry");
            TableId resourceTypeId = new TableId("", "ResourceType");
            TableId funcSelectorId = new TableId("", "funcSelectors");

            dataStore.RegisterTable(
                nsOwnerId.ToString(),
                "NamespaceOwner",
                new Dictionary<string, Types.Type>
                {
                    { "owner", Types.Type.String }
                }
            );

            dataStore.RegisterTable(
                resourceAccessId.ToString(),
                "ResourceAccess",
                new Dictionary<string, Types.Type>
                {
                    { "access", Types.Type.Boolean }
                }
            );

            dataStore.RegisterTable(
                installedModulesId.ToString(),
                "InstalledModules",
                new Dictionary<string, Types.Type>
                {
                    { "moduleAddress", Types.Type.String }
                }
            );

            dataStore.RegisterTable(
                systemsId.ToString(),
                "Systems",
                new Dictionary<string, Types.Type>
                {
                    { "system", Types.Type.String },
                    { "publicAccess", Types.Type.Boolean }
                }
            );

            dataStore.RegisterTable(
                systemRegistryId.ToString(),
                "SystemRegistry",
                new Dictionary<string, Types.Type>
                {
                    { "resourceSelector", Types.Type.String }
                }
            );

            dataStore.RegisterTable(
                resourceTypeId.ToString(),
                "ResourceType",
                new Dictionary<string, Types.Type>
                {
                    { "resourceType", Types.Type.Number },
                }
            );

            dataStore.RegisterTable(
                funcSelectorId.ToString(),
                "funcSelectors",
                new Dictionary<string, Types.Type>
                {
                    { "namespace", Types.Type.String },
                    { "name", Types.Type.String },
                    { "systemFunctionSelector", Types.Type.String },
                }
            );
        }
    }
}
