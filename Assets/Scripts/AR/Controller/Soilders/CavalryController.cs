using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavalryController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.3f;
        HealthMax = 3f;
        Attack = 2;
        AttackDistance = 2f;
        
        base.Awake();
    }
}
