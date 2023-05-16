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

        public void Write(IEnumerable<Record> records)
        {
            var json = JsonConvert.SerializeObject(records);
            File.WriteAllText(_filePath, json);
        }

        public IEnumerable<Record> Load()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<Record>();
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<IEnumerable<Record>>(json);
        }
        
        public int GetCachedBlockNumber()
        {
            return BlockNumber;
        }
    } 
}
