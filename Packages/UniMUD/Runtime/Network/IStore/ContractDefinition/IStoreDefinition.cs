using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace mud.Network.IStore.ContractDefinition
{


    public partial class IStoreDeployment : IStoreDeploymentBase
    {
        public IStoreDeployment() : base(BYTECODE) { }
        public IStoreDeployment(string byteCode) : base(byteCode) { }
    }

    public class IStoreDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public IStoreDeploymentBase() : base(BYTECODE) { }
        public IStoreDeploymentBase(string byteCode) : base(byteCode) { }

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

    public partial class IsStoreFunction : IsStoreFunctionBase { }

    [Function("isStore")]
    public class IsStoreFunctionBase : FunctionMessage
    {

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

    public partial class StoreDeleteRecordEventDTO : StoreDeleteRecordEventDTOBase { }

    [Event("StoreDeleteRecord")]
    public class StoreDeleteRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "table", 1, false )]
        public virtual byte[] Table { get; set; }
        [Parameter("bytes32[]", "key", 2, false )]
        public virtual List<byte[]> Key { get; set; }
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



    public partial class GetFieldOutputDTO : GetFieldOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldOutputDTOBase : IFunctionOutputDTO 
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
