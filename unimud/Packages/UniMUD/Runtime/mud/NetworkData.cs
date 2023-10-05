using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkData", menuName = "MUD/NetworkData", order = 1)]
public class NetworkData : ScriptableObject
{
    public string jsonRpcUrl = "http://localhost:8545";
    public string wsRpcUrl = "ws://localhost:8545";
    public string faucetUrl;
    public string contractAddress;
    public string pk;
    public int chainId = 31337;
    public bool disableCache = true;
}
