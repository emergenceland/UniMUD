using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mud;
using mudworld;

public class PositionComponent : MUDComponent
{
    
    public Vector3 position;
    
    protected override void UpdateComponent(MUDTable table, UpdateInfo updateInfo) {
        PositionTable update = table as PositionTable;
        position = new Vector3((int)update.X, 0f, (int)update.Y);
        transform.position = position;
    }

}
