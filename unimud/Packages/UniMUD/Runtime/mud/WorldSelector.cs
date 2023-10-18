using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mud {

    public class WorldSelector : MonoBehaviour
    {
        [Header("Settings")]
        public string worldFilePath = "worlds";

        [Header("Debug")]
        public string worldAddress;
        public int blockNumber;
        public JObject worldJson;

        void Awake() {
            TextAsset json = Resources.Load<TextAsset>(worldFilePath);
            if(json == null) {Debug.LogError("Could not find worlds.json in Resources"); return;}
            worldJson = JObject.Parse(json?.text);
        }

        public int LoadBlockNumber(NetworkData network) {

            // Specify the path to your JSON file
            string blockString = worldJson[network.chainId.ToString()]["blockNumber"]?.ToString();

            if(string.IsNullOrEmpty(blockString)) {
                blockNumber = 0;
            } else {
                blockNumber = int.Parse(blockString);
            }
                
            return blockNumber;

        }

        public string LoadWorldAddress(NetworkData network) {
            worldAddress = worldJson[network.chainId.ToString()]["address"].ToString();
            return worldAddress;
        }

    }
}
