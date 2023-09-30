using System.Collections.Generic;
using System.Linq;

namespace mud.Network
{
    public class Utils
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
    }
}
