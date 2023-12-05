using System.Collections;
using System.Collections.Generic;
using mud;
using mudworld;
using UnityEngine;

public class ToadComponent : MUDVisibility
{

    protected override void PostInit() {
        transform.position = Entity.GetMUDComponent<MUDPosition>().position;
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>(true);
        ParticleSystemRenderer r = ps.GetComponentInChildren<ParticleSystemRenderer>();
        r.material = GetComponentInChildren<Renderer>().material;
    }

    protected override void UpdateComponent(MUDTable update, UpdateInfo info) {
        base.UpdateComponent(update, info);
    }
}
