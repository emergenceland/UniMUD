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
                var newRxTable = ds.CreateTable(newTable.Namespace, newTable.Name, newTable.ValueSchema);
                if (ds.registeredTables.Contains(newRxTable.Id))
                {
                    Debug.LogWarning($"Skipping registration for already registered table: {JsonConvert.SerializeObject(newTable)}");
                }
                else
                {
                    // var tableName = $"{newTable.Namespace}:{newTable.Name}";
                    // TODO: figure out what to do with namespaces
                    var tableName = $"{newTable.Name}";
                    ds.RegisterTable(newRxTable, tableName);
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
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);
            if (!ds.registeredTables.Contains(tableId))
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {JsonConvert.SerializeObject(tableResource)} at {log.Address}, {tableId}");
                return;
            }

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());
            var staticData = Common.BytesToHex(decoded.StaticData);
            var encodedLengths = Common.BytesToHex(decoded.EncodedLengths);
            var dynamicData = Common.BytesToHex(decoded.DynamicData);
            var hasTable = ds.store.TryGetValue(tableId, out var table);
            if (!hasTable) return;
            var value = DecodeValueArgs(table.Schema, staticData, encodedLengths, dynamicData);

            Debug.Log(
                $"Setting table: {tableResource.Namespace}:{tableResource.Name}, Value: {JsonConvert.SerializeObject(value)}");
            ds.Set(table, entity, new Dictionary<string, object>(value)
            {
                ["__staticData"] = staticData,
                ["__encodedLengths"] = encodedLengths,
                ["__dynamicData"] = dynamicData
            });
        }

        private static void HandleStoreSpliceStatic(RxDatastore ds, FilterLog log,
            StoreSpliceStaticDataEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);

            if (!ds.registeredTables.Contains(tableId))
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {JsonConvert.SerializeObject(tableResource)} at {log.Address}");
                return;
            }

            var hasTable = ds.store.TryGetValue(tableId, out var table);
            if (!hasTable) return;

            var start = (int)decoded.Start;
            var data = Common.BytesToHex(decoded.Data);
            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            var previousValue = ds.GetValue(table, entity);

            object previousStaticData;
            object previousEncodedLengths;
            object previousDynamicData;

            if (previousValue != null)
            {
                previousValue.value.TryGetValue("__staticData", out previousStaticData);
                previousValue.value.TryGetValue("__encodedLengths", out previousEncodedLengths);
                previousValue.value.TryGetValue("__dynamicData", out previousDynamicData);
            }
            else
            {
                previousStaticData = "0x";
                previousEncodedLengths = "0x";
                previousDynamicData = "0x";
            }

            previousStaticData ??= "0x";
            previousEncodedLengths ??= "0x";
            previousDynamicData ??= "0x";

            var newStaticData = Common.SpliceHex((string)previousStaticData, start, Common.Size(data), data);
            var newValue = DecodeValueArgs(
                table.Schema,
                newStaticData,
                (string)previousEncodedLengths,
                (string)previousDynamicData);

            Debug.Log(
                $"Setting table via splice static: {tableResource.Namespace}:{tableResource.Name}, {JsonConvert.SerializeObject(newValue)}");
            ds.Update(table, entity, new Dictionary<string, object>(newValue)
            {
                ["__StaticData"] = newStaticData
            });
        }

        private static void HandleStoreSpliceDynamic(RxDatastore ds, FilterLog log,
            StoreSpliceDynamicDataEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);

            if (!ds.registeredTables.Contains(tableId))
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {tableId} at {log.Address}");
                return;
            }

            var hasTable = ds.store.TryGetValue(tableId, out var table);
            if (!hasTable) return;

            var start = decoded.Start;
            var data = Common.BytesToHex(decoded.Data);
            var encodedLengths = Common.BytesToHex(decoded.EncodedLengths);
            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            var previousValue = ds.GetValue(table, entity);
            object previousStaticData;
            object previousEncodedLengths;
            object previousDynamicData;

            if (previousValue != null)
            {
                previousValue.value.TryGetValue("__staticData", out previousStaticData);
                previousValue.value.TryGetValue("__encodedLengths", out previousEncodedLengths);
                previousValue.value.TryGetValue("__dynamicData", out previousDynamicData);
            }
            else
            {
                previousStaticData = "0x";
                previousEncodedLengths = "0x";
                previousDynamicData = "0x";
            }

            previousStaticData ??= "0x";
            previousEncodedLengths ??= "0x";
            previousDynamicData ??= "0x";

            var newDynamicData = Common.SpliceHex((string)previousDynamicData, (int)start, Common.Size(data), data);
            var newValue = DecodeValueArgs(
                table.Schema,
                (string)previousStaticData,
                (string)previousEncodedLengths,
                newDynamicData);


            Debug.Log(
                $"Setting table via splice dynamic: {tableResource.Namespace}:{tableResource.Name}, {JsonConvert.SerializeObject(newValue)}");
            ds.Update(table, entity, new Dictionary<string, object>(newValue)
            {
                ["__encodedLengths"] = encodedLengths,
                ["__dynamicData"] = newDynamicData
            });
        }

        private static void HandleStoreDeleteRecord(RxDatastore ds, FilterLog log, StoreDeleteRecordEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);
            if (!ds.registeredTables.Contains(tableId))
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {tableId} at {log.Address}");
                return;
            }

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());
            var hasTable = ds.store.TryGetValue(tableId, out var table);
            if (!hasTable) return;

            Debug.Log(
                $"Deleting key for table: {tableResource.Namespace}:{tableResource.Name}");
            ds.Delete(table, entity);
        }
    }
}
