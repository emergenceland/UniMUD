using System.Collections.Generic;

namespace mud.Client
{
    public interface IDataStorage
    {
        void Write(IEnumerable<RxRecord> store);
        IEnumerable<RxRecord> Load();
        int BlockNumber { get; set; }
        int GetCachedBlockNumber();
    }
}
