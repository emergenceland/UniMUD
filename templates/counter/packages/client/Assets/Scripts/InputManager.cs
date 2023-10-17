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
        _positionSub = IMudTable.GetUpdates<PositionTable>().ObserveOnMainThread().Subscribe(OnPositionChange);
    }

    private void OnPositionChange(RecordUpdate update)
    {
        PositionTable.PositionTableUpdate posUpdate = (PositionTable.PositionTableUpdate)update;
        if (posUpdate.Type != UpdateType.DeleteRecord && posUpdate.Table.Name == PositionTable.ID)
        {
            Debug.Log($"HEE HEE: {JsonConvert.SerializeObject(posUpdate)}");
            Debug.Log($"Position changed: {posUpdate.X}, {posUpdate.Y}");
        }
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // SendIncrementTxAsync().Forget();
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
    }
}
