using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkData", menuName = "MUD/NetworkData", order = 1)]
public class NetworkData : ScriptableObject
{
    public string jsonRpcUrl = "http://localhost:8545";
    public string wsRpcUrl = "ws://localhost:8545";
    public string indexerUrl = "http://localhost:3001";
    public string faucetUrl;
    public string contractAddress;
    public int chainId = 31337;
}
