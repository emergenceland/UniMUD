using System;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;

namespace mud.Client
{
	public static class NetworkUpdates
	{
		public static void ApplyNetworkUpdates(Types.NetworkTableUpdate update, Datastore dataStore)
		{
			// TODO: handle LastEventInTx
			var tableQuery = new Query().Find("?key").Where("TableId<datastore:DSMetadata>", "?key", "?table", update.Component);
			using var tableExistsResult = dataStore.Query(tableQuery).GetEnumerator();
			if (!tableExistsResult.MoveNext())
			{
				Debug.LogWarning($"Unknown component: {JsonConvert.SerializeObject(update.Component)}");
				return;
			}

			if (update.PartialValue != null)
			{
				Debug.Log("UpdateValue " + JsonConvert.SerializeObject(update));
				dataStore.UpdateValue(update.Component, update.Entity.Value, update.PartialValue, update.InitialValue);
			}
			else if (update.Value == null)
			{
				Debug.Log("DeleteValue " + update.Component);
				dataStore.DeleteValue(update.Component, update.Entity.Value);
			}
			else
			{
				Debug.Log("Set value " + JsonConvert.SerializeObject(update));
				dataStore.SetValue(update.Component, update.Entity.Value, update.Value);
			}

			if (update.BlockNumber % 100 == 0)
			{
				dataStore.Save();
			}
		}
	}
}
