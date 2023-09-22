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

namespace v2.IStore.ContractDefinition
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
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("bytes32", "fieldLayout", 3)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class GetDynamicFieldFunction : GetDynamicFieldFunctionBase { }

    [Function("getDynamicField", "bytes")]
    public class GetDynamicFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
    }

    public partial class GetFieldFunction : GetFieldFunctionBase { }

    [Function("getField", "bytes")]
    public class GetFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("bytes32", "fieldLayout", 4)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class GetFieldLayoutFunction : GetFieldLayoutFunctionBase { }

    [Function("getFieldLayout", "bytes32")]
    public class GetFieldLayoutFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
    }

    public partial class GetFieldLengthFunction : GetFieldLengthFunctionBase { }

    [Function("getFieldLength", "uint256")]
    public class GetFieldLengthFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("bytes32", "fieldLayout", 4)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class GetFieldSliceFunction : GetFieldSliceFunctionBase { }

    [Function("getFieldSlice", "bytes")]
    public class GetFieldSliceFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("bytes32", "fieldLayout", 4)]
        public virtual byte[] FieldLayout { get; set; }
        [Parameter("uint256", "start", 5)]
        public virtual BigInteger Start { get; set; }
        [Parameter("uint256", "end", 6)]
        public virtual BigInteger End { get; set; }
    }

    public partial class GetKeySchemaFunction : GetKeySchemaFunctionBase { }

    [Function("getKeySchema", "bytes32")]
    public class GetKeySchemaFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
    }

    public partial class GetRecordFunction : GetRecordFunctionBase { }

    [Function("getRecord", typeof(GetRecordOutputDTO))]
    public class GetRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("bytes32", "fieldLayout", 3)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class GetStaticFieldFunction : GetStaticFieldFunctionBase { }

    [Function("getStaticField", "bytes32")]
    public class GetStaticFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("bytes32", "fieldLayout", 4)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class GetValueSchemaFunction : GetValueSchemaFunctionBase { }

    [Function("getValueSchema", "bytes32")]
    public class GetValueSchemaFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
    }

    public partial class PopFromFieldFunction : PopFromFieldFunctionBase { }

    [Function("popFromField")]
    public class PopFromFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("uint256", "byteLengthToPop", 4)]
        public virtual BigInteger ByteLengthToPop { get; set; }
        [Parameter("bytes32", "fieldLayout", 5)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class PushToFieldFunction : PushToFieldFunctionBase { }

    [Function("pushToField")]
    public class PushToFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("bytes", "dataToPush", 4)]
        public virtual byte[] DataToPush { get; set; }
        [Parameter("bytes32", "fieldLayout", 5)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class RegisterStoreHookFunction : RegisterStoreHookFunctionBase { }

    [Function("registerStoreHook")]
    public class RegisterStoreHookFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("address", "hookAddress", 2)]
        public virtual string HookAddress { get; set; }
        [Parameter("uint8", "enabledHooksBitmap", 3)]
        public virtual byte EnabledHooksBitmap { get; set; }
    }

    public partial class RegisterTableFunction : RegisterTableFunctionBase { }

    [Function("registerTable")]
    public class RegisterTableFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32", "fieldLayout", 2)]
        public virtual byte[] FieldLayout { get; set; }
        [Parameter("bytes32", "keySchema", 3)]
        public virtual byte[] KeySchema { get; set; }
        [Parameter("bytes32", "valueSchema", 4)]
        public virtual byte[] ValueSchema { get; set; }
        [Parameter("string[]", "keyNames", 5)]
        public virtual List<string> KeyNames { get; set; }
        [Parameter("string[]", "fieldNames", 6)]
        public virtual List<string> FieldNames { get; set; }
    }

    public partial class SetFieldFunction : SetFieldFunctionBase { }

    [Function("setField")]
    public class SetFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("bytes", "data", 4)]
        public virtual byte[] Data { get; set; }
        [Parameter("bytes32", "fieldLayout", 5)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class SetRecordFunction : SetRecordFunctionBase { }

    [Function("setRecord")]
    public class SetRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("bytes", "staticData", 3)]
        public virtual byte[] StaticData { get; set; }
        [Parameter("bytes32", "encodedLengths", 4)]
        public virtual byte[] EncodedLengths { get; set; }
        [Parameter("bytes", "dynamicData", 5)]
        public virtual byte[] DynamicData { get; set; }
        [Parameter("bytes32", "fieldLayout", 6)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class SpliceDynamicDataFunction : SpliceDynamicDataFunctionBase { }

    [Function("spliceDynamicData")]
    public class SpliceDynamicDataFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
        [Parameter("uint40", "startWithinField", 4)]
        public virtual ulong StartWithinField { get; set; }
        [Parameter("uint40", "deleteCount", 5)]
        public virtual ulong DeleteCount { get; set; }
        [Parameter("bytes", "data", 6)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SpliceStaticDataFunction : SpliceStaticDataFunctionBase { }

    [Function("spliceStaticData")]
    public class SpliceStaticDataFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint48", "start", 3)]
        public virtual ulong Start { get; set; }
        [Parameter("uint40", "deleteCount", 4)]
        public virtual ulong DeleteCount { get; set; }
        [Parameter("bytes", "data", 5)]
        public virtual byte[] Data { get; set; }
    }

    public partial class StoreVersionFunction : StoreVersionFunctionBase { }

    [Function("storeVersion", "bytes32")]
    public class StoreVersionFunctionBase : FunctionMessage
    {

    }

    public partial class UnregisterStoreHookFunction : UnregisterStoreHookFunctionBase { }

    [Function("unregisterStoreHook")]
    public class UnregisterStoreHookFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("address", "hookAddress", 2)]
        public virtual string HookAddress { get; set; }
    }

    public partial class UpdateInFieldFunction : UpdateInFieldFunctionBase { }

    [Function("updateInField")]
    public class UpdateInFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "fieldIndex", 3)]
        public virtual byte FieldIndex { get; set; }
        [Parameter("uint256", "startByteIndex", 4)]
        public virtual BigInteger StartByteIndex { get; set; }
        [Parameter("bytes", "dataToSet", 5)]
        public virtual byte[] DataToSet { get; set; }
        [Parameter("bytes32", "fieldLayout", 6)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class HelloStoreEventDTO : HelloStoreEventDTOBase { }

    [Event("HelloStore")]
    public class HelloStoreEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "storeVersion", 1, true )]
        public virtual byte[] StoreVersion { get; set; }
    }

    public partial class StoreDeleteRecordEventDTO : StoreDeleteRecordEventDTOBase { }

    [Event("StoreDeleteRecord")]
    public class StoreDeleteRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "tableId", 1, true )]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2, false )]
        public virtual List<byte[]> KeyTuple { get; set; }
    }

    public partial class StoreSetRecordEventDTO : StoreSetRecordEventDTOBase { }

    [Event("StoreSetRecord")]
    public class StoreSetRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "tableId", 1, true )]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2, false )]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("bytes", "staticData", 3, false )]
        public virtual byte[] StaticData { get; set; }
        [Parameter("bytes32", "encodedLengths", 4, false )]
        public virtual byte[] EncodedLengths { get; set; }
        [Parameter("bytes", "dynamicData", 5, false )]
        public virtual byte[] DynamicData { get; set; }
    }

    public partial class StoreSpliceDynamicDataEventDTO : StoreSpliceDynamicDataEventDTOBase { }

    [Event("StoreSpliceDynamicData")]
    public class StoreSpliceDynamicDataEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "tableId", 1, true )]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2, false )]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint48", "start", 3, false )]
        public virtual ulong Start { get; set; }
        [Parameter("uint40", "deleteCount", 4, false )]
        public virtual ulong DeleteCount { get; set; }
        [Parameter("bytes", "data", 5, false )]
        public virtual byte[] Data { get; set; }
        [Parameter("bytes32", "encodedLengths", 6, false )]
        public virtual byte[] EncodedLengths { get; set; }
    }

    public partial class StoreSpliceStaticDataEventDTO : StoreSpliceStaticDataEventDTOBase { }

    [Event("StoreSpliceStaticData")]
    public class StoreSpliceStaticDataEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "tableId", 1, true )]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2, false )]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint48", "start", 3, false )]
        public virtual ulong Start { get; set; }
        [Parameter("uint40", "deleteCount", 4, false )]
        public virtual ulong DeleteCount { get; set; }
        [Parameter("bytes", "data", 5, false )]
        public virtual byte[] Data { get; set; }
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

    public partial class StorecoreInvaliddynamicdatalengthError : StorecoreInvaliddynamicdatalengthErrorBase { }

    [Error("StoreCore_InvalidDynamicDataLength")]
    public class StorecoreInvaliddynamicdatalengthErrorBase : IErrorDTO
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

    public partial class StorecoreInvalidkeynameslengthError : StorecoreInvalidkeynameslengthErrorBase { }

    [Error("StoreCore_InvalidKeyNamesLength")]
    public class StorecoreInvalidkeynameslengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StorecoreInvalidresourcetypeError : StorecoreInvalidresourcetypeErrorBase { }

    [Error("StoreCore_InvalidResourceType")]
    public class StorecoreInvalidresourcetypeErrorBase : IErrorDTO
    {
        [Parameter("bytes2", "expected", 1)]
        public virtual byte[] Expected { get; set; }
        [Parameter("bytes32", "resourceId", 2)]
        public virtual byte[] ResourceId { get; set; }
        [Parameter("string", "resourceIdString", 3)]
        public virtual string ResourceIdString { get; set; }
    }

    public partial class StorecoreInvalidspliceError : StorecoreInvalidspliceErrorBase { }

    [Error("StoreCore_InvalidSplice")]
    public class StorecoreInvalidspliceErrorBase : IErrorDTO
    {
        [Parameter("uint40", "startWithinField", 1)]
        public virtual ulong StartWithinField { get; set; }
        [Parameter("uint40", "deleteCount", 2)]
        public virtual ulong DeleteCount { get; set; }
        [Parameter("uint40", "fieldLength", 3)]
        public virtual ulong FieldLength { get; set; }
    }

    public partial class StorecoreInvalidstaticdatalengthError : StorecoreInvalidstaticdatalengthErrorBase { }

    [Error("StoreCore_InvalidStaticDataLength")]
    public class StorecoreInvalidstaticdatalengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StorecoreInvalidvalueschemalengthError : StorecoreInvalidvalueschemalengthErrorBase { }

    [Error("StoreCore_InvalidValueSchemaLength")]
    public class StorecoreInvalidvalueschemalengthErrorBase : IErrorDTO
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



    public partial class GetDynamicFieldOutputDTO : GetDynamicFieldOutputDTOBase { }

    [FunctionOutput]
    public class GetDynamicFieldOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public partial class GetFieldOutputDTO : GetFieldOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetFieldLayoutOutputDTO : GetFieldLayoutOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldLayoutOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "fieldLayout", 1)]
        public virtual byte[] FieldLayout { get; set; }
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
        [Parameter("bytes32", "keySchema", 1)]
        public virtual byte[] KeySchema { get; set; }
    }

    public partial class GetRecordOutputDTO : GetRecordOutputDTOBase { }

    [FunctionOutput]
    public class GetRecordOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "staticData", 1)]
        public virtual byte[] StaticData { get; set; }
        [Parameter("bytes32", "encodedLengths", 2)]
        public virtual byte[] EncodedLengths { get; set; }
        [Parameter("bytes", "dynamicData", 3)]
        public virtual byte[] DynamicData { get; set; }
    }

    public partial class GetStaticFieldOutputDTO : GetStaticFieldOutputDTOBase { }

    [FunctionOutput]
    public class GetStaticFieldOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public partial class GetValueSchemaOutputDTO : GetValueSchemaOutputDTOBase { }

    [FunctionOutput]
    public class GetValueSchemaOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "valueSchema", 1)]
        public virtual byte[] ValueSchema { get; set; }
    }

















    public partial class StoreVersionOutputDTO : StoreVersionOutputDTOBase { }

    [FunctionOutput]
    public class StoreVersionOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }




}
