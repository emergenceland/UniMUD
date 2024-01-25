using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using static mud.ProtocolParser;

namespace mud
{
    public class RxStorageAdapter
    {
        public static void ToStorage(ReplaySubject<RecordUpdate> onUpdate, RxDatastore ds, StorageAdapterBlock logs)
        {
            var dbUpdates = new List<RecordUpdate>();

            var newTables = logs.Logs.Where(l => l != null).Where(IsTableRegistrationLog).Select(LogToTable);
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
                if (log == null) continue;
                switch (log.eventName)
                {
                    case "Store_SetRecord":
                        HandleStoreSetRecord(dbUpdates, ds, log);
                        break;
                    case "Store_SpliceStatic":
                        HandleStoreSpliceStatic(dbUpdates, ds, log);
                        break;
                    case "Store_SpliceDynamic":
                        HandleStoreSpliceDynamic(dbUpdates, ds, log);
                        break;
                    case "Store_DeleteRecord":
                        HandleStoreDeleteRecord(dbUpdates, ds, log);
                        break;
                    default:
                        Debug.LogWarning($"Unknown event: {log.eventName}");
                        break;
                }
            }

            for (var i = 0; i < dbUpdates.Count; i++)
            {
                var update = dbUpdates[i];
                switch (update.Type)
                {
                    case UpdateType.SetRecord:
                        update.Table.Set(update.CurrentRecordKey, update.CurrentRecordValue);
                        dbUpdates[i] = update;
                        break;
                    case UpdateType.SetField:
                        update.PreviousRecordValue =
                            update.Table.Update(update.CurrentRecordKey, update.CurrentRecordValue)?.RawValue;
                        update.PreviousRecordKey = update.CurrentRecordKey;
                        dbUpdates[i] = update;
                        break;
                    case UpdateType.DeleteRecord:
                        var previousValue = update.Table.Delete(update.CurrentRecordKey);
                        update.PreviousRecordValue = previousValue?.RawValue;
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

        private static void HandleStoreSetRecord(List<RecordUpdate> updates, RxDatastore ds,
            MudLog log)
        {
            var table = ds.TryGetTableById(log.args.tableId);
            if (table == null)
            {
                if (NetworkManager.Verbose)
                {
                    Debug.LogWarning(
                        $"Skipping update for unknown table: {JsonConvert.SerializeObject(Common.HexToResourceId(log.args.tableId))} at {log.address}, {log.args.tableId}");
                }

                return;
            }

            var entity = Common.ConcatHex(log.args.keyTuple);
            var value = DecodeValueArgs(table.Schema, log.args.staticData, log.args.encodedLengths,
                log.args.dynamicData);

            if (NetworkManager.Verbose)
            {
                var tableResource = Common.HexToResourceId(log.args.tableId);
                Debug.Log(
                    $"Setting table: {tableResource.Namespace}:{tableResource.Name}, Value: {JsonConvert.SerializeObject(value)}");
            }

            updates.Add(new RecordUpdate
            {
                Type = UpdateType.SetRecord,
                Table = table,
                CurrentRecordValue = new Dictionary<string, object>(value)
                {
                    ["__staticData"] = log.args.staticData,
                    ["__encodedLengths"] = log.args.encodedLengths,
                    ["__dynamicData"] = log.args.dynamicData
                },
                PreviousRecordValue = null,
                CurrentRecordKey = entity,
                PreviousRecordKey = null,
            });
        }

        private static void HandleStoreSpliceStatic(List<RecordUpdate> updates, RxDatastore ds,
            MudLog log)
        {
            var table = ds.TryGetTableById(log.args.tableId);
            if (table == null)
            {
                if (NetworkManager.Verbose)
                {
                    Debug.LogWarning(
                        $"Skipping update for unknown table: {JsonConvert.SerializeObject(Common.HexToResourceId(log.args.tableId))} at {log.address}, {log.args.tableId}");
                }

                return;
            }

            var entity = Common.ConcatHex(log.args.keyTuple);

            var previousValue = table.GetValue(entity);

            var newValue = DecodeValueArgs(
                table.Schema,
                log.args.staticData,
                log.args.encodedLengths,
                log.args.dynamicData
            );

            if (NetworkManager.Verbose)
            {
                var tableResource = Common.HexToResourceId(log.args.tableId);
                Debug.Log(
                    $"Setting table via splice static: {tableResource.Namespace}:{tableResource.Name}, {JsonConvert.SerializeObject(newValue)}, {log.args.tableId}");
            }

            updates.Add(new RecordUpdate
            {
                Type = UpdateType.SetField,
                Table = table,
                CurrentRecordValue = new Dictionary<string, object>(newValue)
                {
                    ["__staticData"] = log.args.staticData,
                },
                PreviousRecordValue = previousValue,
                CurrentRecordKey = entity,
                PreviousRecordKey = entity,
            });
        }

        private static void HandleStoreSpliceDynamic(List<RecordUpdate> updates, RxDatastore ds,
            MudLog log)
        {
            var tableId = log.args.tableId;
            var table = ds.TryGetTableById(tableId);
            if (table == null)
            {
                if (NetworkManager.Verbose)
                {
                    Debug.LogWarning(
                        $"Skipping update for unknown table: {JsonConvert.SerializeObject(Common.HexToResourceId(log.args.tableId))} at {log.address}, {log.args.tableId}");
                }

                return;
            }

            var entity = Common.ConcatHex(log.args.keyTuple);

            var previousValue = table.GetValue(entity);

            var newValue = DecodeValueArgs(
                table.Schema,
                log.args.staticData,
                log.args.encodedLengths,
                log.args.dynamicData);

            if (NetworkManager.Verbose)
            {
                var tableResource = Common.HexToResourceId(tableId);
                Debug.Log(
                    $"Setting table via splice dynamic: {tableResource.Namespace}:{tableResource.Name}, {JsonConvert.SerializeObject(newValue)}");
            }

            updates.Add(new RecordUpdate
            {
                Type = UpdateType.SetField,
                Table = table,
                CurrentRecordValue =
                    new Dictionary<string, object>(newValue)
                    {
                        ["__encodedLengths"] = log.args.encodedLengths,
                        ["__dynamicData"] = log.args.dynamicData,
                    },
                PreviousRecordValue = previousValue,
                CurrentRecordKey = entity,
                PreviousRecordKey = entity,
            });
        }

        private static void HandleStoreDeleteRecord(List<RecordUpdate> dbOps, RxDatastore ds,
            MudLog log)
        {
            var table = ds.TryGetTableById(log.args.tableId);
            if (table == null)
            {
                if (NetworkManager.Verbose)
                {
                    Debug.LogWarning(
                        $"Skipping update for unknown table: {JsonConvert.SerializeObject(Common.HexToResourceId(log.args.tableId))} at {log.address}, {log.args.tableId}");
                }

                return;
            }

            var entity = Common.ConcatHex(log.args.keyTuple);

            if (NetworkManager.Verbose)
            {
                var tableResource = Common.HexToResourceId(log.args.tableId);
                Debug.Log(
                    $"Deleting key for table: {tableResource.Namespace}:{tableResource.Name}");
            }

            dbOps.Add(new RecordUpdate
            {
                Table = table,
                Type = UpdateType.DeleteRecord,
                CurrentRecordKey = entity,
                CurrentRecordValue = null,
                PreviousRecordKey = entity,
                PreviousRecordValue = null, // TODO: get previous value later
            });
        }
    }
}
