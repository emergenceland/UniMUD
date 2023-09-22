using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static v2.ProtocolParser;

namespace v2
{
    public class StorageAdapter
    {
        public static void ToStorage(StorageAdapterBlock logs)
        {
            var newTables = logs.Logs.Where(IsTableRegistrationLog).Select(LogToTable);
            Debug.Log("All new tables: " + JsonConvert.SerializeObject(newTables));
            foreach (var newTable in newTables)
            {
                Debug.Log("New table: " + JsonConvert.SerializeObject(newTable));
            }
        }
    }
}
