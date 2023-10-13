using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using IWorld.ContractDefinition;
using mud;
using Newtonsoft.Json;
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
        _counterSub = CounterTable.GetUpdates().ObserveOnMainThread().Subscribe(OnIncrement);
    }


    private void OnIncrement(CounterTable.CounterTableUpdate update)
    {
        Debug.Log("UDPATE: " + JsonConvert.SerializeObject(update));
        if (update.Type != UpdateType.DeleteRecord)
        {
            var currentValue = update.Value;
            if (currentValue == null) return;
            Debug.Log("Counter is now: " + currentValue);
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
