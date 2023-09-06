using UnityEngine;
using System.Runtime.InteropServices;
using System;
using AOT;


public class BlockchainInterop
{
    [DllImport("__Internal")]
    private static extern void BlockchainInterop_Connect(string rpcUrl);

    [DllImport("__Internal")]
    private static extern void BlockchainInterop_Subscribe(string type);

    [DllImport("__Internal")]
    private static extern void BlockchainInterop_SetOnMessageCallback(Action<string> callback);

    [DllImport("__Internal")]
    private static extern void BlockchainInterop_SetOnErrorCallback(Action<string> callback);

    [DllImport("__Internal")]
    private static extern void BlockchainInterop_SetOnCloseCallback(Action<string> callback);

    [DllImport("__Internal")]
    private static extern void BlockchainInterop_SetOnOpenCallback(Action callback);

    [DllImport("__Internal")]
    private static extern void BlockchainInterop_Close();

    public string rpcWebSocketUrl = "ws://localhost:8545";

    // private void Start()
    // {
    //     ConnectToBlockchain();
    //     SetCallbacks();
    // }

    public void ConnectToBlockchain()
    {
        BlockchainInterop_Connect(rpcWebSocketUrl);
    }

    public void SetCallbacks()
    {
        BlockchainInterop_SetOnMessageCallback(OnMessageReceived);
        BlockchainInterop_SetOnErrorCallback(OnErrorReceived);
        BlockchainInterop_SetOnCloseCallback(OnConnectionClosed);
        BlockchainInterop_SetOnOpenCallback(OnBlockchainConnected);
    }


    // Callback Handlers
    [AOT.MonoPInvokeCallback(typeof(Action<string>))]
    private static void OnMessageReceived(string message)
    {
        Debug.Log("Received from Blockchain: " + message);
        // Handle the incoming message, parse JSON, etc.
    }

    [AOT.MonoPInvokeCallback(typeof(Action<string>))]
    private static void OnErrorReceived(string error)
    {
        Debug.LogError("Blockchain Error: " + error);
    }

    [AOT.MonoPInvokeCallback(typeof(Action<string>))]
    private static void OnConnectionClosed(string message)
    {
        Debug.LogWarning("Blockchain Connection Closed: " + message);
        // Perhaps try to reconnect?
    }

    [MonoPInvokeCallback(typeof(Action))]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void OnBlockchainConnected()
    {
        // This will be called when the WebSocket connection is successfully opened.
        BlockchainInterop_Subscribe("newHeads");
    }

    private void OnDestroy()
    {
        BlockchainInterop_Close(); // Close the connection when the game object is destroyed
    }
}
