#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using IWorld.ContractDefinition;
using mud;
using UniRx;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

public class PlayerController : MonoBehaviour
{
    public PlayerComponent player;
    public GameObject _moveMarker;

    private Camera _camera;
    private Vector3 destination;
    float distance;

    private TankShooting _target;
    Quaternion rotation;

    void Start() {

        player.health.transform.parent = transform;
        player.health.transform.localPosition = Vector3.zero;
        player.health.transform.localRotation = Quaternion.identity;
        
        //set initial position
        player.position.OnUpdated += UpdatePosition;
        rotation = Quaternion.identity;
        destination = player.position.position;
        transform.position = destination;

        _moveMarker.SetActive(false);

        _target = GetComponentInChildren<TankShooting>(true);
        _camera = Camera.main;

    }

    void OnEnable() {
        if(player.Loaded) {
            transform.position = player.position.position;
        }
    }

    private void OnDestroy() {
        if(player.position) player.position.OnUpdated -= UpdatePosition;

    }

    void UpdatePosition() {
        destination = player.position.position;   
    }

    // TODO: Send tx
    private async UniTaskVoid SendMoveTxAsync(int x, int y) {
        try {
            await TxManager.SendDirect<MoveFunction>(Convert.ToInt32(x), Convert.ToInt32(y));
        }
        catch (Exception ex) {
            // Handle your exception here
            Debug.LogException(ex);
        }
    }

    void Update() {
        UpdateInput();
        UpdateTank();
    }

    void UpdateInput() {

        if (!player.Loaded || !player.IsLocalPlayer) return;

        if (Input.GetMouseButtonDown(0)) {

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            var dest = new Vector3(Mathf.RoundToInt(hit.point.x), 0, Mathf.RoundToInt(hit.point.z));
            if(dest == transform.position) {return;}

            // Determine the new rotation
            destination = dest;

            distance = Vector3.Distance(transform.position, destination);
            rotation = Quaternion.LookRotation(destination - transform.position);

            _moveMarker.SetActive(true);
            _moveMarker.transform.position = dest;
            _moveMarker.transform.rotation = rotation;

            //send tx
            SendMoveTxAsync(Convert.ToInt32(dest.x), Convert.ToInt32(dest.z)).Forget();

        }
    }

    void UpdateTank() {

        if (Vector3.Distance(transform.position, destination) > 0.02) {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * 1f * distance);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 25f);
    
        } else {
            _moveMarker.SetActive(false);
        }

    }

}