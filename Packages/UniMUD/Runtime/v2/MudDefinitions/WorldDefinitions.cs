using System.Collections.Generic;
using mud.Client;
using mud.Network.schemas;
using Newtonsoft.Json;
using UnityEngine;
using static v2.ProtocolParser;
using static v2.SchemaAbiTypes;

namespace v2
{
    public partial class MudDefinitions
    {
        public static void DefineWorldConfig(string address, RxDatastore ds)
        {
            Table NamespaceOwner = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "NamespaceOwner",
                KeySchema = new Dictionary<string, SchemaType>
                    { { "namespaceId", SchemaType.BYTES32 } },
                ValueSchema = new Dictionary<string, SchemaType>
                    { { "owner", SchemaType.ADDRESS } }
            };

            Table ResourceAccess = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "ResourceAccess",
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
                Namespace = "world",
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

            Table UserDelegationControl = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "UserDelegationControl",
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

            Table NamespaceDelegationControl = new Table()
            {
                Address = address,
                Namespace = "world",
                Name = "NamespaceDelegationControl",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "namespaceId", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>()
                {
                    { "DelegationControlId", SchemaType.BYTES32 }
                }
            };

            Table Balances = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "Balances",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "namespaceId", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "balance", SchemaType.UINT256 }
                }
            };

            Table Systems = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "Systems",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "systemId", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "system", SchemaType.ADDRESS },
                    { "publicAccess", SchemaType.BOOL }
                }
            };

            Table SystemRegistry = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "SystemRegistry",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "system", SchemaType.ADDRESS },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "systemId", SchemaType.BYTES32 },
                }
            };

            Table SystemHooks = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "SystemHooks",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "systemId", SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "valueSchema", SchemaType.BYTES21_ARRAY }
                }
            };

            Table FunctionSelectors = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "FunctionSelectors",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "functionSelector", SchemaType.BYTES4 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "systemId", SchemaType.BYTES32 },
                    { "systemIdFunctionSelector", SchemaType.BYTES4 }
                }
            };

            Table FunctionSignatures = new Table
            {
                Address = address,
                Namespace = "world",
                Name = "FunctionSignatures",
                KeySchema = new Dictionary<string, SchemaType>
                {
                    { "functionSelector", SchemaType.BYTES4 },
                },
                ValueSchema = new Dictionary<string, SchemaType>
                {
                    { "functionSignature", SchemaType.STRING },
                }
            };

            var tables = new List<Table>()
            {
                NamespaceOwner,
                ResourceAccess,
                InstalledModules,
                UserDelegationControl,
                NamespaceDelegationControl,
                Balances,
                Systems,
                SystemRegistry,
                SystemHooks,
                // FunctionSignatures,
                FunctionSelectors
            };

            tables.ForEach(table =>
            {
                var newRxTable = ds.CreateTable(table.Namespace, table.Name, table.ValueSchema);
                Debug.Log($"{table.Name} - {newRxTable.Id}");
                if (ds.registeredTables.Contains(newRxTable.Id)) return;
                // var tableName = $"{newTable.Namespace}:{newTable.Name}";
                // TODO: figure out what to do with namespaces
                var tableName = $"{table.Name}";
                Debug.Log($"Registering table: {tableName}");
                ds.RegisterTable(newRxTable, tableName);
            });

            // TODO: handle offchain tables properly
            // This is dumb
            var functionSigRxTable = ds.CreateTable(FunctionSignatures.Namespace, FunctionSignatures.Name,
                FunctionSignatures.ValueSchema, true);
            ds.RegisterTable(functionSigRxTable, FunctionSignatures.Name);
        }
    }
}
