using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaulerController : SoilderController
{
    //盾兵
    protected override void Awake()
    {
        Speed = 0.05f;
        HealthMax = 20f;
        Attack = 1;
        AttackDistance = 1f;

        base.Awake();
    }
}
