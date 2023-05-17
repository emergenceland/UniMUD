using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using mud.Network.IStore.ContractDefinition;

namespace mud.Network.IStore
{
    public partial class IStoreService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, IStoreDeployment iStoreDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<IStoreDeployment>().SendRequestAndWaitForReceiptAsync(iStoreDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, IStoreDeployment iStoreDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<IStoreDeployment>().SendRequestAsync(iStoreDeployment);
        }

        public static async Task<IStoreService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, IStoreDeployment iStoreDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, iStoreDeployment, cancellationTokenSource);
            return new IStoreService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public IStoreService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public IStoreService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<string> DeleteRecordRequestAsync(DeleteRecordFunction deleteRecordFunction)
        {
             return ContractHandler.SendRequestAsync(deleteRecordFunction);
        }

        public Task<TransactionReceipt> DeleteRecordRequestAndWaitForReceiptAsync(DeleteRecordFunction deleteRecordFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(deleteRecordFunction, cancellationToken);
        }

        public Task<string> DeleteRecordRequestAsync(byte[] table, List<byte[]> key)
        {
            var deleteRecordFunction = new DeleteRecordFunction();
                deleteRecordFunction.Table = table;
                deleteRecordFunction.Key = key;
            
             return ContractHandler.SendRequestAsync(deleteRecordFunction);
        }

        public Task<TransactionReceipt> DeleteRecordRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, CancellationTokenSource cancellationToken = null)
        {
            var deleteRecordFunction = new DeleteRecordFunction();
                deleteRecordFunction.Table = table;
                deleteRecordFunction.Key = key;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(deleteRecordFunction, cancellationToken);
        }

        public Task<string> EmitEphemeralRecordRequestAsync(EmitEphemeralRecordFunction emitEphemeralRecordFunction)
        {
             return ContractHandler.SendRequestAsync(emitEphemeralRecordFunction);
        }

        public Task<TransactionReceipt> EmitEphemeralRecordRequestAndWaitForReceiptAsync(EmitEphemeralRecordFunction emitEphemeralRecordFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(emitEphemeralRecordFunction, cancellationToken);
        }

        public Task<string> EmitEphemeralRecordRequestAsync(byte[] table, List<byte[]> key, byte[] data)
        {
            var emitEphemeralRecordFunction = new EmitEphemeralRecordFunction();
                emitEphemeralRecordFunction.Table = table;
                emitEphemeralRecordFunction.Key = key;
                emitEphemeralRecordFunction.Data = data;
            
             return ContractHandler.SendRequestAsync(emitEphemeralRecordFunction);
        }

        public Task<TransactionReceipt> EmitEphemeralRecordRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var emitEphemeralRecordFunction = new EmitEphemeralRecordFunction();
                emitEphemeralRecordFunction.Table = table;
                emitEphemeralRecordFunction.Key = key;
                emitEphemeralRecordFunction.Data = data;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(emitEphemeralRecordFunction, cancellationToken);
        }

        public Task<byte[]> GetFieldQueryAsync(GetFieldFunction getFieldFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetFieldFunction, byte[]>(getFieldFunction, blockParameter);
        }

        
        public Task<byte[]> GetFieldQueryAsync(byte[] table, List<byte[]> key, byte schemaIndex, BlockParameter blockParameter = null)
        {
            var getFieldFunction = new GetFieldFunction();
                getFieldFunction.Table = table;
                getFieldFunction.Key = key;
                getFieldFunction.SchemaIndex = schemaIndex;
            
            return ContractHandler.QueryAsync<GetFieldFunction, byte[]>(getFieldFunction, blockParameter);
        }

        public Task<BigInteger> GetFieldLengthQueryAsync(GetFieldLengthFunction getFieldLengthFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetFieldLengthFunction, BigInteger>(getFieldLengthFunction, blockParameter);
        }

        
        public Task<BigInteger> GetFieldLengthQueryAsync(byte[] table, List<byte[]> key, byte schemaIndex, byte[] schema, BlockParameter blockParameter = null)
        {
            var getFieldLengthFunction = new GetFieldLengthFunction();
                getFieldLengthFunction.Table = table;
                getFieldLengthFunction.Key = key;
                getFieldLengthFunction.SchemaIndex = schemaIndex;
                getFieldLengthFunction.Schema = schema;
            
            return ContractHandler.QueryAsync<GetFieldLengthFunction, BigInteger>(getFieldLengthFunction, blockParameter);
        }

        public Task<byte[]> GetFieldSliceQueryAsync(GetFieldSliceFunction getFieldSliceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetFieldSliceFunction, byte[]>(getFieldSliceFunction, blockParameter);
        }

        
        public Task<byte[]> GetFieldSliceQueryAsync(byte[] table, List<byte[]> key, byte schemaIndex, byte[] schema, BigInteger start, BigInteger end, BlockParameter blockParameter = null)
        {
            var getFieldSliceFunction = new GetFieldSliceFunction();
                getFieldSliceFunction.Table = table;
                getFieldSliceFunction.Key = key;
                getFieldSliceFunction.SchemaIndex = schemaIndex;
                getFieldSliceFunction.Schema = schema;
                getFieldSliceFunction.Start = start;
                getFieldSliceFunction.End = end;
            
            return ContractHandler.QueryAsync<GetFieldSliceFunction, byte[]>(getFieldSliceFunction, blockParameter);
        }

        public Task<byte[]> GetKeySchemaQueryAsync(GetKeySchemaFunction getKeySchemaFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetKeySchemaFunction, byte[]>(getKeySchemaFunction, blockParameter);
        }

        
        public Task<byte[]> GetKeySchemaQueryAsync(byte[] table, BlockParameter blockParameter = null)
        {
            var getKeySchemaFunction = new GetKeySchemaFunction();
                getKeySchemaFunction.Table = table;
            
            return ContractHandler.QueryAsync<GetKeySchemaFunction, byte[]>(getKeySchemaFunction, blockParameter);
        }

        public Task<byte[]> GetRecordQueryAsync(GetRecord1Function getRecord1Function, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRecord1Function, byte[]>(getRecord1Function, blockParameter);
        }

        
        public Task<byte[]> GetRecordQueryAsync(byte[] table, List<byte[]> key, byte[] schema, BlockParameter blockParameter = null)
        {
            var getRecord1Function = new GetRecord1Function();
                getRecord1Function.Table = table;
                getRecord1Function.Key = key;
                getRecord1Function.Schema = schema;
            
            return ContractHandler.QueryAsync<GetRecord1Function, byte[]>(getRecord1Function, blockParameter);
        }

        public Task<byte[]> GetRecordQueryAsync(GetRecordFunction getRecordFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRecordFunction, byte[]>(getRecordFunction, blockParameter);
        }

        
        public Task<byte[]> GetRecordQueryAsync(byte[] table, List<byte[]> key, BlockParameter blockParameter = null)
        {
            var getRecordFunction = new GetRecordFunction();
                getRecordFunction.Table = table;
                getRecordFunction.Key = key;
            
            return ContractHandler.QueryAsync<GetRecordFunction, byte[]>(getRecordFunction, blockParameter);
        }

        public Task<byte[]> GetSchemaQueryAsync(GetSchemaFunction getSchemaFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSchemaFunction, byte[]>(getSchemaFunction, blockParameter);
        }

        
        public Task<byte[]> GetSchemaQueryAsync(byte[] table, BlockParameter blockParameter = null)
        {
            var getSchemaFunction = new GetSchemaFunction();
                getSchemaFunction.Table = table;
            
            return ContractHandler.QueryAsync<GetSchemaFunction, byte[]>(getSchemaFunction, blockParameter);
        }



        public Task<string> PopFromFieldRequestAsync(PopFromFieldFunction popFromFieldFunction)
        {
             return ContractHandler.SendRequestAsync(popFromFieldFunction);
        }

        public Task<TransactionReceipt> PopFromFieldRequestAndWaitForReceiptAsync(PopFromFieldFunction popFromFieldFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(popFromFieldFunction, cancellationToken);
        }

        public Task<string> PopFromFieldRequestAsync(byte[] table, List<byte[]> key, byte schemaIndex, BigInteger byteLengthToPop)
        {
            var popFromFieldFunction = new PopFromFieldFunction();
                popFromFieldFunction.Table = table;
                popFromFieldFunction.Key = key;
                popFromFieldFunction.SchemaIndex = schemaIndex;
                popFromFieldFunction.ByteLengthToPop = byteLengthToPop;
            
             return ContractHandler.SendRequestAsync(popFromFieldFunction);
        }

        public Task<TransactionReceipt> PopFromFieldRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, byte schemaIndex, BigInteger byteLengthToPop, CancellationTokenSource cancellationToken = null)
        {
            var popFromFieldFunction = new PopFromFieldFunction();
                popFromFieldFunction.Table = table;
                popFromFieldFunction.Key = key;
                popFromFieldFunction.SchemaIndex = schemaIndex;
                popFromFieldFunction.ByteLengthToPop = byteLengthToPop;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(popFromFieldFunction, cancellationToken);
        }

        public Task<string> PushToFieldRequestAsync(PushToFieldFunction pushToFieldFunction)
        {
             return ContractHandler.SendRequestAsync(pushToFieldFunction);
        }

        public Task<TransactionReceipt> PushToFieldRequestAndWaitForReceiptAsync(PushToFieldFunction pushToFieldFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(pushToFieldFunction, cancellationToken);
        }

        public Task<string> PushToFieldRequestAsync(byte[] table, List<byte[]> key, byte schemaIndex, byte[] dataToPush)
        {
            var pushToFieldFunction = new PushToFieldFunction();
                pushToFieldFunction.Table = table;
                pushToFieldFunction.Key = key;
                pushToFieldFunction.SchemaIndex = schemaIndex;
                pushToFieldFunction.DataToPush = dataToPush;
            
             return ContractHandler.SendRequestAsync(pushToFieldFunction);
        }

        public Task<TransactionReceipt> PushToFieldRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, byte schemaIndex, byte[] dataToPush, CancellationTokenSource cancellationToken = null)
        {
            var pushToFieldFunction = new PushToFieldFunction();
                pushToFieldFunction.Table = table;
                pushToFieldFunction.Key = key;
                pushToFieldFunction.SchemaIndex = schemaIndex;
                pushToFieldFunction.DataToPush = dataToPush;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(pushToFieldFunction, cancellationToken);
        }

        public Task<string> RegisterSchemaRequestAsync(RegisterSchemaFunction registerSchemaFunction)
        {
             return ContractHandler.SendRequestAsync(registerSchemaFunction);
        }

        public Task<TransactionReceipt> RegisterSchemaRequestAndWaitForReceiptAsync(RegisterSchemaFunction registerSchemaFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registerSchemaFunction, cancellationToken);
        }

        public Task<string> RegisterSchemaRequestAsync(byte[] table, byte[] schema, byte[] keySchema)
        {
            var registerSchemaFunction = new RegisterSchemaFunction();
                registerSchemaFunction.Table = table;
                registerSchemaFunction.Schema = schema;
                registerSchemaFunction.KeySchema = keySchema;
            
             return ContractHandler.SendRequestAsync(registerSchemaFunction);
        }

        public Task<TransactionReceipt> RegisterSchemaRequestAndWaitForReceiptAsync(byte[] table, byte[] schema, byte[] keySchema, CancellationTokenSource cancellationToken = null)
        {
            var registerSchemaFunction = new RegisterSchemaFunction();
                registerSchemaFunction.Table = table;
                registerSchemaFunction.Schema = schema;
                registerSchemaFunction.KeySchema = keySchema;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registerSchemaFunction, cancellationToken);
        }

        public Task<string> RegisterStoreHookRequestAsync(RegisterStoreHookFunction registerStoreHookFunction)
        {
             return ContractHandler.SendRequestAsync(registerStoreHookFunction);
        }

        public Task<TransactionReceipt> RegisterStoreHookRequestAndWaitForReceiptAsync(RegisterStoreHookFunction registerStoreHookFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registerStoreHookFunction, cancellationToken);
        }

        public Task<string> RegisterStoreHookRequestAsync(byte[] table, string hook)
        {
            var registerStoreHookFunction = new RegisterStoreHookFunction();
                registerStoreHookFunction.Table = table;
                registerStoreHookFunction.Hook = hook;
            
             return ContractHandler.SendRequestAsync(registerStoreHookFunction);
        }

        public Task<TransactionReceipt> RegisterStoreHookRequestAndWaitForReceiptAsync(byte[] table, string hook, CancellationTokenSource cancellationToken = null)
        {
            var registerStoreHookFunction = new RegisterStoreHookFunction();
                registerStoreHookFunction.Table = table;
                registerStoreHookFunction.Hook = hook;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registerStoreHookFunction, cancellationToken);
        }

        public Task<string> SetFieldRequestAsync(SetFieldFunction setFieldFunction)
        {
             return ContractHandler.SendRequestAsync(setFieldFunction);
        }

        public Task<TransactionReceipt> SetFieldRequestAndWaitForReceiptAsync(SetFieldFunction setFieldFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setFieldFunction, cancellationToken);
        }

        public Task<string> SetFieldRequestAsync(byte[] table, List<byte[]> key, byte schemaIndex, byte[] data)
        {
            var setFieldFunction = new SetFieldFunction();
                setFieldFunction.Table = table;
                setFieldFunction.Key = key;
                setFieldFunction.SchemaIndex = schemaIndex;
                setFieldFunction.Data = data;
            
             return ContractHandler.SendRequestAsync(setFieldFunction);
        }

        public Task<TransactionReceipt> SetFieldRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, byte schemaIndex, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var setFieldFunction = new SetFieldFunction();
                setFieldFunction.Table = table;
                setFieldFunction.Key = key;
                setFieldFunction.SchemaIndex = schemaIndex;
                setFieldFunction.Data = data;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setFieldFunction, cancellationToken);
        }

        public Task<string> SetMetadataRequestAsync(SetMetadataFunction setMetadataFunction)
        {
             return ContractHandler.SendRequestAsync(setMetadataFunction);
        }

        public Task<TransactionReceipt> SetMetadataRequestAndWaitForReceiptAsync(SetMetadataFunction setMetadataFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setMetadataFunction, cancellationToken);
        }

        public Task<string> SetMetadataRequestAsync(byte[] table, string tableName, List<string> fieldNames)
        {
            var setMetadataFunction = new SetMetadataFunction();
                setMetadataFunction.Table = table;
                setMetadataFunction.TableName = tableName;
                setMetadataFunction.FieldNames = fieldNames;
            
             return ContractHandler.SendRequestAsync(setMetadataFunction);
        }

        public Task<TransactionReceipt> SetMetadataRequestAndWaitForReceiptAsync(byte[] table, string tableName, List<string> fieldNames, CancellationTokenSource cancellationToken = null)
        {
            var setMetadataFunction = new SetMetadataFunction();
                setMetadataFunction.Table = table;
                setMetadataFunction.TableName = tableName;
                setMetadataFunction.FieldNames = fieldNames;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setMetadataFunction, cancellationToken);
        }

        public Task<string> SetRecordRequestAsync(SetRecordFunction setRecordFunction)
        {
             return ContractHandler.SendRequestAsync(setRecordFunction);
        }

        public Task<TransactionReceipt> SetRecordRequestAndWaitForReceiptAsync(SetRecordFunction setRecordFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setRecordFunction, cancellationToken);
        }

        public Task<string> SetRecordRequestAsync(byte[] table, List<byte[]> key, byte[] data)
        {
            var setRecordFunction = new SetRecordFunction();
                setRecordFunction.Table = table;
                setRecordFunction.Key = key;
                setRecordFunction.Data = data;
            
             return ContractHandler.SendRequestAsync(setRecordFunction);
        }

        public Task<TransactionReceipt> SetRecordRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var setRecordFunction = new SetRecordFunction();
                setRecordFunction.Table = table;
                setRecordFunction.Key = key;
                setRecordFunction.Data = data;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setRecordFunction, cancellationToken);
        }

        public Task<string> UpdateInFieldRequestAsync(UpdateInFieldFunction updateInFieldFunction)
        {
             return ContractHandler.SendRequestAsync(updateInFieldFunction);
        }

        public Task<TransactionReceipt> UpdateInFieldRequestAndWaitForReceiptAsync(UpdateInFieldFunction updateInFieldFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(updateInFieldFunction, cancellationToken);
        }

        public Task<string> UpdateInFieldRequestAsync(byte[] table, List<byte[]> key, byte schemaIndex, BigInteger startByteIndex, byte[] dataToSet)
        {
            var updateInFieldFunction = new UpdateInFieldFunction();
                updateInFieldFunction.Table = table;
                updateInFieldFunction.Key = key;
                updateInFieldFunction.SchemaIndex = schemaIndex;
                updateInFieldFunction.StartByteIndex = startByteIndex;
                updateInFieldFunction.DataToSet = dataToSet;
            
             return ContractHandler.SendRequestAsync(updateInFieldFunction);
        }

        public Task<TransactionReceipt> UpdateInFieldRequestAndWaitForReceiptAsync(byte[] table, List<byte[]> key, byte schemaIndex, BigInteger startByteIndex, byte[] dataToSet, CancellationTokenSource cancellationToken = null)
        {
            var updateInFieldFunction = new UpdateInFieldFunction();
                updateInFieldFunction.Table = table;
                updateInFieldFunction.Key = key;
                updateInFieldFunction.SchemaIndex = schemaIndex;
                updateInFieldFunction.StartByteIndex = startByteIndex;
                updateInFieldFunction.DataToSet = dataToSet;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(updateInFieldFunction, cancellationToken);
        }
    }
}
