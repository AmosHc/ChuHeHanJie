using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowmenController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.2f;
        HealthMax = 6f;
        Attack = 1;
        AttackDistance = 2f;

        base.Awake();
    }
}
