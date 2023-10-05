using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using mud;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class InputManager : MonoBehaviour
{
    private IDisposable _counterSub;
    private NetworkManager net;
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        net = NetworkManager.Instance;
        net.OnNetworkInitialized += SubscribeToCounter;
    }
    
    private void SubscribeToCounter(NetworkManager _)
    {
        Debug.Log("Subscribed to counter.");
        var incrementQuery = new Query().In(net.ds.tableNameIndex["Counter"]);
        _counterSub = net.ds.RxQuery(incrementQuery).ObserveOnMainThread().Subscribe(OnIncrement);
    }


    private void OnIncrement((List<RxRecord> SetRecords, List<RxRecord> RemovedRecords) update)
    {
        // first element of tuple is set records, second is deleted records
        foreach (var record in update.SetRecords)
        {
            var currentValue = record.value;
            if (currentValue == null) return;
            Debug.Log("Counter is now: " + currentValue["value"]);
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        var randomX = Random.Range(-1, 1);
        var randomZ = Random.Range(-1, 1);
        Instantiate(prefab, new Vector3(randomX, 5, randomZ), Quaternion.identity);
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
            await net.world.Write<IncrementFunction>();
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
