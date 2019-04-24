using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavalryController : SoilderController
{
    //骑兵
    protected override void Awake()
    {
        base.Awake();

        Speed = 0.15f;
        HealthMax = 15f;
        Attack = 2;
        AttackDistance = 1f;
    }
}
