using System.Collections.Generic;
using System.Linq;
using Nethereum.Contracts;
using Newtonsoft.Json;
using UnityEngine;
using v2.IStore.ContractDefinition;
using static v2.ProtocolParser;

namespace v2
{
    public class StorageAdapter
    {
        public static void ToStorage(mud.Client.RxDatastore ds, StorageAdapterBlock logs)
        {
            var newTables = logs.Logs.Where(IsTableRegistrationLog).Select(LogToTable);
            foreach (var newTable in newTables)
            {
                var tableEntity = mud.Client.Common.CreateTableKey(newTable.Address, newTable.Namespace, newTable.Name);
                if (ds.tableIds.ContainsKey(tableEntity))
                {
                    Debug.LogWarning($"Skipping registration for already registered table: {tableEntity}");
                    continue;
                }

                Debug.Log("Registering table: " + JsonConvert.SerializeObject(newTable));
                ds.RegisterTable(newTable);
            }

            foreach (var log in logs.Logs)
            {
                var storeSetRecordSignature =
                    new StoreSetRecordEventDTO().GetEventABI().Sha3Signature;
                var storeSpliceStaticDataSignature =
                    new StoreSpliceStaticDataEventDTO().GetEventABI().Sha3Signature;
                var storeSpliceDynamicDataSignature =
                    new StoreSpliceDynamicDataEventDTO().GetEventABI().Sha3Signature;
                var storeDeleteRecordSignature =
                    new StoreDeleteRecordEventDTO().GetEventABI().Sha3Signature;
                if (log.IsLogForEvent(storeSetRecordSignature))
                {
                    var decoded = Event<StoreSetRecordEventDTO>.DecodeEvent(log);
                    var tableIdHex = Common.BytesToHex(decoded.Event.TableId);
                    var tableResource = Common.HexToResourceId(tableIdHex);
                    var hasTable = ds.tableIds.TryGetValue(
                        mud.Client.Common.CreateTableKey(log.Address, tableResource.Namespace, tableResource.Name),
                        out var table);
                    if (!hasTable)
                    {
                        Debug.LogWarning(
                            $"Skipping update for unknown table: {tableResource.Namespace}:{tableResource.Name} at {log.Address}");
                        continue;
                    }

                    var entity = Common.ConcatHex(decoded.Event.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

                    var staticData = Common.BytesToHex(decoded.Event.StaticData);
                    var encodedLengths = Common.BytesToHex(decoded.Event.EncodedLengths);
                    var dynamicData = Common.BytesToHex(decoded.Event.DynamicData);
                    var value = DecodeValueArgs(table.ValueSchema, staticData, encodedLengths, dynamicData);
                    value.TryAdd("__staticData", staticData);
                    value.TryAdd("__encodedLengths", encodedLengths);
                    value.TryAdd("__dynamicData", dynamicData);
                    Debug.Log(
                        $"Setting table: {table.Namespace}:{table.Name}, Key: {entity}, Value: {JsonConvert.SerializeObject(value)}");
                    ds.Set(table, entity, value);
                }
                else if (log.IsLogForEvent(storeSpliceStaticDataSignature))
                {
                    var decoded = Event<StoreSpliceStaticDataEventDTO>.DecodeEvent(log);
                    var tableIdHex = Common.BytesToHex(decoded.Event.TableId);
                    var tableResource = Common.HexToResourceId(tableIdHex);
                    var tableKey =
                        mud.Client.Common.CreateTableKey(log.Address, tableResource.Namespace, tableResource.Name);

                    var start = decoded.Event.Start;
                    var deleteCount = decoded.Event.DeleteCount;
                    var data = Common.BytesToHex(decoded.Event.Data);

                    Debug.Log($"start: {start}, deleteCount: {deleteCount}, data: {data}");

                    var hasTable = ds.tableIds.TryGetValue(tableKey, out var table);
                    if (!hasTable)
                    {
                        Debug.LogWarning("Skipping update for unknown table: " + tableKey);
                        continue;
                    }

                    var previousValue = ds.GetValue(table, tableKey);
                    var previousStaticData = (string)previousValue?.value["__staticData"] ?? "0x";
                    Debug.Log("Previous static data: " + previousStaticData);
                    ;
                    var newStaticData = Common.SpliceHex(previousStaticData, (int)start, (int)deleteCount, data);
                    var newValue = DecodeValueArgs(table.ValueSchema, newStaticData,
                        (string)previousValue?.value["__encodedLengths"] ?? "0x",
                        (string)previousValue?.value["__dynamicData"] ?? "0x");
                    newValue.TryAdd("__staticData", newStaticData);

                    var entity = Common.ConcatHex(decoded.Event.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

                    Debug.Log(
                        $"Setting table via splice static: {JsonConvert.SerializeObject(table.Namespace)}:{JsonConvert.SerializeObject(table.Name)}");
                    Debug.Log(
                        $"Key: {entity}, {previousStaticData}, {newStaticData}, {JsonConvert.SerializeObject(previousValue)}, {JsonConvert.SerializeObject(newValue)}");
                    ds.Update(table, entity, newValue);
                }
                else if (log.IsLogForEvent(storeSpliceDynamicDataSignature))
                {
                    var decoded = Event<StoreSpliceDynamicDataEventDTO>.DecodeEvent(log);
                    var tableIdHex = Common.BytesToHex(decoded.Event.TableId);
                    var tableResource = Common.HexToResourceId(tableIdHex);
                    var tableKey =
                        mud.Client.Common.CreateTableKey(log.Address, tableResource.Namespace, tableResource.Name);

                    var start = decoded.Event.Start;
                    var deleteCount = decoded.Event.DeleteCount;
                    var data = Common.BytesToHex(decoded.Event.Data);
                    var encodedLengths = Common.BytesToHex(decoded.Event.EncodedLengths);

                    var hasTable = ds.tableIds.TryGetValue(tableKey, out var table);
                    if (!hasTable)
                    {
                        Debug.LogWarning("Skipping update for unknown table: " + tableKey);
                        continue;
                    }

                    var previousValue = ds.GetValue(table, tableKey);
                    var previousDynamicData = (string)previousValue?.value["__dynamicData"] ?? "0x";
                    var newDynamicData = Common.SpliceHex(previousDynamicData, (int)start, (int)deleteCount, data);
                    var newValue = DecodeValueArgs(table.ValueSchema,
                        (string)previousValue?.value["__staticData"] ?? "0x",
                        (string)previousValue?.value["__encodedLengths"] ?? "0x",
                        newDynamicData);
                    newValue.TryAdd("__encodedLengths", encodedLengths);
                    newValue.TryAdd("__dynamicData", newDynamicData);

                    var entity = Common.ConcatHex(decoded.Event.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());

                    Debug.Log(
                        $"Setting table via splice dynamic: {table.Namespace}:{table.Name}, Key: {entity}, {previousDynamicData}, {newDynamicData}, {previousValue}, {newValue}");
                    ds.Update(table, entity, newValue);
                }
                else if (log.IsLogForEvent(storeDeleteRecordSignature))
                {
                    var decoded = Event<StoreSpliceDynamicDataEventDTO>.DecodeEvent(log);
                    var tableIdHex = Common.BytesToHex(decoded.Event.TableId);
                    var tableResource = Common.HexToResourceId(tableIdHex);
                    var tableKey =
                        mud.Client.Common.CreateTableKey(log.Address, tableResource.Namespace, tableResource.Name);

                    var entity = Common.ConcatHex(decoded.Event.KeyTuple.Select(b => Common.BytesToHex(b)).ToArray());


                    var hasTable = ds.tableIds.TryGetValue(tableKey, out var table);
                    if (!hasTable)
                    {
                        Debug.LogWarning("Skipping update for unknown table: " + tableKey);
                        continue;
                    }

                    Debug.Log(
                        $"Deleting key for table: {JsonConvert.SerializeObject(table.Namespace)}:{JsonConvert.SerializeObject(table.Name)}");
                    ds.Delete(table, entity);
                }
            }
        }
    }
}
