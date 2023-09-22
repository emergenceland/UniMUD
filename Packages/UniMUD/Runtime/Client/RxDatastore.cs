#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using v2;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public class RxDatastore
    {
        // tableId -> table -> records
        public readonly Dictionary<string, RxTable> store = new();
        public Dictionary<string, ProtocolParser.Table> registeredTables = new();

        private readonly ReplaySubject<RecordUpdate> _onDataStoreUpdate = new();
        private readonly Subject<RecordUpdate> _onRxDataStoreUpdate = new();
        public IObservable<RecordUpdate> OnDataStoreUpdate => _onDataStoreUpdate;

        public void RegisterTable(ProtocolParser.Table table)
        {
            var tableKey = Common.GetTableKey(table);
            store.TryAdd(tableKey, new RxTable());
            registeredTables.TryAdd(tableKey, table);
        }

        public void RegisterTable(RxTable table)
        {
            store.TryAdd(table.Id, table);
        }

        public RxTable CreateTable(string tNamespace, string tName,
            Dictionary<string, SchemaAbiTypes.SchemaType> schema)
        {
            var id = v2.Common.ResourceIDToHex(new ResourceID()
            {
                Type = ResourceType.Table,
                Namespace = tNamespace,
                Name = tName,
            });
            return new RxTable()
            {
                Id = id,
                Schema = schema,
                Values = new Dictionary<string, RxRecord>()
            };
        }

        public ProtocolParser.Table GetTable(string ns, string name)
        {
            var result = registeredTables.FirstOrDefault(t => t.Value.Namespace == ns && t.Value.Name == name);
            return result.Value;
        }

        public void Set(RxTable table, string entity, Property value)
        {
            var hasTable = store.TryGetValue(table.Id, out var tableValue);
            var record = new RxRecord(table.Id, entity, value);
            if (tableValue != null && hasTable && table.Values.ContainsKey(entity))
            {
                store[table.Id].Values[entity] = record;
                EmitUpdate(UpdateType.SetField, table.Id, entity, record.value);
                return;
            }

            store[table.Id].Values[entity] = record;
            EmitUpdate(UpdateType.SetRecord, table.Id, entity, record.value);
        }

        public void Update(RxTable table, string entity, Property value, Property? initialValue = null)
        {
            var index = entity;
            if (!store[table.Id].Values.ContainsKey(index))
            {
                Set(table, index, value);
            }
            else
            {
                var record = new RxRecord(table.Id, entity, value);
                store[table.Id].Values[index] = record;
                EmitUpdate(UpdateType.SetField, table.Id, index, record.value, initialValue);
            }
        }

        public void Delete(RxTable table, string key)
        {
            if (!store[table.Id].Values.ContainsKey(key)) return;
            EmitUpdate(UpdateType.DeleteRecord, table.Id, key, null, store[table.Id].Values[key].value);
            store[table.Id].Values.Remove(key);
        }

        public RxRecord? GetValue(RxTable table, string key)
        {
            var hasTable = store.ContainsKey(table.Id);
            if (!hasTable) return null;
            var hasKey = store[table.Id].Values.TryGetValue(key, out var value);
            return hasKey ? value : null;
        }

        protected string GetKey(string tableId)
        {
            if (!store.ContainsKey(tableId)) throw new Exception($"Table {tableId} does not exist");
            return store[tableId].Values.Count == 0 ? "1" : (store[tableId].Values.Count + 1).ToString();
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
                queryTables.Select(t => _onRxDataStoreUpdate.Where(update => update.TableId == t.Id));
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
                    var recordFromStore = store[affectedTableKey].Values[affectedRecordKey];
                    
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
