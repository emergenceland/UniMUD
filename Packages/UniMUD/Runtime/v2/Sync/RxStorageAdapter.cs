using System;
using System.Collections.Generic;
using System.Linq;
using mud.Client;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using UnityEngine;
using v2.IStore.ContractDefinition;
using static v2.ProtocolParser;

namespace v2
{
    public class RxStorageAdapter
    {
        private static DecodedEventUnion DecodeEvent(FilterLog log)
        {
            if (log.IsLogForEvent(new StoreSetRecordEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log);
                return new DecodedSetRecord(decoded.Event);
            }

            if (log.IsLogForEvent(new StoreSpliceStaticDataEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreSpliceStaticDataEventDTO>.DecodeEvent(log);
                return new DecodedSpliceStatic(decoded.Event);
            }

            if (log.IsLogForEvent(new StoreSpliceDynamicDataEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreSpliceDynamicDataEventDTO>.DecodeEvent(log);
                return new DecodedSpliceDynamic(decoded.Event);
            }

            if (log.IsLogForEvent(new StoreDeleteRecordEventDTO().GetEventABI().Sha3Signature))
            {
                var decoded = Event<StoreDeleteRecordEventDTO>.DecodeEvent(log);
                return new DecodedDeleteRecord(decoded.Event);
            }

            throw new Exception("Unknown log type");
        }

        public static void ToStorage(RxDatastore ds, StorageAdapterBlock logs)
        {
            var newTables = logs.Logs.Where(IsTableRegistrationLog).Select(LogToTable);
            foreach (var newTable in newTables)
            {
                var tableEntity = mud.Client.Common.GetTableKey(newTable);
                if (ds.registeredTables.ContainsKey(tableEntity))
                {
                    Debug.LogWarning($"Skipping registration for already registered table: {tableEntity}");
                }
                else
                {
                    ds.RegisterTable(newTable);
                }
            }

            foreach (var log in logs.Logs)
            {
                var decoded = DecodeEvent(log);

                decoded.Match(
                    setRecord => HandleStoreSetRecord(ds, log, setRecord),
                    spliceStatic => HandleStoreSpliceStatic(ds, log, spliceStatic),
                    spliceDynamic => HandleStoreSpliceDynamic(ds, log, spliceDynamic),
                    deleteRecord => HandleStoreDeleteRecord(ds, log, deleteRecord)
                );
            }
        }

        private static void HandleStoreSetRecord(RxDatastore ds, FilterLog log, StoreSetRecordEventDTO decoded)
        {
            var tableResource = Common.HexToResourceId(Common.BytesToHex(decoded.TableId));
            var tableKey = mud.Client.Common.GetTableKey(log.Address, tableResource.Namespace, tableResource.Name);
            if (!ds.registeredTables.TryGetValue(tableKey, out var table))
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {table.Namespace}:{table.Name} at {table.Address}");
                return;
            }

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());
            var staticData = Common.BytesToHex(decoded.StaticData);
            var encodedLengths = Common.BytesToHex(decoded.EncodedLengths);
            var dynamicData = Common.BytesToHex(decoded.DynamicData);
            var value = DecodeValueArgs(table.ValueSchema, staticData, encodedLengths, dynamicData);

            Debug.Log(
                $"Setting table: {table.Namespace}:{table.Name}, Key: {entity}, Value: {JsonConvert.SerializeObject(value)}");
            // ds.Set(table, entity, new Dictionary<string, object>(value)
            // {
            //     ["__staticData"] = staticData,
            //     ["__encodedLengths"] = encodedLengths,
            //     ["__dynamicData"] = dynamicData
            // });
        }

