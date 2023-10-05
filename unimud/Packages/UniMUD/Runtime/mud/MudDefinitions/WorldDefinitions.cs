#nullable enable
using System.Collections.Generic;


namespace mud
{
    public static partial class MudDefinitions
    {
        public static Dictionary<string, ProtocolParser.Table> DefineWorldConfig(string? address)
        {
            ProtocolParser.Table NamespaceOwner = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "NamespaceOwner",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                    { { "namespaceId", SchemaAbiTypes.SchemaType.BYTES32 } },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                    { { "owner", SchemaAbiTypes.SchemaType.ADDRESS } }
            };

            ProtocolParser.Table ResourceAccess = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "ResourceAccess",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "resourceId", SchemaAbiTypes.SchemaType.BYTES32 },
                    { "caller", SchemaAbiTypes.SchemaType.ADDRESS }
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    {
                        "access", SchemaAbiTypes.SchemaType.BOOL
                    }
                }
            };

            ProtocolParser.Table InstalledModules = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "InstalledModules",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "moduleName", SchemaAbiTypes.SchemaType.BYTES16 },
                    { "argumentsHash", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    {
                        "moduleAddress", SchemaAbiTypes.SchemaType.ADDRESS
                    }
                }
            };

            ProtocolParser.Table UserDelegationControl = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "UserDelegationControl",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "delegator", SchemaAbiTypes.SchemaType.ADDRESS },
                    { "delegatee", SchemaAbiTypes.SchemaType.ADDRESS },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    {
                        "delegationControlId", SchemaAbiTypes.SchemaType.BYTES32
                    }
                }
            };

            ProtocolParser.Table NamespaceDelegationControl = new ProtocolParser.Table()
            {
                Address = address,
                Namespace = "world",
                Name = "NamespaceDelegationControl",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "namespaceId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>()
                {
                    { "DelegationControlId", SchemaAbiTypes.SchemaType.BYTES32 }
                }
            };

            ProtocolParser.Table Balances = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "Balances",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "namespaceId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "balance", SchemaAbiTypes.SchemaType.UINT256 }
                }
            };

            ProtocolParser.Table Systems = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "Systems",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "systemId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "system", SchemaAbiTypes.SchemaType.ADDRESS },
                    { "publicAccess", SchemaAbiTypes.SchemaType.BOOL }
                }
            };

            ProtocolParser.Table SystemRegistry = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "SystemRegistry",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "system", SchemaAbiTypes.SchemaType.ADDRESS },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "systemId", SchemaAbiTypes.SchemaType.BYTES32 },
                }
            };

            ProtocolParser.Table SystemHooks = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "SystemHooks",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "systemId", SchemaAbiTypes.SchemaType.BYTES32 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "valueSchema", SchemaAbiTypes.SchemaType.BYTES21_ARRAY }
                }
            };

            ProtocolParser.Table FunctionSelectors = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "FunctionSelectors",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "functionSelector", SchemaAbiTypes.SchemaType.BYTES4 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "systemId", SchemaAbiTypes.SchemaType.BYTES32 },
                    { "systemIdFunctionSelector", SchemaAbiTypes.SchemaType.BYTES4 }
                }
            };

            ProtocolParser.Table FunctionSignatures = new ProtocolParser.Table
            {
                Address = address,
                Namespace = "world",
                Name = "FunctionSignatures",
                KeySchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "functionSelector", SchemaAbiTypes.SchemaType.BYTES4 },
                },
                ValueSchema = new Dictionary<string, SchemaAbiTypes.SchemaType>
                {
                    { "functionSignature", SchemaAbiTypes.SchemaType.STRING },
                },
                OffchainOnly = true
            };

            return new Dictionary<string, ProtocolParser.Table>
            {
                { "NamespaceOwner", NamespaceOwner },
                { "ResourceAccess", ResourceAccess },
                { "InstalledModules", InstalledModules },
                { "UserDelegationControl", UserDelegationControl },
                { "NamespaceDelegationControl", NamespaceDelegationControl },
                { "Balances", Balances },
                { "Systems", Systems },
                { "SystemRegistry", SystemRegistry },
                { "SystemHooks", SystemHooks },
                { "FunctionSignatures", FunctionSignatures },
                { "FunctionSelectors", FunctionSelectors }
            };
        }

  

    }
}
