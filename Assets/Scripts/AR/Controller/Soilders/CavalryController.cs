using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavalryController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.1f;
        HealthMax = 2f;
        Attack = 2;
        
        base.Awake();
    }
}