        private static void HandleStoreSpliceStatic(RxDatastore ds, FilterLog log,
            StoreSpliceStaticDataEventDTO decoded)
        {
            var tableResource = Common.HexToResourceId(Common.BytesToHex(decoded.TableId));
            var tableKey = mud.Client.Common.GetTableKey(log.Address, tableResource.Namespace, tableResource.Name);

            if (!ds.registeredTables.TryGetValue(tableKey, out var table))
            {
                Debug.LogWarning("Skipping update for unknown table: " + tableKey);
                return;
            }

            var start = (int)decoded.Start;
            var deleteCount = (int)decoded.DeleteCount;
            var data = Common.BytesToHex(decoded.Data);

            // var previousValue = ds.GetValue(table, tableKey);
            var previousValue = new RxRecord
            (
                tableKey,
                tableKey,
                new Dictionary<string, object>()
                {
                    ["__staticData"] = "0x",
                    ["__encodedLengths"] = "0x",
                    ["__dynamicData"] = "0x"
                }
            );
            var previousStaticData = (string)previousValue?.value["__staticData"] ?? "0x";
            ;
            var newStaticData = Common.SpliceHex(previousStaticData, start, deleteCount, data);
            var newValue = DecodeValueArgs(
                table.ValueSchema,
                newStaticData,
                (string)previousValue?.value["__encodedLengths"] ?? "0x",
                (string)previousValue?.value["__dynamicData"] ?? "0x");

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            Debug.Log(
                $"Setting table via splice static: {table.Namespace}:{table.Name}");
            Debug.Log(
                $"Key: {entity}, {previousStaticData}, {newStaticData}, {JsonConvert.SerializeObject(previousValue)}, {JsonConvert.SerializeObject(newValue)}");
            // ds.Update(table, entity, new Dictionary<string, object>(newValue)
            // {
            //     ["__StaticData"] = newStaticData
            // });
        }

        private static void HandleStoreSpliceDynamic(RxDatastore ds, FilterLog log,
            StoreSpliceDynamicDataEventDTO decoded)
        {
            var tableResource = Common.HexToResourceId(Common.BytesToHex(decoded.TableId));
            var tableKey = mud.Client.Common.GetTableKey(log.Address, tableResource.Namespace, tableResource.Name);

            if (!ds.registeredTables.TryGetValue(tableKey, out var table))
            {
                Debug.LogWarning("Skipping update for unknown table: " + tableKey);
                return;
            }

            var start = decoded.Start;
            var deleteCount = decoded.DeleteCount;
            var data = Common.BytesToHex(decoded.Data);
            var encodedLengths = Common.BytesToHex(decoded.EncodedLengths);

            // var previousValue = ds.GetValue(table, tableKey);
            var previousValue = new RxRecord
            (
                tableKey,
                tableKey,
                new Dictionary<string, object>()
                {
                    ["__staticData"] = "0x",
                    ["__encodedLengths"] = "0x",
                    ["__dynamicData"] = "0x"
                }
            );
            var previousDynamicData = (string)previousValue?.value["__dynamicData"] ?? "0x";
            var newDynamicData = Common.SpliceHex(previousDynamicData, (int)start, (int)deleteCount, data);
            var newValue = DecodeValueArgs(
                table.ValueSchema,
                (string)previousValue?.value["__staticData"] ?? "0x",
                (string)previousValue?.value["__encodedLengths"] ?? "0x",
                newDynamicData);

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            Debug.Log(
                $"Setting table via splice dynamic: {table.Namespace}:{table.Name}, Key: {entity}, {previousDynamicData}, {newDynamicData}, {previousValue}, {newValue}");
            // ds.Update(table, entity, new Dictionary<string, object>(newValue)
            // {
            //     ["__encodedLengths"] = encodedLengths,
            //     ["__dynamicData"] = newDynamicData
            // });
        }

        private static void HandleStoreDeleteRecord(RxDatastore ds, FilterLog log, StoreDeleteRecordEventDTO decoded)
        {
            var tableResource = Common.HexToResourceId(Common.BytesToHex(decoded.TableId));
            var tableKey = mud.Client.Common.GetTableKey(log.Address, tableResource.Namespace, tableResource.Name);

            if (!ds.registeredTables.TryGetValue(tableKey, out var table))
            {
                Debug.LogWarning("Skipping update for unknown table: " + tableKey);
                return;
            }

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            Debug.Log(
                $"Deleting key for table: {table.Namespace}:{table.Name}");
            // ds.Delete(table, entity);
        }
    }
}
