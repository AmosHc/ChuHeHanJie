using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryController : SoilderController
{
    //步兵
    protected override void Awake()
    {
        Speed = 0.1f;
        HealthMax = 15f;
        Attack = 2;
        AttackDistance = 1f;

        base.Awake();
    }
}
