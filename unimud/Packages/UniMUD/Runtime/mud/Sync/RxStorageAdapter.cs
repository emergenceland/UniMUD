using System;
using System.Collections.Generic;
using System.Linq;
using mud.IStore.ContractDefinition;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using static mud.ProtocolParser;

namespace mud
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

        public static void ToStorage(ReplaySubject<RecordUpdate> onUpdate, RxDatastore ds, StorageAdapterBlock logs)
        {
            var dbUpdates = new List<RecordUpdate>();

            var newTables = logs.Logs.Where(IsTableRegistrationLog).Select(LogToTable);
            foreach (var newTable in newTables)
            {
                var id = Common.ResourceIDToHex(new ResourceID
                {
                    Type = newTable.OffchainOnly != null ? ResourceType.OffchainTable : ResourceType.Table,
                    Namespace = newTable.Namespace,
                    Name = newTable.Name,
                });
                ds.RegisterTable(id, newTable.Name, newTable.ValueSchema);
            }

            foreach (var log in logs.Logs)
            {
                var decoded = DecodeEvent(log);

                decoded.Match(
                    setRecord => HandleStoreSetRecord(dbUpdates, ds, log, setRecord),
                    spliceStatic => HandleStoreSpliceStatic(dbUpdates, ds, log, spliceStatic),
                    spliceDynamic => HandleStoreSpliceDynamic(dbUpdates, ds, log, spliceDynamic),
                    deleteRecord => HandleStoreDeleteRecord(dbUpdates, ds, log, deleteRecord)
                );
            }

            for (var i = 0; i < dbUpdates.Count; i++)
            {
                var update = dbUpdates[i];
                switch (update.Type)
                {
                    case UpdateType.SetRecord:
                        update.Table.Set(update.CurrentRecordKey, update.CurrentValue);
                        dbUpdates[i] = update;
                        break;
                    case UpdateType.SetField:
                        update.PreviousValue = update.Table.Update(update.CurrentRecordKey, update.CurrentValue)?.RawValue;
                        update.PreviousRecordKey = update.CurrentRecordKey;
                        dbUpdates[i] = update;
                        break;
                    case UpdateType.DeleteRecord:
                        var previousValue = update.Table.Delete(update.CurrentRecordKey);
                        update.PreviousValue = previousValue?.RawValue;
                        dbUpdates[i] = update;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var t in dbUpdates)
            {
                onUpdate.OnNext(t);
            }
        }

        private static void HandleStoreSetRecord(List<RecordUpdate> updates, RxDatastore ds, FilterLog log,
            StoreSetRecordEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);
            var table = ds.TryGetTable(tableResource.Name);
            if (table == null)
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {JsonConvert.SerializeObject(tableResource)} at {log.Address}, {tableId}");
                return;
            }

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());
            var staticData = Common.BytesToHex(decoded.StaticData);
            var encodedLengths = Common.BytesToHex(decoded.EncodedLengths);
            var dynamicData = Common.BytesToHex(decoded.DynamicData);
            var value = DecodeValueArgs(table.Schema, staticData, encodedLengths, dynamicData);

            var expandedValue = new Dictionary<string, object>(value)
            {
                ["__staticData"] = staticData,
                ["__encodedLengths"] = encodedLengths,
                ["__dynamicData"] = dynamicData
            };

            Debug.Log(
                $"Setting table: {tableResource.Namespace}:{tableResource.Name}, Value: {JsonConvert.SerializeObject(value)}");
            updates.Add(new RecordUpdate
            {
                Type = UpdateType.SetRecord,
                Table = table,
                CurrentValue = expandedValue,
                PreviousValue = null,
                CurrentRecordKey = entity,
                PreviousRecordKey = null,
            });
        }

        private static void HandleStoreSpliceStatic(List<RecordUpdate> updates, RxDatastore ds, FilterLog log,
            StoreSpliceStaticDataEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);
            var table = ds.TryGetTable(tableResource.Name);
            if (table == null)
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {JsonConvert.SerializeObject(tableResource)} at {log.Address}, {tableId}");
                return;
            }

            var start = (int)decoded.Start;
            var data = Common.BytesToHex(decoded.Data);
            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            // var previousValue = ds.GetValue(table, entity);
            var previousValue = table.GetValue(entity);

            object previousStaticData;
            object previousEncodedLengths;
            object previousDynamicData;

            if (previousValue != null)
            {
                previousValue.RawValue.TryGetValue("__staticData", out previousStaticData);
                previousValue.RawValue.TryGetValue("__encodedLengths", out previousEncodedLengths);
                previousValue.RawValue.TryGetValue("__dynamicData", out previousDynamicData);
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

            var expandedValue = new Dictionary<string, object>(newValue)
            {
                ["__staticData"] = newStaticData,
            };

            Debug.Log(
                $"Setting table via splice static: {tableResource.Namespace}:{tableResource.Name}, {JsonConvert.SerializeObject(newValue)}");
            updates.Add(new RecordUpdate
            {
                Type = UpdateType.SetField,
                Table = table,
                CurrentValue = expandedValue,
                PreviousValue = previousValue,
                CurrentRecordKey = entity,
                PreviousRecordKey = entity,
            });
        }

        private static void HandleStoreSpliceDynamic(List<RecordUpdate> updates, RxDatastore ds, FilterLog log,
            StoreSpliceDynamicDataEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);
            var table = ds.TryGetTable(tableResource.Name);
            if (table == null)
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {JsonConvert.SerializeObject(tableResource)} at {log.Address}, {tableId}");
                return;
            }

            var start = decoded.Start;
            var data = Common.BytesToHex(decoded.Data);
            var encodedLengths = Common.BytesToHex(decoded.EncodedLengths);
            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            // var previousValue = ds.GetValue(table, entity);
            var previousValue = table.GetValue(entity);
            object previousStaticData;
            object previousEncodedLengths;
            object previousDynamicData;

            if (previousValue != null)
            {
                previousValue.RawValue.TryGetValue("__staticData", out previousStaticData);
                previousValue.RawValue.TryGetValue("__encodedLengths", out previousEncodedLengths);
                previousValue.RawValue.TryGetValue("__dynamicData", out previousDynamicData);
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
            var expandedValue = new Dictionary<string, object>(newValue)
            {
                ["__encodedLengths"] = encodedLengths,
                ["__dynamicData"] = newDynamicData,
            };


            Debug.Log(
                $"Setting table via splice dynamic: {tableResource.Namespace}:{tableResource.Name}, {JsonConvert.SerializeObject(newValue)}");
            updates.Add(new RecordUpdate
            {
                Type = UpdateType.SetField,
                Table = table,
                CurrentValue = expandedValue,
                PreviousValue = previousValue,
                CurrentRecordKey = entity,
                PreviousRecordKey = entity,
            });
        }

        private static void HandleStoreDeleteRecord(List<RecordUpdate> dbOps, RxDatastore ds, FilterLog log,
            StoreDeleteRecordEventDTO decoded)
        {
            var tableId = Common.BytesToHex(decoded.TableId);
            var tableResource = Common.HexToResourceId(tableId);
            var table = ds.TryGetTable(tableResource.Name);
            if (table == null)
            {
                Debug.LogWarning(
                    $"Skipping update for unknown table: {JsonConvert.SerializeObject(tableResource)} at {log.Address}, {tableId}");
                return;
            }

            var entity = Common.ConcatHex(decoded.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

            Debug.Log(
                $"Deleting key for table: {tableResource.Namespace}:{tableResource.Name}");
            dbOps.Add(new RecordUpdate
            {
                Table = table,
                Type = UpdateType.DeleteRecord,
                CurrentRecordKey = entity,
                CurrentValue = null,
                PreviousRecordKey = entity,
                PreviousValue = null, // TODO: get previous value later
            });
        }
    }
}
