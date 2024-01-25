using System;
using System.Numerics;
using Codice.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace mud
{
    public class SyncFilter
    {
        public string tableId;
        public string key0;
        public string key1;
    }

    public class SnapshotRoot
    {
        public SnapshotResult result { get; set; }
    }

    public class SnapshotResult
    {
        public SnapshotData data { get; set; }
    }

    public class SnapshotData
    {
        public SnapshotDataJson json { get; set; }
    }

    public class SnapshotDataJson
    {
        public string blockNumber { get; set; }
        public MudLog[] logs { get; set; }
    }

    public class Snapshot
    {
        public static async UniTask<StorageAdapterBlock?> GetSnapshot(string indexerUrl, int chainId,
            string storeContractAddress)
        {
            if (string.IsNullOrEmpty(indexerUrl)) return null;

            var query = new IndexerQuery
            {
                chainId = chainId,
                address = storeContractAddress,
                filters = new SyncFilter[] { }
            };
            var urlEncoded = HttpUtility.UrlEncode(@"{""0"":{""json"":" +
                                                   JsonConvert.SerializeObject(query) +
                                                   "}}");

            var requestString = $"{indexerUrl}/trpc/getLogs?batch=1&input={urlEncoded}";
            var res = await Common.GetRequestAsyncString(requestString);

            if (string.IsNullOrEmpty(res)) return null;

            try
            {
                var block = JsonConvert.DeserializeObject<SnapshotRoot[]>(res)[0];
                if (block?.result?.data?.json?.blockNumber == null) return null;
                return new StorageAdapterBlock
                {
                    BlockNumber = BigInteger.Parse(block.result.data.json.blockNumber),
                    Logs = block.result.data.json.logs
                };
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }
    }
}
