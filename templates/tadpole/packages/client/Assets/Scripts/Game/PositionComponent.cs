using System.Collections;
using System.Collections.Generic;
using mud;
using mudworld;
using UnityEngine;

public class PositionComponent : MUDComponent
{

    public Vector3 position;

    protected override void UpdateComponent(MUDTable table, UpdateInfo info) {
        PositionTable update = (PositionTable)table;
        
        if(info.UpdateType == UpdateType.DeleteRecord) {
            //Position was deleted
            Entity.Toggle(false);
        }

        position = new Vector3((int)update.X, (int)update.Y, (int)update.Z);
        transform.position = position;
        
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, Vector3.one * .5f);
    }
}
