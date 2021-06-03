using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : EntityBase
{

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    public void OnSpawn(AnimationEvent ev)
    {
        anim.enabled = false;
    }
}
