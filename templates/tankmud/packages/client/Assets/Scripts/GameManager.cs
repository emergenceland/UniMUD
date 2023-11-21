using IWorld.ContractDefinition;
using mud;
using mudworld;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Button spawn;

	void Start() {

		spawn.gameObject.SetActive(false);

		NetworkManager.OnInitialized += UpdateSpawn;
		PlayerComponent.OnPlayerSpawned += SetupLocalPlayer;
	}

	void SetupLocalPlayer() {
		PlayerComponent.LocalPlayer.OnUpdated += UpdateSpawn;
	}

	void UpdateSpawn() {
		var currentPlayer = MUDTable.GetTable<PlayerTable>(NetworkManager.LocalKey);
		spawn.gameObject.SetActive(currentPlayer == null || currentPlayer.Value == false);
	}

	public async void SpawnPlayer() {
		await TxManager.SendDirect<SpawnFunction>(0, 2);
	}

}
