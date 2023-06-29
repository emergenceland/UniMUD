using Newtonsoft.Json;
using NLog;

namespace mud.Client
{
    public static class NetworkUpdates
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void ApplyNetworkUpdates(Network.Types.NetworkTableUpdate update, Datastore dataStore)
        {
            // TODO: handle LastEventInTx
            if (!dataStore.tableIds.TryGetValue(update.Component, out var componentTableId))
            {
                Logger.Warn($"Unknown component: {JsonConvert.SerializeObject(update.Component)}");
                return;
            }
            

            if (update.PartialValue != null)
            {
                Logger.Debug("UpdateValue " + JsonConvert.SerializeObject(update));
                dataStore.Update(componentTableId, update.Entity.Value, update.PartialValue, update.InitialValue);
            }
            else if (update.Value == null)
            {
                Logger.Debug("DeleteValue " + update.Component);
                dataStore.Delete(componentTableId, update.Entity.Value);
            }
            else
            {
                Logger.Debug("Set value " + JsonConvert.SerializeObject(update));
                dataStore.Set(componentTableId, update.Entity.Value, update.Value);
            }

            // if (update.BlockNumber % 100 == 0)
            // {
            // 	dataStore.Save();
            // }
        }
    }
}
