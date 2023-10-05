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

namespace mud.IStore.ContractDefinition
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

    public partial class GetDynamicFieldLengthFunction : GetDynamicFieldLengthFunctionBase { }

    [Function("getDynamicFieldLength", "uint256")]
    public class GetDynamicFieldLengthFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
    }

    public partial class GetDynamicFieldSliceFunction : GetDynamicFieldSliceFunctionBase { }

    [Function("getDynamicFieldSlice", "bytes")]
    public class GetDynamicFieldSliceFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
        [Parameter("uint256", "start", 4)]
        public virtual BigInteger Start { get; set; }
        [Parameter("uint256", "end", 5)]
        public virtual BigInteger End { get; set; }
    }

    public partial class GetField1Function : GetField1FunctionBase { }

    [Function("getField", "bytes")]
    public class GetField1FunctionBase : FunctionMessage
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
    }

    public partial class GetFieldLayoutFunction : GetFieldLayoutFunctionBase { }

    [Function("getFieldLayout", "bytes32")]
    public class GetFieldLayoutFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
    }

    public partial class GetFieldLength1Function : GetFieldLength1FunctionBase { }

    [Function("getFieldLength", "uint256")]
    public class GetFieldLength1FunctionBase : FunctionMessage
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
    }

    public partial class GetKeySchemaFunction : GetKeySchemaFunctionBase { }

    [Function("getKeySchema", "bytes32")]
    public class GetKeySchemaFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
    }

    public partial class GetRecord1Function : GetRecord1FunctionBase { }

    [Function("getRecord", typeof(GetRecord1OutputDTO))]
    public class GetRecord1FunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("bytes32", "fieldLayout", 3)]
        public virtual byte[] FieldLayout { get; set; }
    }

    public partial class GetRecordFunction : GetRecordFunctionBase { }

    [Function("getRecord", typeof(GetRecordOutputDTO))]
    public class GetRecordFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
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

    public partial class PopFromDynamicFieldFunction : PopFromDynamicFieldFunctionBase { }

    [Function("popFromDynamicField")]
    public class PopFromDynamicFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
        [Parameter("uint256", "byteLengthToPop", 4)]
        public virtual BigInteger ByteLengthToPop { get; set; }
    }

    public partial class PushToDynamicFieldFunction : PushToDynamicFieldFunctionBase { }

    [Function("pushToDynamicField")]
    public class PushToDynamicFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
        [Parameter("bytes", "dataToPush", 4)]
        public virtual byte[] DataToPush { get; set; }
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

    public partial class SetDynamicFieldFunction : SetDynamicFieldFunctionBase { }

    [Function("setDynamicField")]
    public class SetDynamicFieldFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2)]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint8", "dynamicFieldIndex", 3)]
        public virtual byte DynamicFieldIndex { get; set; }
        [Parameter("bytes", "data", 4)]
        public virtual byte[] Data { get; set; }
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
    }

    public partial class SetField1Function : SetField1FunctionBase { }

    [Function("setField")]
    public class SetField1FunctionBase : FunctionMessage
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
    }

    public partial class SetStaticFieldFunction : SetStaticFieldFunctionBase { }

    [Function("setStaticField")]
    public class SetStaticFieldFunctionBase : FunctionMessage
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
        [Parameter("bytes", "data", 4)]
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

    public partial class HelloStoreEventDTO : HelloStoreEventDTOBase { }

    [Event("HelloStore")]
    public class HelloStoreEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "storeVersion", 1, true )]
        public virtual byte[] StoreVersion { get; set; }
    }

    public partial class StoreDeleteRecordEventDTO : StoreDeleteRecordEventDTOBase { }

    [Event("Store_DeleteRecord")]
    public class StoreDeleteRecordEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "tableId", 1, true )]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2, false )]
        public virtual List<byte[]> KeyTuple { get; set; }
    }

    public partial class StoreSetRecordEventDTO : StoreSetRecordEventDTOBase { }

    [Event("Store_SetRecord")]
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

    [Event("Store_SpliceDynamicData")]
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
        [Parameter("bytes32", "encodedLengths", 5, false )]
        public virtual byte[] EncodedLengths { get; set; }
        [Parameter("bytes", "data", 6, false )]
        public virtual byte[] Data { get; set; }
    }

    public partial class StoreSpliceStaticDataEventDTO : StoreSpliceStaticDataEventDTOBase { }

    [Event("Store_SpliceStaticData")]
    public class StoreSpliceStaticDataEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "tableId", 1, true )]
        public virtual byte[] TableId { get; set; }
        [Parameter("bytes32[]", "keyTuple", 2, false )]
        public virtual List<byte[]> KeyTuple { get; set; }
        [Parameter("uint48", "start", 3, false )]
        public virtual ulong Start { get; set; }
        [Parameter("bytes", "data", 4, false )]
        public virtual byte[] Data { get; set; }
    }

    public partial class StoreIndexOutOfBoundsError : StoreIndexOutOfBoundsErrorBase { }

    [Error("Store_IndexOutOfBounds")]
    public class StoreIndexOutOfBoundsErrorBase : IErrorDTO
    {
        [Parameter("uint256", "length", 1)]
        public virtual BigInteger Length { get; set; }
        [Parameter("uint256", "accessedIndex", 2)]
        public virtual BigInteger AccessedIndex { get; set; }
    }

    public partial class StoreInvalidDynamicDataLengthError : StoreInvalidDynamicDataLengthErrorBase { }

    [Error("Store_InvalidDynamicDataLength")]
    public class StoreInvalidDynamicDataLengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StoreInvalidFieldNamesLengthError : StoreInvalidFieldNamesLengthErrorBase { }

    [Error("Store_InvalidFieldNamesLength")]
    public class StoreInvalidFieldNamesLengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StoreInvalidKeyNamesLengthError : StoreInvalidKeyNamesLengthErrorBase { }

    [Error("Store_InvalidKeyNamesLength")]
    public class StoreInvalidKeyNamesLengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StoreInvalidResourceTypeError : StoreInvalidResourceTypeErrorBase { }

    [Error("Store_InvalidResourceType")]
    public class StoreInvalidResourceTypeErrorBase : IErrorDTO
    {
        [Parameter("bytes2", "expected", 1)]
        public virtual byte[] Expected { get; set; }
        [Parameter("bytes32", "resourceId", 2)]
        public virtual byte[] ResourceId { get; set; }
        [Parameter("string", "resourceIdString", 3)]
        public virtual string ResourceIdString { get; set; }
    }

    public partial class StoreInvalidSpliceError : StoreInvalidSpliceErrorBase { }

    [Error("Store_InvalidSplice")]
    public class StoreInvalidSpliceErrorBase : IErrorDTO
    {
        [Parameter("uint40", "startWithinField", 1)]
        public virtual ulong StartWithinField { get; set; }
        [Parameter("uint40", "deleteCount", 2)]
        public virtual ulong DeleteCount { get; set; }
        [Parameter("uint40", "fieldLength", 3)]
        public virtual ulong FieldLength { get; set; }
    }

    public partial class StoreInvalidValueSchemaLengthError : StoreInvalidValueSchemaLengthErrorBase { }

    [Error("Store_InvalidValueSchemaLength")]
    public class StoreInvalidValueSchemaLengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "expected", 1)]
        public virtual BigInteger Expected { get; set; }
        [Parameter("uint256", "received", 2)]
        public virtual BigInteger Received { get; set; }
    }

    public partial class StoreTableAlreadyExistsError : StoreTableAlreadyExistsErrorBase { }

    [Error("Store_TableAlreadyExists")]
    public class StoreTableAlreadyExistsErrorBase : IErrorDTO
    {
        [Parameter("bytes32", "tableId", 1)]
        public virtual byte[] TableId { get; set; }
        [Parameter("string", "tableIdString", 2)]
        public virtual string TableIdString { get; set; }
    }

    public partial class StoreTableNotFoundError : StoreTableNotFoundErrorBase { }

    [Error("Store_TableNotFound")]
    public class StoreTableNotFoundErrorBase : IErrorDTO
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

    public partial class GetDynamicFieldLengthOutputDTO : GetDynamicFieldLengthOutputDTOBase { }

    [FunctionOutput]
    public class GetDynamicFieldLengthOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetDynamicFieldSliceOutputDTO : GetDynamicFieldSliceOutputDTOBase { }

    [FunctionOutput]
    public class GetDynamicFieldSliceOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
    }

    public partial class GetField1OutputDTO : GetField1OutputDTOBase { }

    [FunctionOutput]
    public class GetField1OutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "data", 1)]
        public virtual byte[] Data { get; set; }
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

    public partial class GetFieldLength1OutputDTO : GetFieldLength1OutputDTOBase { }

    [FunctionOutput]
    public class GetFieldLength1OutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetFieldLengthOutputDTO : GetFieldLengthOutputDTOBase { }

    [FunctionOutput]
    public class GetFieldLengthOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetKeySchemaOutputDTO : GetKeySchemaOutputDTOBase { }

    [FunctionOutput]
    public class GetKeySchemaOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "keySchema", 1)]
        public virtual byte[] KeySchema { get; set; }
    }

    public partial class GetRecord1OutputDTO : GetRecord1OutputDTOBase { }

    [FunctionOutput]
    public class GetRecord1OutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes", "staticData", 1)]
        public virtual byte[] StaticData { get; set; }
        [Parameter("bytes32", "encodedLengths", 2)]
        public virtual byte[] EncodedLengths { get; set; }
        [Parameter("bytes", "dynamicData", 3)]
        public virtual byte[] DynamicData { get; set; }
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
