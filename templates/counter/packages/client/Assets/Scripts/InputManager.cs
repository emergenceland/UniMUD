using System;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using mud;
using mudworld;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class InputManager : MonoBehaviour
{
    private IDisposable _counterSub;
    private IDisposable _positionSub;
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
        Debug.Log("Subscribed to Tables.");
        _counterSub = MUDTable.GetUpdates<CounterTable>().ObserveOnMainThread().Subscribe(OnIncrement);
        _positionSub = MUDTable.GetUpdates<PositionTable>().ObserveOnMainThread().Subscribe(OnPositionChange);
    }
    private void OnIncrement(RecordUpdate update)
    {
        if (update.Type != UpdateType.DeleteRecord)
        {
            var currentValue = update.CurrentRecordValue;
            if (currentValue == null) return;
            // Debug.Log("Counter is now: " + currentValue["value"]);
            Debug.Log("Counter is now: " + JsonConvert.SerializeObject(currentValue));
            Instantiate(prefab, Vector3.up, Quaternion.identity); 
        }
    }

    private void OnPositionChange(RecordUpdate update)
    {
        PositionTable.PositionTableUpdate posUpdate = (PositionTable.PositionTableUpdate)update;
        if (posUpdate.Type != UpdateType.DeleteRecord && posUpdate.Table.Name == PositionTable.ID)
        {
            Debug.Log($"Position changed: {posUpdate.X}, {posUpdate.Y}");
        }
    }
    
    private void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            SendIncrementTxAsync().Forget();
        } else if (Input.GetMouseButtonDown(1)){
            var randomX = Random.Range(-10, 10);
            var randomZ = Random.Range(-10, 10);
            SendMoveTx(randomX, randomZ).Forget();
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
    
    private async UniTask SendMoveTx(int x, int y)
    {
        try
        {
            await net.world.Write<MoveFunction>(x, y);
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
        _positionSub?.Dispose();
    }
}
