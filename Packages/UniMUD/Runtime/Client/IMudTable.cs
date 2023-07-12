#nullable enable

namespace mud.Client
{
    public abstract class IMudTable {
        public IMudTable(){}
        public abstract void SetValues(params object[] functionParameters);
        public abstract object[] GetValues();
        public abstract IMudTable? GetTableValue(string key);

    }
}
