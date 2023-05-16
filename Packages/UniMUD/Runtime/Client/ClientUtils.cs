#nullable enable
using System.Collections.Generic;
using System.Linq;
using mud.Network.schemas;

namespace mud.Client
{
    using Property = Dictionary<string, object>;

    public struct MudTableMetadata
    {
        public TableId TableId { get; set; }
        public string TableName { get; set; }
    }

    public static class ClientUtils
    {
        public static Dictionary<string, object> CreateProperty(
            params (string Key, object Value)[] keyValuePairs
        )
        {
            return keyValuePairs.ToDictionary(
                keyValuePair => keyValuePair.Key,
                keyValuePair => keyValuePair.Value
            );
        }
        
        public static bool IsVar(string str)
        {
            return str.StartsWith("?");
        }
    }
    
}
