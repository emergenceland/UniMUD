#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using mud.Network.schemas;
using NLog;
using v2;

namespace mud.Client
{
    using Property = Dictionary<string, object>;
    
    public class RxDatastore
    {
        // tableId -> table -> records
        public readonly Dictionary<string, RxTable> store = new();
        public Dictionary<string, ProtocolParser.Table> tableIds = new();

        private readonly ReplaySubject<RecordUpdate> _onDataStoreUpdate = new();
        private readonly Subject<RecordUpdate> _onRxDataStoreUpdate = new();
        public IObservable<RecordUpdate> OnDataStoreUpdate => _onDataStoreUpdate;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void RegisterTable(ProtocolParser.Table table)
        {
            var tableKey = Common.GetTableKey(table);
            store.TryAdd(tableKey, new RxTable());
            tableIds.TryAdd(tableKey, table);
        }

        public void Set(ProtocolParser.Table tableId, string entity, Property value)
        {
            var tableKey = Common.GetTableKey(tableId);
            var hasTable = store.TryGetValue(tableKey, out var table);
            var record = new RxRecord(tableKey, entity, value);
            if (table != null && hasTable && table.Records.ContainsKey(entity))
            {
                store[tableKey].Records[entity] = record;
                EmitUpdate(UpdateType.SetField, tableKey, entity, record.value);
                return;
            }

            store[tableKey].Records[entity] = record;
            EmitUpdate(UpdateType.SetRecord, tableKey, entity, record.value);
        }

        public void Update(ProtocolParser.Table tableId, string entity, Property value, Property? initialValue = null)
        {
            var tableKey = Common.GetTableKey(tableId);
            var index = entity;
            if (!store[tableKey].Records.ContainsKey(index))
            {
                Set(tableId, index, value);
            }
            else
            {
                var record = new RxRecord(tableKey, entity, value);
                store[tableKey].Records[index] = record;
                EmitUpdate(UpdateType.SetField, tableKey, index, record.value, initialValue);
            }
        }

        public void Delete(ProtocolParser.Table tableId, string key)
        {
            var tableKey = Common.GetTableKey(tableId);
            if (!store[tableKey].Records.ContainsKey(key)) return;
            EmitUpdate(UpdateType.DeleteRecord, tableKey, key, null, store[tableKey].Records[key].value);
            store[tableKey].Records.Remove(key);
        }

        public RxRecord? GetValue(ProtocolParser.Table tableId, string key)
        {
            var tableKey = Common.GetTableKey(tableId);
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

        public IEnumerable<RxRecord> RunQuery(Query query)
        {
            return query.Run(store);
        }

        public IObservable<(List<RxRecord> SetRecords, List<RxRecord> RemovedRecords)> RxQuery(Query query,
            bool pushInitialResults = true)
        {
            var queryTables = query.GetTableFilters().Select(f => f.Table);
            var tableSubjects =
                queryTables.Select(t => _onRxDataStoreUpdate.Where(update => update.TableId == t.ToString()));
            var tableUpdates = tableSubjects.Merge();

            return Observable.Create<(List<RxRecord> SetRecords, List<RxRecord> RemovedRecords)>(observer =>
            {
                var initialResult = RunQuery(query).ToList();
                if (pushInitialResults)
                {
                    observer.OnNext((SetRecords: initialResult, RemovedRecords: new List<RxRecord>()));
                }

                var updateSubscription = tableUpdates.Subscribe(update =>
                {
                    var affectedRecordKey = update.Key;
                    var affectedTableKey = update.TableId;
                    var recordFromStore = store[affectedTableKey].Records[affectedRecordKey];

                    switch (update.Type)
                    {
                        case UpdateType.SetField:
                        case UpdateType.SetRecord:
                            // first, run query on the record
                            var setRecordQuery = RunQuery(query).ToList();
                            // is the record in the query result?
                            var setRecord = setRecordQuery.SingleOrDefault(r => r.key == affectedRecordKey);
                            if (setRecord != null)
                            {
                                observer.OnNext((SetRecords: new List<RxRecord> { setRecord },
                                    RemovedRecords: new List<RxRecord>()));
                            }
                            break;
                        case UpdateType.DeleteRecord:
                            observer.OnNext((SetRecords: new List<RxRecord>(),
                                RemovedRecords: new List<RxRecord> { recordFromStore }));
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
