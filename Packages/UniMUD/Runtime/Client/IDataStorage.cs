using System.Collections.Generic;

namespace mud.Client
{
    public interface IDataStorage
    {
        void Write(IEnumerable<Record> store);
        IEnumerable<Record> Load();
        int BlockNumber { get; set; }
        int GetCachedBlockNumber();
    }
}
