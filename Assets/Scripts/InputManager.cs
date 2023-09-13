using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using UniRx;
using mud.Client;
using mud.Network.schemas;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

public class InputManager : MonoBehaviour
{
    private IDisposable _counterSub;
    private v2.NetworkManager net;

    // Start is called before the first frame update
    void Start()
    {
        net = v2.NetworkManager.Instance;
        net.OnNetworkInitialized += SubscribeToCounter;
    }

    private void SubscribeToCounter(v2.NetworkManager _)
    {
        Debug.Log("Subscribed to counter.");
        var incrementQuery = new Query().In(new TableId("", "Counter"));
        _counterSub = ObservableExtensions.Subscribe(net.ds.RxQuery(incrementQuery).ObserveOnMainThread(), OnIncrement);
    }


    private void OnIncrement((List<Record> SetRecords, List<Record> RemovedRecords) update)
    {
        // first element of tuple is set records, second is deleted records
        foreach (var record in update.SetRecords)
        {
            var currentValue = record.value;
            if (currentValue == null) return;
            Debug.Log("Counter is now: " + currentValue["value"]);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SendIncrementTxAsync().Forget();
        }
    }

    private async UniTask SendIncrementTxAsync()
    {
        try
        {
            Debug.Log(net);
            await net.world.Write<IncrementFunction>();
            // var contractHandler = net._provider.Eth.GetContractTransactionHandler<IncrementFunction>();
            // // Debug.Log(JsonConvert.SerializeObject(contractHandler));
            // // var transactionReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(net.worldAddress);
            // // Debug.Log(JsonConvert.SerializeObject(transactionReceipt));
        }
        catch (Exception ex)
        {
            // Handle your exception here
            Debug.LogException(ex);
        }
    }

    private void OnDestroy()
    {
        _counterSub?.Dispose();
    }
}
