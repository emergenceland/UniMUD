using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UniRx;

namespace mud {

    public enum WorldLocation {ResourcesFolder, URL}
    public class WorldSelector : MonoBehaviour
    {
        [Header("Settings")]
        public WorldLocation loadFrom;
        public string worldFilePath = "worlds";
        public string worldURL = "";

        [Header("Debug")]
        public string worldAddress;
        public int blockNumber;
        public JObject worldJson;

        void Awake() {

        }

        public async UniTask LoadWorldFile() {
            if(loadFrom == WorldLocation.ResourcesFolder) {await LoadFromPath(worldFilePath);}
            else if(loadFrom == WorldLocation.URL) {await LoadFromURL(worldURL);}
        }

        async UniTask LoadFromPath(string newPath) {
            TextAsset json = Resources.Load<TextAsset>(newPath);
            if(json == null) {Debug.LogError($"Could not find {newPath} in Resources"); return;}
            worldJson = JObject.Parse(json?.text);
        }

        async UniTask LoadFromURL(string newURL) {
            string json = await GetRequestAsync(newURL);
            if(json == null) {Debug.LogError($"Could not load from {newURL}"); return;}
            worldJson = JObject.Parse(json);
        }

        public int GetBlockNumber(NetworkData network) {
            
            // Specify the path to your JSON file
            string blockString = worldJson[network.chainId.ToString()]?["blockNumber"]?.ToString();

            if(string.IsNullOrEmpty(blockString)) {
                blockNumber = 0;
            } else {
                blockNumber = int.Parse(blockString);
            }
            
            return blockNumber;

        }

        [CanBeNull]
        public string GetWorldContract(NetworkData network) {
            if(worldJson == null) {Debug.LogError("Not loaded");}

            worldAddress = worldJson[network.chainId.ToString()]?["address"]?.ToString();
            return worldAddress;
        }

        async UniTask<string> GetRequestAsync(string uri) {

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success) {
                    Debug.LogError("Error: " + webRequest.error);
                    return null;
                } else {
                    Debug.Log("Success: " + uri);
                    return webRequest.downloadHandler.text;
                }
            }
        }

    }
}
