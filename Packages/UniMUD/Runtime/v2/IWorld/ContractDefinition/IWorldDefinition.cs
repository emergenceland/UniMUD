using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace IWorld.ContractDefinition
{


    public partial class IWorldDeployment : IWorldDeploymentBase
    {
        public IWorldDeployment() : base(BYTECODE) { }
        public IWorldDeployment(string byteCode) : base(byteCode) { }
    }

    public class IWorldDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public IWorldDeploymentBase() : base(BYTECODE) { }
        public IWorldDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class CallFunction : CallFunctionBase { }

    [Function("call", "bytes")]
    public class CallFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes", "funcSelectorAndArgs", 3)]
        public virtual byte[] FuncSelectorAndArgs { get; set; }
    }

    public partial class DeleteRecordFunction : DeleteRecordFunctionBase { }

    [Function("deleteRecord")]
    public class DeleteRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
    }

    public partial class DeleteRecord1Function : DeleteRecord1FunctionBase { }

    [Function("deleteRecord")]
    public class DeleteRecord1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
    }

    public partial class EmitEphemeralRecord1Function : EmitEphemeralRecord1FunctionBase { }

    [Function("emitEphemeralRecord")]
    public class EmitEphemeralRecord1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes", "data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public partial class EmitEphemeralRecordFunction : EmitEphemeralRecordFunctionBase { }

    [Function("emitEphemeralRecord")]
    public class EmitEphemeralRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes", "data", 3)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetFieldFunction : GetFieldFunctionBase { }

    [Function("getField", "bytes")]
    public class GetFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
    }

    public partial class GetFieldLengthFunction : GetFieldLengthFunctionBase { }

    [Function("getFieldLength", "uint256")]
    public class GetFieldLengthFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes32", "schema", 4)]
        public virtual byte[] Schema { get; set; }
    }

    public partial class GetFieldSliceFunction : GetFieldSliceFunctionBase { }

    [Function("getFieldSlice", "bytes")]
    public class GetFieldSliceFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes32", "schema", 4)]
        public virtual byte[] Schema { get; set; }
        [Parameter("uint256", "start", 5)]
        public virtual BigInteger Start { get; set; }
        [Parameter("uint256", "end", 6)]
        public virtual BigInteger End { get; set; }
    }

    public partial class GetKeySchemaFunction : GetKeySchemaFunctionBase { }

    [Function("getKeySchema", "bytes32")]
    public class GetKeySchemaFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
    }

    public partial class GetRecord1Function : GetRecord1FunctionBase { }

    [Function("getRecord", "bytes")]
    public class GetRecord1FunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes32", "schema", 3)]
        public virtual byte[] Schema { get; set; }
    }

    public partial class GetRecordFunction : GetRecordFunctionBase { }

    [Function("getRecord", "bytes")]
    public class GetRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
    }

    public partial class GetSchemaFunction : GetSchemaFunctionBase { }

    [Function("getSchema", "bytes32")]
    public class GetSchemaFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
    }

    public partial class GrantAccessFunction : GrantAccessFunctionBase { }

    [Function("grantAccess")]
    public class GrantAccessFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("address", "grantee", 3)]
        public virtual string Grantee { get; set; }
    }

    public partial class IncrementFunction : IncrementFunctionBase { }

    [Function("increment", "uint32")]
    public class IncrementFunctionBase : FunctionMessage
    {

    }

    public partial class InstallModuleFunction : InstallModuleFunctionBase { }

    [Function("installModule")]
    public class InstallModuleFunctionBase : FunctionMessage
    {
        [Parameter("address", "module", 1)]
        public virtual string Module { get; set; }
        [Parameter("bytes", "args", 2)]
        public virtual byte[] Args { get; set; }
    }

    public partial class InstallRootModuleFunction : InstallRootModuleFunctionBase { }

    [Function("installRootModule")]
    public class InstallRootModuleFunctionBase : FunctionMessage
    {
        [Parameter("address", "module", 1)]
        public virtual string Module { get; set; }
        [Parameter("bytes", "args", 2)]
        public virtual byte[] Args { get; set; }
    }

    public partial class IsStoreFunction : IsStoreFunctionBase { }

    [Function("isStore")]
    public class IsStoreFunctionBase : FunctionMessage
    {

    }

    public partial class PopFromField1Function : PopFromField1FunctionBase { }

    [Function("popFromField")]
    public class PopFromField1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 4)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("uint256", "byteLengthToPop", 5)]
        public virtual BigInteger ByteLengthToPop { get; set; }
    }

    public partial class PopFromFieldFunction : PopFromFieldFunctionBase { }

    [Function("popFromField")]
    public class PopFromFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("uint256", "byteLengthToPop", 4)]
        public virtual BigInteger ByteLengthToPop { get; set; }
    }

    public partial class PushToFieldFunction : PushToFieldFunctionBase { }

    [Function("pushToField")]
    public class PushToFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes", "dataToPush", 4)]
        public virtual byte[] DataToPush { get; set; }
    }

    public partial class PushToField1Function : PushToField1FunctionBase { }

    [Function("pushToField")]
    public class PushToField1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 4)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes", "dataToPush", 5)]
        public virtual byte[] DataToPush { get; set; }
    }

    public partial class RegisterFunctionSelectorFunction : RegisterFunctionSelectorFunctionBase { }

    [Function("registerFunctionSelector", "bytes4")]
    public class RegisterFunctionSelectorFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("string", "systemFunctionName", 3)]
        public virtual string SystemFunctionName { get; set; }
        [Parameter("string", "systemFunctionArguments", 4)]
        public virtual string SystemFunctionArguments { get; set; }
    }

    public partial class RegisterHookFunction : RegisterHookFunctionBase { }

    [Function("registerHook")]
    public class RegisterHookFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("address", "hook", 3)]
        public virtual string Hook { get; set; }
    }

    public partial class RegisterNamespaceFunction : RegisterNamespaceFunctionBase { }

    [Function("registerNamespace")]
    public class RegisterNamespaceFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
    }

    public partial class RegisterRootFunctionSelectorFunction : RegisterRootFunctionSelectorFunctionBase { }

    [Function("registerRootFunctionSelector", "bytes4")]
    public class RegisterRootFunctionSelectorFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes4", "worldFunctionSelector", 3)]
        public virtual byte[] WorldFunctionSelector { get; set; }
        [Parameter("bytes4", "systemFunctionSelector", 4)]
        public virtual byte[] SystemFunctionSelector { get; set; }
    }

    public partial class RegisterSchemaFunction : RegisterSchemaFunctionBase { }

    [Function("registerSchema")]
    public class RegisterSchemaFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32", "schema", 2)]
        public virtual byte[] Schema { get; set; }
        [Parameter("bytes32", "keySchema", 3)]
        public virtual byte[] KeySchema { get; set; }
    }

    public partial class RegisterStoreHookFunction : RegisterStoreHookFunctionBase { }

    [Function("registerStoreHook")]
    public class RegisterStoreHookFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("address", "hook", 2)]
        public virtual string Hook { get; set; }
    }

    public partial class RegisterSystemFunction : RegisterSystemFunctionBase { }

    [Function("registerSystem", "bytes32")]
    public class RegisterSystemFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("address", "system", 3)]
        public virtual string System { get; set; }
        [Parameter("bool", "publicAccess", 4)]
        public virtual bool PublicAccess { get; set; }
    }

    public partial class RegisterSystemHookFunction : RegisterSystemHookFunctionBase { }

    [Function("registerSystemHook")]
    public class RegisterSystemHookFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("address", "hook", 3)]
        public virtual string Hook { get; set; }
    }

    public partial class RegisterTableFunction : RegisterTableFunctionBase { }

    [Function("registerTable", "bytes32")]
    public class RegisterTableFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32", "valueSchema", 3)]
        public virtual byte[] ValueSchema { get; set; }
        [Parameter("bytes32", "keySchema", 4)]
        public virtual byte[] KeySchema { get; set; }
    }

    public partial class RegisterTableHookFunction : RegisterTableHookFunctionBase { }

    [Function("registerTableHook")]
    public class RegisterTableHookFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("address", "hook", 3)]
        public virtual string Hook { get; set; }
    }

    public partial class RevokeAccessFunction : RevokeAccessFunctionBase { }

    [Function("revokeAccess")]
    public class RevokeAccessFunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("address", "grantee", 3)]
        public virtual string Grantee { get; set; }
    }

    public partial class SetFieldFunction : SetFieldFunctionBase { }

    [Function("setField")]
    public class SetFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes", "data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SetField1Function : SetField1FunctionBase { }

    [Function("setField")]
    public class SetField1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 4)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes", "data", 5)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SetMetadata1Function : SetMetadata1FunctionBase { }

    [Function("setMetadata")]
    public class SetMetadata1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("string", "tableName", 3)]
        public virtual string TableName { get; set; }
        [Parameter("string[]", "fieldNames", 4)]
        public virtual List<string> FieldNames { get; set; }
    }

    public partial class SetMetadataFunction : SetMetadataFunctionBase { }

    [Function("setMetadata")]
    public class SetMetadataFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("string", "tableName", 2)]
        public virtual string TableName { get; set; }
        [Parameter("string[]", "fieldNames", 3)]
        public virtual List<string> FieldNames { get; set; }
    }

    public partial class SetRecord1Function : SetRecord1FunctionBase { }

    [Function("setRecord")]
    public class SetRecord1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes", "data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SetRecordFunction : SetRecordFunctionBase { }

    [Function("setRecord")]
    public class SetRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes", "data", 3)]
        public virtual byte[] Data { get; set; }
    }

    public partial class UpdateInFieldFunction : UpdateInFieldFunctionBase { }

    [Function("updateInField")]
    public class UpdateInFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "table", 1)]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("uint256", "startByteIndex", 4)]
        public virtual BigInteger StartByteIndex { get; set; }
        [Parameter("bytes", "dataToSet", 5)]
        public virtual byte[] DataToSet { get; set; }
    }

    public partial class UpdateInField1Function : UpdateInField1FunctionBase { }

    [Function("updateInField")]
    public class UpdateInField1FunctionBase : FunctionMessage
    {
        [Parameter("bytes16", "namespace", 1)]
        public virtual byte[] Namespace { get; set; }
        [Parameter("bytes16", "name", 2)]
        public virtual byte[] Name { get; set; }
        [Parameter("bytes32[]", "key", 3)]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 4)]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("uint256", "startByteIndex", 5)]
        public virtual BigInteger StartByteIndex { get; set; }
        [Parameter("bytes", "dataToSet", 6)]
        public virtual byte[] DataToSet { get; set; }
    }



    public partial class StoreDeleteRecordEventDTO : StoreDeleteRecordEventDTOBase { }

    [Event("StoreDeleteRecord")]
    public class StoreDeleteRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "table", 1, false )]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2, false )]
        public virtual List<byte[]> Key { get; set; }
    }

    public partial class StoreEphemeralRecordEventDTO : StoreEphemeralRecordEventDTOBase { }

    [Event("StoreEphemeralRecord")]
    public class StoreEphemeralRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "table", 1, false )]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2, false )]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes", "data", 3, false )]
        public virtual byte[] Data { get; set; }
    }

    public partial class StoreSetFieldEventDTO : StoreSetFieldEventDTOBase { }

    [Event("StoreSetField")]
    public class StoreSetFieldEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "table", 1, false )]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2, false )]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("uint8", "schemaIndex", 3, false )]
        public virtual byte SchemaIndex { get; set; }
        [Parameter("bytes", "data", 4, false )]
        public virtual byte[] Data { get; set; }
    }

    public partial class StoreSetRecordEventDTO : StoreSetRecordEventDTOBase { }

    [Event("StoreSetRecord")]
    public class StoreSetRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "table", 1, false )]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2, false )]
        public virtual List<byte[]> Key { get; set; }
        [Parameter("bytes", "data", 3, false )]
        public virtual byte[] Data { get; set; }
    }

    public partial class AccessDeniedError : AccessDeniedErrorBase { }

    [Error("AccessDenied")]
    public class AccessDeniedErrorBase : IErrorDTO
    {
        [Parameter("string", "resource", 1)]
        public virtual string Resource { get; set; }
        [Parameter("address", "caller", 2)]
        public virtual string Caller { get; set; }
    }

    public partial class FunctionSelectorExistsError : FunctionSelectorExistsErrorBase { }

    [Error("FunctionSelectorExists")]
    public class FunctionSelectorExistsErrorBase : IErrorDTO
    {
        [Parameter("bytes4", "functionSelector", 1)]
        public virtual byte[] FunctionSelector { get; set; }
    }

    public partial class FunctionSelectorNotFoundError : FunctionSelectorNotFoundErrorBase { }

    [Error("FunctionSelectorNotFound")]
    public class FunctionSelectorNotFoundErrorBase : IErrorDTO
    {
        [Parameter("bytes4", "functionSelector", 1)]
        public virtual byte[] FunctionSelector { get; set; }
    }

    public partial class InvalidSelectorError : InvalidSelectorErrorBase { }

    [Error("InvalidSelector")]
    public class InvalidSelectorErrorBase : IErrorDTO
    {
        [Parameter("string", "resource", 1)]
        public virtual string Resource { get; set; }
    }

    public partial class ModuleAlreadyInstalledError : ModuleAlreadyInstalledErrorBase { }

    [Error("ModuleAlreadyInstalled")]
    public class ModuleAlreadyInstalledErrorBase : IErrorDTO
    {
        [Parameter("string", "module", 1)]
        public virtual string Module { get; set; }
    }

    public partial class ResourceExistsError : ResourceExistsErrorBase { }

    [Error("ResourceExists")]
    public class ResourceExistsErrorBase : IErrorDTO
    {
        [Parameter("string", "resource", 1)]
        public virtual string Resource { get; set; }
    }

    public partial class ResourceNotFoundError : ResourceNotFoundErrorBase { }

    [Error("ResourceNotFound")]
    public class ResourceNotFoundErrorBase : IErrorDTO
    {
        [Parameter("string", "resource", 1)]
        public virtual string Resource { get; set; }
    }

    public partial class StorecoreDataindexoverflowError : StorecoreDataindexoverflowErrorBase { }

    [Error("StoreCore_DataIndexOverflow")]
    public class StorecoreDataindexoverflowErrorBase : IErrorDTO
    {
        [Parameter("uint256", "length", 1)]
        public virtual BigInteger Length { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StorecoreInvaliddatalengthError : StorecoreInvaliddatalengthErrorBase { }

    [Error("StoreCore_InvalidDataLength")]
    public class StorecoreInvaliddatalengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StorecoreInvalidfieldnameslengthError : StorecoreInvalidfieldnameslengthErrorBase { }

    [Error("StoreCore_InvalidFieldNamesLength")]
    public class StorecoreInvalidfieldnameslengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StorecoreNotdynamicfieldError : StorecoreNotdynamicfieldErrorBase { }
    [Error("StoreCore_NotDynamicField")]
    public class StorecoreNotdynamicfieldErrorBase : IErrorDTO
    {
    }

    public partial class StorecoreNotimplementedError : StorecoreNotimplementedErrorBase { }
    [Error("StoreCore_NotImplemented")]
    public class StorecoreNotimplementedErrorBase : IErrorDTO
    {
    }

    public partial class StorecoreTablealreadyexistsError : StorecoreTablealreadyexistsErrorBase { }

    [Error("StoreCore_TableAlreadyExists")]
    public class StorecoreTablealreadyexistsErrorBase : IErrorDTO
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("string", "tableIdString", 2)]
        public virtual string TableIdString { get; set; }
    }

    public partial class StorecoreTablenotfoundError : StorecoreTablenotfoundErrorBase { }

    [Error("StoreCore_TableNotFound")]
    public class StorecoreTablenotfoundErrorBase : IErrorDTO
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("string", "tableIdString", 2)]
        public virtual string TableIdString { get; set; }
    }

    public partial class SystemExistsError : SystemExistsErrorBase { }

    [Error("SystemExists")]
    public class SystemExistsErrorBase : IErrorDTO
    {
        [Parameter("address", "system", 1)]
        public virtual string System { get; set; }
    }











    public partial class GetFieldOutputDTO : GetFieldOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetFieldLengthOutputDTO : GetFieldLengthOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldLengthOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetFieldSliceOutputDTO : GetFieldSliceOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldSliceOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetKeySchemaOutputDTO : GetKeySchemaOutputDTOBase { }

    [FunctionOutput]
    public class GetKeySchemaOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "schema", 1)]
        public virtual byte[] Schema { get; set; }
    }

    public partial class GetRecord1OutputDTO : GetRecord1OutputDTOBase { }

    [FunctionOutput]
    public class GetRecord1OutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetRecordOutputDTO : GetRecordOutputDTOBase { }

    [FunctionOutput]
    public class GetRecordOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetSchemaOutputDTO : GetSchemaOutputDTOBase { }

    [FunctionOutput]
    public class GetSchemaOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "schema", 1)]
        public virtual byte[] Schema { get; set; }
    }
























































}
