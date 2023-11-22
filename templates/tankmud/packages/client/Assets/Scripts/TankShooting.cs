using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using mud;
using UnityEngine;

public class TankShooting : MonoBehaviour
{
	public GameObject shell;
	public Transform _tankTurret;
	public GameObject _moveIndicator;
	public GameObject _attackIndicator;
	public bool IsAttacking => _attackIndicator.activeInHierarchy;
	private Camera _camera;

	// Start is called before the first frame update
	void Start() {
		_camera = Camera.main;
		_attackIndicator.SetActive(false);
	}

	// Update is called once per frame
	void Update() {

		var ray = _camera.ScreenPointToRay(Input.mousePosition);
		if (!Physics.Raycast(ray, out var hit)) return;

		var dest = new Vector3(Mathf.RoundToInt(hit.point.x), 0, Mathf.RoundToInt(hit.point.z));

		//Turret rotation
		if(dest != transform.parent.position) {
        	var turretRotation = Quaternion.LookRotation(dest - transform.parent.position);
        	_tankTurret.rotation = Quaternion.Lerp(_tankTurret.rotation, turretRotation, Time.deltaTime * 20f);
		}

		transform.position = dest;

		bool attackInput = Input.GetKey(KeyCode.E);

		_attackIndicator.SetActive(attackInput);
		_moveIndicator.SetActive(!attackInput);

		if (Input.GetKeyDown(KeyCode.Space)) {
			Fire();
		}
	}

	public void Fire() {
        var initialShellPosition = transform.position;
        initialShellPosition.y += 3f;
        Instantiate(shell, initialShellPosition, Quaternion.LookRotation(Vector3.down));
		SendFireTxAsync((int)transform.position.x, (int)transform.position.z).Forget();
    }


	private async UniTaskVoid SendFireTxAsync(int x, int y) {
		try {
			await TxManager.SendDirect<AttackFunction>(System.Convert.ToInt32(x), System.Convert.ToInt32(y));
		}
		catch (System.Exception ex)
		{
			Debug.LogException(ex);
		}
	}
}
