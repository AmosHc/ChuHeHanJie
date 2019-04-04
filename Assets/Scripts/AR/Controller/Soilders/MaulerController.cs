using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaulerController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.5f;
        HealthMax = 2f;
        Attack = 3;
        AttackDistance = 3f;

        base.Awake();
    }
}
