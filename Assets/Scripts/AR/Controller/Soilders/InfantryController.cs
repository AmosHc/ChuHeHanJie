using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfantryController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.2f;
        HealthMax = 4f;
        Attack = 2;

        base.Awake();
    }
}
