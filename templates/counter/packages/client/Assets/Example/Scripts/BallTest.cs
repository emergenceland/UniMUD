using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using mud;
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

public enum StateType
{
    Normal,
    Happy,
    Sad,
    Count
}

public class BallTest : MonoBehaviour
{

    public const int txDelay = 2000;

    public bool log = false; 
    public int ballIndex;
    public TextMesh text;
    public string key, keyBytes32;

    

    [Header("Position Test")]
    public bool hasUpdatedPosition = false;
    public GameObject error;
    public Vector3 expectedPos;
    public Vector3 currentPos;
    public UpdateType expectedType;
    public UpdateType currentType;    
    public bool expectedWaited = false;
    public bool hasWaited = false;

    float moveTime = 0f;
    bool txSending = false;
    bool recievedUpdate = false;


    [Header("State Test")]
    public StateType state;
    public StateType transactionState;

    [Header("Ball")]
    public Material[] materials;
    public Material setRecord, deleteRecord, sameValue;
    public MeshRenderer mr;



    public void UpdatePosition(Vector3 newPosition, bool newHasWaited, UpdateType newUpdateType) {

        ballIndex = (int)newPosition.y;

        if(newUpdateType == UpdateType.DeleteRecord) {
            mr.sharedMaterial = deleteRecord;
        } else {
            if (transform.position == newPosition) { mr.sharedMaterial = sameValue; }
            else { mr.sharedMaterial = setRecord; }
        }

        if(log) Debug.Log("Updated " + Describe(newPosition, newUpdateType), this);
        if(log) Debug.Log("Expected " + Describe(expectedPos, expectedType), this);

        transform.position = newPosition;
        hasWaited = newHasWaited;
        currentType = newUpdateType;

        text.text = newUpdateType.ToString();

        if(hasUpdatedPosition) {
            // Debug.Assert(newState == transactionState);
            error.SetActive(expectedPos != newPosition || expectedType != newUpdateType || expectedWaited != hasWaited) ;
            Debug.Assert(expectedPos == newPosition, "--POS MISMATCH--", this);
            Debug.Assert(expectedWaited == hasWaited, "--WAIT MISMATCH--", this);
            Debug.Assert(expectedType == newUpdateType, "--TYPE MISMATCH--", this);

        } else {
            error.SetActive(false);
        }

        recievedUpdate = true;
        hasUpdatedPosition = true;

        SetExpected();

    }


    void SetExpected() {
        
        currentPos = transform.position;

        if(currentPos.x == 2 && hasWaited == false) {
            expectedPos = currentPos;
            expectedType = UpdateType.SetRecord;
            expectedWaited = true;
        } else if(currentPos.x == 3) {
            expectedPos = currentType == UpdateType.SetRecord ? transform.position : Vector3.right + Vector3.up * ballIndex;
            expectedType = currentType == UpdateType.SetRecord ? UpdateType.DeleteRecord : UpdateType.SetRecord;
            expectedWaited = false;
        } else {
            expectedPos = currentPos + Vector3.right;
            expectedType = UpdateType.SetRecord;
            expectedWaited = false;
        }

        recievedUpdate = false; 
    }

    public string Describe(Vector3 pos, UpdateType updateType) {
        return "Pos: " + ((int)pos.x).ToString() + "," + ((int)pos.y).ToString() + " Type: " + updateType.ToString();
    }























    public void UpdateState(StateType newState, UpdateType updateType) {

        if(log) Debug.Log("Updated " + ballIndex + " " + updateType.ToString(), this);

        state = newState;
        mr.sharedMaterial = materials[(int)newState];

        if(TestManager.TestInitialized) {
            // Debug.Assert(newState == transactionState);
            Debug.Assert(expectedType == updateType, "[Actual: " + updateType.ToString() + "]", this);
        }

        text.text = newState.ToString() + "\n" + updateType.ToString();
    }   

    void Update() {
        
        if(txSending) {
            return;
        }

        if(ballIndex != 0) {
            return;
        }

        // if(Input.GetKeyDown(KeyCode.Alpha1)) {
        //     TestSingle(0);
        // } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
        //     TestSingle(1);
        // } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
        //     TestSingle(2);
        // } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
        //     TestSingle(3);
        // } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
        //     TestSingle(4);
        // }
    }

    // public async UniTask TestSingle(int function)
    // {
      
    //     if (function == 0)
    //     {
    //         transactionState = (StateType)(((int)state + 1) % (int)StateType.Count);
    //         expectedType = UpdateType.SetRecord;
    //         if(log) Debug.Log("[SET]");
    //         if(log) Debug.Log("[Expected: " + expectedType.ToString() + "]");
    //         await RunTxUntilItPasses<SetSimpleFunction>();
    //     }
    //     else if (function == 1)
    //     {
    //         transactionState = (StateType)(((int)state + 1) % (int)StateType.Count);
    //         expectedType = UpdateType.DeleteRecord;
    //         if(log) Debug.Log("[DELETE]");
    //         if(log) Debug.Log("[Expected: " + expectedType.ToString() + "]");
    //         await RunTxUntilItPasses<DeleteSimpleFunction>();
    //     }
    //     else if (function == 2)
    //     {
    //         transactionState = (StateType)(((int)state + 1) % (int)StateType.Count);
    //         expectedType = UpdateType.SetRecord;
    //         if(log) Debug.Log("[DELETESET]");
    //         if(log) Debug.Log("[Expected: " + expectedType.ToString() + "]");
    //         await RunTxUntilItPasses<DeleteSetFunction>();
    //     }
    //     else if (function == 3)
    //     {
    //         transactionState = (StateType)(((int)state + 1) % (int)StateType.Count);
    //         expectedType = UpdateType.DeleteRecord;
    //         if(log) Debug.Log("[SETDELETE]");
    //         if(log) Debug.Log("[Expected: " + expectedType.ToString() + "]");
    //         await RunTxUntilItPasses<SetDeleteFunction>();
    //     } 

        
    // }


    static byte[] HexStringToBytes(string hexString)
    {
        int length = hexString.Length;
        byte[] byteArray = new byte[length / 2];

        for (int i = 0; i < length; i += 2)
        {
            byteArray[i / 2] = System.Convert.ToByte(hexString.Substring(i, 2), 16);
        }

        return byteArray;
    }


    public async UniTask RunTxUntilItPasses<TFunction>() where TFunction : FunctionMessage, new() {
        while (await SendTx<TFunction>() == false){ await UniTask.Delay(txDelay);}

    }

    
    public async UniTask<bool> SendTx<TFunction>() where TFunction : FunctionMessage, new() 
    {
        if(log) Debug.Log("Sending", this);
        byte[] byteHexArray = HexStringToBytes(key.Replace("0x", ""));

        try {
            return await NetworkManager.Instance.world.Write<TFunction>(byteHexArray);
        }
        catch (System.Exception ex) {
            // Handle your exception here
            if(log) Debug.LogException(ex, this);
            return false;
        }
    }


}
