using System;
using System.Collections.Generic;
using System.Linq;
using mud.Network.schemas;
using Newtonsoft.Json;
using UnityEngine;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public class TableFilter
    {
        public TableId Table { get; }
        public Condition[] Conditions { get; }
        public bool IsNegative { get; }

        public TableFilter(TableId table, Condition[] conditions = null, bool isNegative = false)
        {
            Table = table;
            Conditions = conditions ?? Array.Empty<Condition>();
            IsNegative = isNegative;
        }
    }

    public class Condition
    {
        public string Attribute { get; }
        public object Value { get; }

        public Condition(string attribute, object value)
        {
            Attribute = attribute;
            Value = value;
        }

        public static Condition Has(string attribute, object value)
        {
            return new Condition(attribute, value);
        }
    }

    public class Query
    {
        private readonly List<string> _queryVariables = new();

        private readonly List<TableFilter> _filters = new();
        
        public List<TableFilter> GetTableFilters()
        {
            return _filters;
        }

        public Query In(TableId table, Condition[] conditions = null)
        {
            var clause = new TableFilter(table, conditions);
            _filters.Add(clause);
            return this;
        }

        public Query Not(TableId table)
        {
            var clause = new TableFilter(table, null, true);
            _filters.Add(clause);
            return this;
        }

        public Query Select(params TableId[] variables)
        {
            foreach (var table in variables)
            {
                _queryVariables.Add(table.ToString());
            }

            return this;
        }

        public IEnumerable<Record> Run(Dictionary<string, Table> store)
        {
            HashSet<Record> context = null;
            foreach (var table in _filters)
            {
                if (!store.TryGetValue(table.Table.ToString(), out var candidates)) continue; // table not registered
                if (context == null)
                {
                    // populate context with first table
                    if (table.IsNegative) throw new Exception("Negative table filter cannot be first");
                    context = new HashSet<Record>(candidates.Records.Values);
                }
                else
                {
                    var recordsToRemove = new List<Record>();
                    var recordsToAdd = new List<Record>();

                    foreach (var record in context)
                    {
                        // does key also exist in the requested table?
                        var recordInTable = candidates.Records.TryGetValue(record.key, out var newRecord);
                        if (table.IsNegative)
                        {
                            if (recordInTable)
                            {
                                // If record exists in table and filter is negative, add to remove list
                                recordsToRemove.Add(record);
                            }
                        }
                        else
                        {
                            if (recordInTable)
                            {
                                // If record exists in table and filter is positive, add to add list
                                recordsToAdd.Add(newRecord);
                            }
                            else
                            {
                                // If record does not exist in table and filter is positive, add to remove list
                                recordsToRemove.Add(record);
                            }
                        }
                    }

                    foreach (var record in recordsToAdd)
                    {
                        context.Add(record);
                    }

                    foreach (var condition in table.Conditions)
                    {
                        foreach (var record in context)
                        {
                            if (!record.value.TryGetValue(condition.Attribute, out var value)) continue;
                            if (value.Equals(condition.Value)) continue;
                            recordsToRemove.Add(record);
                        }
                    }

                    foreach (var record in recordsToRemove)
                    {
                        context.Remove(record);
                    }
                }
            }

            if (_queryVariables.Count > 0 && context != null)
            {
                context.RemoveWhere(record => !_queryVariables.All(record.table.Contains));
            }

            // Debug.Log("FINAL RESULT: " + JsonConvert.SerializeObject(context));
            return context ?? new HashSet<Record>();
        }
    }
}
