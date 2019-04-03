using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaulerController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.3f;
        HealthMax = 2f;
        Attack = 3;

        base.Awake();
    }
}
