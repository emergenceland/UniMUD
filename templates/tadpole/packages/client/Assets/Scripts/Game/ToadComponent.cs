using System.Collections;
using System.Collections.Generic;
using mud;
using mudworld;
using UnityEngine;

public class ToadComponent : MUDComponent
{

    protected override void PostInit() {
        transform.position = Entity.GetMUDComponent<PositionComponent>().position;
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>(true);
        ParticleSystemRenderer r = ps.GetComponentInChildren<ParticleSystemRenderer>();
        r.material = GetComponentInChildren<Renderer>().material;
    }

    protected override void UpdateComponent(MUDTable update, UpdateInfo info) {
        ToadTable table = (ToadTable)update;
        
        if(info.UpdateType == UpdateType.DeleteRecord || table.Value == false) {
            Entity.Toggle(false);
            //toad was deleted
        }
    }
}
