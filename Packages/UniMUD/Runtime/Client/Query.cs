using System;
using System.Collections.Generic;
using System.Linq;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public class TableFilter
    {
        public RxTable Table { get; }
        public Condition[] Conditions { get; }
        public bool IsNegative { get; }

        public TableFilter(RxTable table, Condition[] conditions = null, bool isNegative = false)
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

        public Query In(RxTable table, Condition[] conditions = null)
        {
            var clause = new TableFilter(table, conditions);
            _filters.Add(clause);
            return this;
        }

        public Query Not(RxTable table)
        {
            var clause = new TableFilter(table, null, true);
            _filters.Add(clause);
            return this;
        }

        public Query Select(params string[] variables)
        {
            foreach (var table in variables)
            {
                _queryVariables.Add(table.ToString());
            }

            return this;
        }

        public IEnumerable<RxRecord> Run(Dictionary<string, RxTable> store)
        {
            HashSet<RxRecord> context = null;
            foreach (var table in _filters)
            {
                if (!store.TryGetValue(table.Table.Id, out var candidates)) continue; // table not registered
                if (context == null)
                {
                    // populate context with first table
                    if (table.IsNegative) throw new Exception("Negative table filter cannot be first");
                    context = new HashSet<RxRecord>(candidates.Values.Values);
                }
                else
                {
                    var recordsToRemove = new List<RxRecord>();
                    var recordsToAdd = new List<RxRecord>();

                    foreach (var record in context)
                    {
                        var recordInTable = candidates.Values.TryGetValue(record.key, out var newRecord);
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
                context.RemoveWhere(record => !_queryVariables.All(record.tableId.Contains));
            }

            return context ?? new HashSet<RxRecord>();
        }
    }
}
