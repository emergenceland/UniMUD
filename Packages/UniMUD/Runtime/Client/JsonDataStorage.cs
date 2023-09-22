using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace mud.Client
{
    public class JsonDataStorage : IDataStorage
    {
        private readonly string _filePath;
        public int BlockNumber { get; set; }

        public JsonDataStorage(string filePath)
        {
            _filePath = filePath;
        }

        public void Write(IEnumerable<RxRecord> records)
        {
            var json = JsonConvert.SerializeObject(records);
            File.WriteAllText(_filePath, json);
        }

        public IEnumerable<RxRecord> Load()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<RxRecord>();
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<IEnumerable<RxRecord>>(json);
        }
        
        public int GetCachedBlockNumber()
        {
            return BlockNumber;
        }
    } 
}
