using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mud;
using mudworld;
using IWorld.ContractDefinition;

public class GameManager : MonoBehaviour
{

    public GameObject spawnPointer;

    void Start() {
        NetworkManager.OnInitialized += SpawnPlayer;
        PlayerComponent.OnLocalPlayerSpawned += StartGame;
    }

    void SpawnPlayer() {
        PlayerTable player = MUDTable.GetTable<PlayerTable>(NetworkManager.LocalKey);
        if(player == null || player.Value == false) {
            Debug.Log("Spawning player");
            SendSpawnTx();
        } else {
            Debug.Log("Player found");
        }
    }

    async void SendSpawnTx() {
        await TxManager.SendUntilPasses<SpawnPlayerFunction>();
    }

    void StartGame() {
        //setup input
        Debug.Log("Game started");
        GameManagerTable game = MUDTable.GetTable<GameManagerTable>("0x");
        SpawnToad(new Vector3(0, (int)game.Tadpoles, 0));
    }

    async void SpawnToad(Vector3 pos) {
        await TxManager.SendDirect<SpawnToadFunction>(System.Convert.ToInt32(pos.x), System.Convert.ToInt32(pos.y), System.Convert.ToInt32(pos.z));
    }
    
    async void DeleteToad(Vector3 pos) {
        await TxManager.SendDirect<DeleteToadFunction>(System.Convert.ToInt32(pos.x), System.Convert.ToInt32(pos.y), System.Convert.ToInt32(pos.z));
    }


    void Update() {
        if(PlayerComponent.LocalPlayer == null) {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        if (Physics.Raycast (ray, out var hit, 100)) {

            Vector3 spawnPos = hit.point + hit.normal * .33f;
            spawnPos = new Vector3(Mathf.RoundToInt(spawnPos.x),Mathf.RoundToInt(spawnPos.y),Mathf.RoundToInt(spawnPos.z));
            spawnPointer.transform.position = spawnPos;

            if(Input.GetMouseButtonDown(0)) {
                SpawnToad(spawnPos);
            } else if(Input.GetMouseButtonDown(1)) {
                spawnPos -= hit.normal * .66f;
                spawnPos = new Vector3(Mathf.RoundToInt(spawnPos.x),Mathf.RoundToInt(spawnPos.y),Mathf.RoundToInt(spawnPos.z));
                DeleteToad(spawnPos);
            }
        }

    }
}
