using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using mud;
using mudworld;
using UniRx;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;
using IWorld.ContractDefinition;
using System.Text;
using Nethereum.RPC.TransactionTypes;
using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.RPC.TransactionTypes;
using Nethereum.Web3;
using Newtonsoft.Json;
using Property = System.Collections.Generic.Dictionary<string, object>;

public class TestManager : MonoBehaviour
{
    public static System.Action OnInitialized;
    public static bool TestInitialized = false; 
    public GameObject prefab;
    private NetworkManager net;

    Dictionary<string, BallTest> dictionary;
    int spheres;
    private IDisposable ballSub;

    // Start is called before the first frame update
    async void Start()
    {
        dictionary = new Dictionary<string, BallTest>();
        TestInitialized = false;
        net = NetworkManager.Instance;
        net.OnNetworkInitialized += StartTest;

    }

    void OnDestroy() {
        TestInitialized = false;
        ballSub?.Dispose();
        dictionary = null;
    }


    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            SpawnBall();
        }
    }

    private async void StartTest(NetworkManager _)
    {

        ballSub = IMudTable.GetUpdates<PositionTable>().ObserveOnMainThread().Subscribe(UpdatePosition);

        // var query = new Query().In(StateTable.ID);
        // ballSub = ObservableExtensions.Subscribe(net.ds.RxQuery(query).ObserveOnMainThread(), OnState);

        await UpdateAllBalls();

    }   

    async UniTask UpdateAllBalls() {
        while(true) {
            await SendAllUpdate<UpdateAllBallsFunction>();
            await UniTask.Delay(1000);
        }
    }

    private async UniTask SpawnBall() {

        await NetworkManager.Instance.world.Write<SpawnBallFunction>();

        TestInitialized = true;
        OnInitialized?.Invoke();

        Debug.Log("Initialized");
    }

    void UpdatePosition(RecordUpdate update) {
        
        Debug.Log($"Update: {JsonConvert.SerializeObject(update)}");

        //process the table event to a key and the entity of that key
        string entityKey = update.CurrentRecordKey;

        var currentValue = update.CurrentRecordValue;
        if (currentValue == null) Debug.Log("Empty record", this);
        
        Property p = (Property)update.CurrentRecordValue;
        PositionTable positionTable = (PositionTable)Activator.CreateInstance(typeof(PositionTable));
        positionTable.PropertyToTable(p);

        BallTest ball = GetBall(positionTable.Key);

        Vector3 newPosition = new Vector3(System.Convert.ToInt32(positionTable.X), System.Convert.ToInt32(positionTable.Y), 0f);
        bool hasWaited = (bool)positionTable.HasWaited;
        ball.UpdatePosition(newPosition, hasWaited, update.Type);
    
    }


    private void OnState((List<RxRecord> SetRecords, List<RxRecord> RemovedRecords) update)
    {
        Debug.Log("--TABLE UPDATE--");
        UpdateBall(update.SetRecords, false);
        UpdateBall(update.RemovedRecords, true);
    }



    void UpdateBall(List<RxRecord> records, bool isDelete = false) {

        //  // first element of tuple is set records, second is deleted records
        // foreach (var record in records) {

        //     var currentValue = record.value;
        //     if (currentValue == null) Debug.Log("Empty record", this);

        //     BallTest ball = GetBall(record.key);
                    
        //     ball = dictionary[record.key];

        //     StateType newState = (StateType)((UInt64)currentValue["value"]);

        //     ball.UpdateState(newState, isDelete ? UpdateType.DeleteRecord : UpdateType.SetRecord);

        // }

    }

    BallTest GetBall(string key) {

        if (dictionary.ContainsKey(key)) { return dictionary[key]; }

        // Debug.Log("Adding : " + key);
        BallTest ball = (Instantiate(prefab, Vector3.zero - Vector3.right + Vector3.up * spheres, Quaternion.identity) as GameObject).GetComponent<BallTest>();
        ball.name = "Ball " + key;
        ball.key = key;
        ball.keyBytes32 = "0x" + ball.key.Replace("0x", "").PadLeft(64, '0').ToLower();

        dictionary.Add(key, ball);
        spheres++;

        return ball;

    }

    
    
    public async UniTask<bool> SendAllUpdate<TFunction>() where TFunction : FunctionMessage, new() 
    {
        bool success = false;

        while(!success) {

            try {
                success = await NetworkManager.Instance.world.Write<TFunction>();
            }
            catch (System.Exception ex) {
                // Handle your exception here
                Debug.LogException(ex, this);
                success = false;
            }

            if(success == false)
                await UniTask.Delay(1000);

        }

        return success;

    }


}
