#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using UnityEngine;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public class Record
    {
        protected bool Equals(Record other)
        {
            return table == other.table && key == other.key && attribute == other.attribute &&
                   value.Equals(other.value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Record)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(table, key, attribute, value);
        }

        public string table;
        public string key;
        public string attribute;
        public object value;

        public Record(string table, string key, string attribute, object value)
        {
            this.table = table;
            this.key = key;
            this.attribute = attribute;
            this.value = value;
        }
    }

    public class RecordUpdate
    {
        public string TableId { get; set; }
        public string TableName { get; set; }
        public string Key { get; set; }
        public Tuple<Property?, Property?> Value { get; set; }
    }

    public class Datastore
    {
        private HashSet<Record> _store;
        public readonly IDataStorage? dataStorage;

        // indexing
        private readonly Dictionary<string, HashSet<Record>> _tableIndex;
        private readonly Dictionary<string, HashSet<Record>> _attributeIndex;
        private readonly Dictionary<string, string> _nextTableKeys;
        private readonly Dictionary<string, string> _tableIdToName;
        private readonly Dictionary<string, string> _tableNameToId;

        private readonly ReplaySubject<RecordUpdate> _onDataStoreUpdate = new();
        public IObservable<RecordUpdate> OnDataStoreUpdate => _onDataStoreUpdate;

        public Datastore(IDataStorage? dataStorage = null)
        {
            _store = new HashSet<Record>();
            _tableIndex = _store.GroupBy(r => r.table).ToDictionary(g => g.Key, g => g.ToHashSet());
            _attributeIndex = _store.GroupBy(r => r.attribute).ToDictionary(g => g.Key, g => g.ToHashSet());
            _nextTableKeys = new Dictionary<string, string>();
            _tableIdToName = new Dictionary<string, string>();
            _tableNameToId = new Dictionary<string, string>();

            if (dataStorage != null) this.dataStorage = dataStorage;

            _tableIdToName["TableId<datastore:DSMetadata>"] = "datastore:DSMetadata";
            _tableNameToId["datastore:DSMetadata"] = "TableId<datastore:DSMetadata>";
        }

        public void RegisterTable(string tableId, string tableName, Dictionary<string, Types.Type>? schema = null,
            string? key = null)
        {
            SetValue("TableId<datastore:DSMetadata>", key, ClientUtils.CreateProperty(("tableName", tableName)));
            SetValue("TableId<datastore:DSMetadata>", key, ClientUtils.CreateProperty(("tableId", tableId)));
            _tableIdToName[tableId] = tableName;
            _tableNameToId[tableName] = tableId;
        }

        public void SetValue(string table, string? key, Property value)
        {
            var index = key ?? GetKey(table);
            foreach (var (propertyName, propertyValue) in value)
            {
                var newRecord = new Record(table, index, propertyName, propertyValue);
                if (!_store.Contains(newRecord))
                {
                    _store.Add(newRecord);
                    if (!_tableIndex.TryGetValue(table, out var tableIndexSet))
                    {
                        tableIndexSet = new HashSet<Record>();
                        _tableIndex[table] = tableIndexSet;
                    }

                    tableIndexSet.Add(newRecord);
                    
                    if (!_attributeIndex.TryGetValue(propertyName, out var attributeIndexSet))
                    {
                        attributeIndexSet = new HashSet<Record>();
                        _attributeIndex[propertyName] = attributeIndexSet;
                    }
                    attributeIndexSet.Add(newRecord);
                }
                else
                {
                    var existingRecord =
                        _store.First(r => r.table == table && r.key == index && r.attribute == propertyName);
                    existingRecord.value = propertyValue;
                }
                UpdateStream(table, index, value);
            }
        }

        public void UpdateValue(string table, string key, Property value, Property? initialValue)
        {
            var existingRecord = _store.FirstOrDefault(r => r.table == table && r.key == key);
            if (existingRecord == null)
            {
                SetValue(table, key, value);
            }
            else
            {
                _store
                    .Where(r => r.table == table && r.key == key)
                    .ToList()
                    .ForEach(r =>
                    {
                        r.value = value;
                        UpdateStream(table, key, value);
                    });
            }
        }

        public void DeleteValue(string table, string key)
        {
            _store
                .Where(r => r.table == table && r.key == key)
                .ToList()
                .ForEach(record =>
                {
                    var recordProperty = ClientUtils.CreateProperty((record.attribute, record.key));
                    _store.Remove(record);
                    _tableIndex[record.table].Remove(record);
                    _attributeIndex[record.attribute].Remove(record);
                    UpdateStream(table, key, null, recordProperty);
                });
        }

        protected string GetKey(string table)
        {
            if (_nextTableKeys.TryGetValue(table, out string nextKey))
            {
                var key = nextKey;
                _nextTableKeys[table] = GenerateNextKey(nextKey);
                return key;
            }
            else
            {
                var key = "1";
                _nextTableKeys[table] = GenerateNextKey(key);
                return key;
            }
        }

        private string GenerateNextKey(string currentKey)
        {
            int nextKey = int.Parse(currentKey) + 1;
            return nextKey.ToString();
        }

        public string? GetTableName(string tableId)
        {
            return _tableIdToName.TryGetValue(tableId, out var tableName) ? tableName : null;
        }

        public string? GetTableId(string tableName)
        {
            return _tableNameToId.TryGetValue(tableName, out var tableId) ? tableId : null;
        }

        private HashSet<Record> CandidateRecords(List<string> pattern)
        {
            if (pattern.Count >= 1 && !ClientUtils.IsVar(pattern[0]) && _tableIndex.ContainsKey(pattern[0]))
            {
                return _tableIndex.GetValueOrDefault(pattern[0], _store);
            }

            if (pattern.Count >= 3 && !ClientUtils.IsVar(pattern[2]) && _attributeIndex.ContainsKey(pattern[2]))
            {
                return _attributeIndex.GetValueOrDefault(pattern[2], _store);
            }

            return _store;
        }

        public List<Property> Query(Query query)
        {
            var bindingsList = query.whereVars.Aggregate(new List<Property> { new() }, (bindings, pattern) =>
            {
                var candidateRecords = CandidateRecords(pattern);
                return bindings.SelectMany(b => query.RunQuery(pattern, candidateRecords, b)).ToList();
            });

            if (query.findVars.Any())
            {
                bindingsList = bindingsList.Where(b => query.findVars.All(b.ContainsKey)).ToList().Select(prop =>
                {
                    var result = new Property();
                    foreach (var key in query.findVars)
                    {
                        result[key.Replace("?", "")] = prop[key];
                    }

                    return result;
                }).ToList();
            }

            return bindingsList;
        }

        private void UpdateStream(string tableId, string keyIndex, Property? value, Property? previousValue = null)
        {
            _onDataStoreUpdate.OnNext(new RecordUpdate
            {
                TableId = tableId,
                TableName = _tableIdToName[tableId],
                Key = keyIndex,
                Value = new Tuple<Property?, Property?>(value, previousValue)
            });
        }

        public void Save()
        {
            if (dataStorage != null) dataStorage.Write(_store);
        }

        public int? GetCachedBlockNumber()
        {
            if (dataStorage != null) return dataStorage.GetCachedBlockNumber();
            return null;
        }

        public void LoadCache()
        {
            if (dataStorage != null)
            {
                var data = dataStorage.Load();
                var updates = data.ToHashSet();
                // foreach (var record in updates)
                // {
                //     var prop = ClientUtils.CreateProperty((record.attribute, record.value));
                //     // UpdateStream(record.table, record.key, record.attribute, record.value));
                // }
                if (data != null) _store = updates;
                Debug.Log($"Got {updates?.Count()} items from cache");
            }
        }
    }
}
