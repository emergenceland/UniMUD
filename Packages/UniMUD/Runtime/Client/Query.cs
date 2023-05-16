#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public class Query
    {
        public readonly List<string> findVars = new();
        public readonly List<List<string>> whereVars = new();

        public Query Find(params string[] variables)
        {
            findVars.AddRange(variables);
            return this;
        }

        public Query Where(params string[] clause)
        {
            whereVars.Add(new List<string>(clause));
            return this;
        }

        public List<Property> RunQuery(List<string> query, HashSet<Record> records,
            Property? bindings = null)
        {
            bindings ??= new Property();
            return records.Where(record => MatchPattern(query, record, new Property(bindings)) != null).Select(
                matched =>
                {
                    var newBindings = new Property(bindings);
                    query
                        .Select((queryPart, i) => new { queryPart, i })
                        .Where(collect =>
                            ClientUtils.IsVar(collect.queryPart) && !newBindings.ContainsKey(collect.queryPart))
                        .ToList()
                        .ForEach(collect => newBindings[collect.queryPart] = GetValue(matched, collect.i));
                    return newBindings;
                }).ToList();
        }

        private Property? MatchPattern(List<string> pattern, Record record, Property bindings)
        {
            return pattern
                .Select((patternPart, i) => MatchPart(patternPart, GetValue(record, i), bindings))
                .All(matchedBinding => matchedBinding != null)
                ? bindings
                : null;
        }

        private Property? MatchPart(string patternPart, object triplePart, Property bindings)
        {
            return ClientUtils.IsVar(patternPart)
                ? MatchVariable(patternPart, triplePart, bindings)
                : patternPart.Equals(triplePart.ToString())
                    ? bindings
                    : null;
        }

        private Property? MatchVariable(string variable, object triplePart, Property bindings)
        {
            if (bindings.TryGetValue(variable, out var bound))
            {
                return bound.ToString().Equals(triplePart.ToString()) ? bindings : null;
            }

            bindings[variable] = triplePart;
            return bindings;
        }

        private object GetValue(Record record, int index)
        {
            switch (index)
            {
                case 0: return record.table;
                case 1: return record.key;
                case 2: return record.attribute;
                case 3: return record.value;
                default: throw new ArgumentException("Invalid index");
            }
        }
    }
}
