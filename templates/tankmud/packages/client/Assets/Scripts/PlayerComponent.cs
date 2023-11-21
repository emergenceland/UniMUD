using UnityEngine;
using mud;
using mudworld;
using System;

public class PlayerComponent : MUDComponent
{
    public static Action OnPlayerSpawned;
    public static PlayerComponent LocalPlayer;

    public bool IsLocalPlayer;

    protected override void UpdateComponent(MUDTable table, UpdateInfo updateInfo) {

    }

}
