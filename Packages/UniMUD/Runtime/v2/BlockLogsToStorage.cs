using Cysharp.Threading.Tasks;
using mud.Network.IStore.ContractDefinition;
using mud.Network.schemas;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using UnityEngine;

namespace v2
{
    public partial class Sync
    {
        public static async UniTask<mud.Network.Types.NetworkTableUpdate> BlockLogsToStorage(FilterLog log,
            string storeContractAddress,
            string account,
            bool lastEventInTx)
        {
            var eventSig = log.EventSignature().Replace("0x", "");
            var storeSetFieldSignature =
                new StoreSetFieldEventDTO().GetEventABI().Sha3Signature;
            var storeSetRecordSignature =
                new StoreSetRecordEventDTO().GetEventABI().Sha3Signature;
            var storeDeleteRecordSignature =
                new StoreDeleteRecordEventDTO().GetEventABI().Sha3Signature;
            var storeEphemeralRecordSignature =
                new StoreEphemeralRecordEventDTO().GetEventABI().Sha3Signature;

            var ecsEvent = new mud.Network.Types.NetworkTableUpdate
            {
                Type = mud.Network.Types.NetworkEvents.NetworkComponentUpdate,
                BlockNumber = log.BlockNumber.Value,
                TxHash = log.TransactionHash,
                LogIndex = log.LogIndex.Value,
                LastEventInTx = lastEventInTx
            };

            if (eventSig == storeSetRecordSignature)
            {
                var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log);
                var tableId = TableId.FromBytes32(decoded.Event.Table);
                var component = tableId.ToString();
                var entity = EntityIdUtil.KeyTupleToEntityID(decoded.Event.Key);
                var entityTuple = EntityIdUtil.BytesToStringArray(decoded.Event.Key);
                var data = mud.Network.schemas.Common.ByteArrayToHexString(decoded.Event.Data);

                var value = await DecodeStore.DecodeStoreSetRecord(storeContractAddress, account, tableId, entityTuple, data);
                Debug.Log($"StoreSetRecord: {tableId}, {component}, {entity}");

                ecsEvent.Component = component;
                ecsEvent.Entity = new mud.Network.Types.EntityID { Value = entity };
                ecsEvent.Value = value;

                return ecsEvent;
            }

            if (eventSig == storeEphemeralRecordSignature)
            {
                var decoded = Event<StoreEphemeralRecordEventDTO>.DecodeEvent(log);
                var tableId = TableId.FromBytes32(decoded.Event.Table);
                var component = tableId.ToString();
                var entity = EntityIdUtil.KeyTupleToEntityID(decoded.Event.Key);
                var entityTuple = EntityIdUtil.BytesToStringArray(decoded.Event.Key);
                var data = mud.Network.schemas.Common.ByteArrayToHexString(decoded.Event.Data);

                var value = await DecodeStore.DecodeStoreSetRecord(storeContractAddress, account, tableId, entityTuple, data);
                Debug.Log($"StoreEphemeralRecord: {tableId}, {component}, {entity}");

                ecsEvent.Component = component;
                ecsEvent.Entity = new mud.Network.Types.EntityID { Value = entity };
                ecsEvent.Value = value;
                ecsEvent.Ephemeral = true;
            }

            if (eventSig == storeSetFieldSignature)
            {
                var decoded = Event<StoreSetFieldEventDTO>.DecodeEvent(log);
                var tableId = TableId.FromBytes32(decoded.Event.Table);
                var component = tableId.ToString();
                var entity = EntityIdUtil.KeyTupleToEntityID(decoded.Event.Key);
                var data = mud.Network.schemas.Common.ByteArrayToHexString(decoded.Event.Data);

                var (schema, value, initialValue) = await DecodeStore.DecodeStoreSetField(storeContractAddress, account, tableId,
                    decoded.Event.SchemaIndex, data);

                Debug.Log($"StoreSetField: {tableId}, {component}, {entity}");

                ecsEvent.Component = component;
                ecsEvent.Entity = new mud.Network.Types.EntityID { Value = entity };
                ecsEvent.InitialValue = initialValue;
                ecsEvent.PartialValue = value;

                return ecsEvent;
            }

            if (eventSig == storeDeleteRecordSignature)
            {
                var decoded = Event<StoreDeleteRecordEventDTO>.DecodeEvent(log);
                var tableId = TableId.FromBytes32(decoded.Event.Table);
                var component = tableId.ToString();
                var entity = EntityIdUtil.KeyTupleToEntityID(decoded.Event.Key);

                ecsEvent.Component = component;
                ecsEvent.Entity = new mud.Network.Types.EntityID { Value = entity };
                ecsEvent.Value = null;
                Debug.Log($"StoreDeleteRecord: {tableId}, {component}, {entity}");
                return ecsEvent;
            }

            return null;
        }
    }
}
