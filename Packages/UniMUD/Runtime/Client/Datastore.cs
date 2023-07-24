#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using mud.Network.schemas;
using NLog;
using Logger = NLog.Logger;

namespace mud.Client
{
    using Property = Dictionary<string, object>;
    
    public class Datastore
    {
        // tableId -> table -> records
        public readonly Dictionary<string, Table> store;
        public Dictionary<string, TableId> tableIds;

        private readonly ReplaySubject<RecordUpdate> _onDataStoreUpdate = new();
        private readonly Subject<RecordUpdate> _onRxDataStoreUpdate = new(); // bit of a hack rn
        public IObservable<RecordUpdate> OnDataStoreUpdate => _onDataStoreUpdate;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Datastore()
        {
            store = new Dictionary<string, Table>();
            tableIds = new Dictionary<string, TableId>();
        }

        public void RegisterTable(TableId table, Dictionary<string, Types.Type>? schema = null,
            string? key = null)
        {
            store.TryAdd(table.ToString(), new Table());
            tableIds.TryAdd(table.ToString(), table);
        }

        public void Set(TableId tableId, string? entity, Property value)
        {
            var tableKey = tableId.ToString();
            var index = entity ?? GetKey(tableKey);
            var hasTable = store.TryGetValue(tableKey, out var table);
            var record = new Record(tableKey, entity, value);
            if (table != null && hasTable && table.Records.ContainsKey(index))
            {
                store[tableKey].Records[index] = record;
                EmitUpdate(UpdateType.SetField, tableKey, index, record.value);
                return;
            }

            store[tableKey].Records[index] = record;
            EmitUpdate(UpdateType.SetRecord, tableKey, index, record.value);
        }

        public void Update(TableId tableId, string? entity, Property value, Property? initialValue = null)
        {
            var tableKey = tableId.ToString();
            var index = entity ?? GetKey(tableKey);
            if (!store[tableKey].Records.ContainsKey(index))
            {
                Set(tableId, index, value);
            }
            else
            {
                var record = new Record(tableKey, entity, value);
                store[tableKey].Records[index] = record;
                EmitUpdate(UpdateType.SetField, tableKey, index, record.value, initialValue);
            }
        }

        public void Delete(TableId tableId, string key)
        {
            var tableKey = tableId.ToString();
            if (!store[tableKey].Records.ContainsKey(key)) return;
            EmitUpdate(UpdateType.DeleteRecord, tableKey, key, null, store[tableKey].Records[key].value);
            store[tableKey].Records.Remove(key);
        }

        public Record? GetValue(TableId tableId, string key)
        {
            var tableKey = tableId.ToString();
            var hasTable = store.TryGetValue(tableKey, out var table);
            if (!hasTable) return null;
            var hasKey = store[tableKey].Records.TryGetValue(key, out var value);
            return hasKey ? value : null;
        }

        protected string GetKey(string tableId)
        {
            if (!store.ContainsKey(tableId)) throw new Exception($"Table {tableId} does not exist");
            return store[tableId].Records.Count == 0 ? "1" : (store[tableId].Records.Count + 1).ToString();
        }

        public IEnumerable<Record> RunQuery(Query query)
        {
            return query.Run(store);
        }

        public IObservable<(List<Record> SetRecords, List<Record>RemovedRecords)> RxQuery(Query query)
        {
            var queryTables = query.GetTableFilters().Select(f => f.Table);
            var tableSubjects =
                queryTables.Select(t => _onRxDataStoreUpdate.Where(update => update.TableId == t.ToString()));
            var tableUpdates = tableSubjects.Merge();

            return Observable.Create<(List<Record>, List<Record>)>(observer =>
            {
                var initialResult = RunQuery(query).ToList();
                observer.OnNext((SetRecords: initialResult, RemovedRecords: new List<Record>()));

                var updateSubscription = tableUpdates.Subscribe(update =>
                {
                    var updated = RunQuery(query).ToList();
                    switch (update.Type)
                    {
                        case UpdateType.SetField:
                        case UpdateType.SetRecord:
                            observer.OnNext((SetRecords: updated, RemovedRecords: new List<Record>()));
                            break;
                        case UpdateType.DeleteRecord:
                            observer.OnNext((new List<Record>(), updated));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
                return updateSubscription;
            });
        }

        private void EmitUpdate(UpdateType type, string tableId, string keyIndex, Property? value,
            Property? previousValue = null)
        {
            _onDataStoreUpdate.OnNext(new RecordUpdate
            {
                Type = type,
                TableId = tableId,
                Key = keyIndex,
                Value = new Tuple<Property?, Property?>(value, previousValue)
            });
            
            _onRxDataStoreUpdate.OnNext(new RecordUpdate
            {
                Type = type,
                TableId = tableId,
                Key = keyIndex,
                Value = new Tuple<Property?, Property?>(value, previousValue)
            });
        }
    }
}
