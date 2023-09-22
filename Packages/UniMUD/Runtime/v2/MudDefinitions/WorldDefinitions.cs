using System.Collections.Generic;
using mud.Client;
using mud.Network.schemas;
using static v2.ProtocolParser;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class MudDefinitions
    {
        public static void DefineWorldConfig(string address)
        {
            /************************************************************************
             *
             *    CORE TABLES
             *
             ************************************************************************/

            Table NamespaceOwner = new Table
            {
                Address = address,
                Namespace = "",
                Name = "NamespaceOwner",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "namespaceId", SchemaType.BYTES32 }
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    {
                        "owner", SchemaType.ADDRESS
                    }
                }
            };
            
            Table ResourceOwner = new Table
            {
                Address = address,
                Namespace = "",
                Name = "ResourceOwner",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "resourceId", SchemaType.BYTES32 },
                    { "caller", SchemaType.ADDRESS }
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    {
                        "access", SchemaType.BOOL
                    }
                }
            };
            
            Table InstalledModules = new Table
            {
                Address = address,
                Namespace = "",
                Name = "InstalledModules",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "moduleName", SchemaType.BYTES16 },
                    { "argumentsHash", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    {
                        "moduleAddress", SchemaType.ADDRESS
                    }
                }
            };
            
            Table Delegations = new Table
            {
                Address = address,
                Namespace = "",
                Name = "Delegations",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "delegator", SchemaType.ADDRESS },
                    { "delegatee", SchemaType.ADDRESS },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    {
                        "delegationControlId", SchemaType.BYTES32
                    }
                }
            };
            
            /************************************************************************
             *
             *    MODULE TABLES
             *
             ************************************************************************/

            // TableId nsOwnerId = new TableId("", "NamespaceOwner");
            // TableId resourceAccessId = new TableId("", "ResourceAccess");
            // TableId installedModulesId = new TableId("", "InstalledModules");
            // TableId systemsId = new TableId("", "Systems");
            // TableId systemRegistryId = new TableId("", "SystemRegistry");
            // TableId resourceTypeId = new TableId("", "ResourceType");
            // TableId funcSelectorId = new TableId("", "funcSelectors");
            //
            // dataStore.RegisterTable(
            //     nsOwnerId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "owner", Types.Type.String }
            //     }
            // );
            //
            // dataStore.RegisterTable(
            //     resourceAccessId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "access", Types.Type.Boolean }
            //     }
            // );
            //
            // dataStore.RegisterTable(
            //     installedModulesId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "moduleAddress", Types.Type.String }
            //     }
            // );
            //
            // dataStore.RegisterTable(
            //     systemsId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "system", Types.Type.String },
            //         { "publicAccess", Types.Type.Boolean }
            //     }
            // );
            //
            // dataStore.RegisterTable(
            //     systemRegistryId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "resourceSelector", Types.Type.String }
            //     }
            // );
            //
            // dataStore.RegisterTable(
            //     resourceTypeId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "resourceType", Types.Type.Number },
            //     }
            // );
            //
            // dataStore.RegisterTable(
            //     funcSelectorId,
            //     new Dictionary<string, Types.Type>
            //     {
            //         { "namespace", Types.Type.String },
            //         { "name", Types.Type.String },
            //         { "systemFunctionSelector", Types.Type.String },
            //     }
            // );
        }
    }
}
