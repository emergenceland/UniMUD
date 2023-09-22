// using Newtonsoft.Json;
// using UnityEngine;
//
// namespace mud.Client
// {
//     public static class NetworkUpdates
//     {
//         public static void ApplyNetworkUpdates(Network.Types.NetworkTableUpdate update, Datastore dataStore)
//         {
//             Debug.Log("yee hee");
//             // TODO: handle LastEventInTx
//             if (!dataStore.tableIds.TryGetValue(update.Component, out var componentTableId))
//             {
//                 Debug.LogWarning($"Unknown component: {JsonConvert.SerializeObject(update.Component)}");
//                 return;
//             }
//             
//             if (update.PartialValue != null)
//             {
//                 Debug.Log("UpdateValue " + JsonConvert.SerializeObject(update));
//                 dataStore.Update(componentTableId, update.Entity.Value, update.PartialValue, update.InitialValue);
//             }
//             else if (update.Value == null)
//             {
//                 Debug.Log("DeleteValue " + update.Component);
//                 dataStore.Delete(componentTableId, update.Entity.Value);
//             }
//             else
//             {
//                 Debug.Log("Set value " + JsonConvert.SerializeObject(update));
//                 dataStore.Set(componentTableId, update.Entity.Value, update.Value);
//             }
//
//             // if (update.BlockNumber % 100 == 0)
//             // {
//             // 	dataStore.Save();
//             // }
//         }
//     }
// }
