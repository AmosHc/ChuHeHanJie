using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.3f;
        HealthMax = 4f;
        Attack = 2;
        AttackDistance = 4f;

        base.Awake();
    }
}
