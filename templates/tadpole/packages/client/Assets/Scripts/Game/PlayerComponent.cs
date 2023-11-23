using System.Collections;
using System.Collections.Generic;
using mud;
using mudworld;
using UnityEngine;

public class PlayerComponent : MUDComponent
{
    public static System.Action OnLocalPlayerSpawned;
    public static PlayerComponent LocalPlayer;
    public bool IsLocalPlayer;
    protected override void UpdateComponent(MUDTable update, UpdateInfo info) {
        PlayerTable table = (PlayerTable)update;

        if(info.UpdateType == UpdateType.DeleteRecord || table.Value == false) {
            //player died
        }

        if(LocalPlayer == null && Entity.Key == NetworkManager.LocalKey) {
            LocalPlayer = this;
            IsLocalPlayer = true;
            OnLocalPlayerSpawned?.Invoke();
        }
    }
}
