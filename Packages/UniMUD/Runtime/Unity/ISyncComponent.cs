using mud.Client;

namespace mud.Unity
{
    public interface ISyncComponent
    {
        void OnDataStoreUpdate(RecordUpdate update);
    }
}
