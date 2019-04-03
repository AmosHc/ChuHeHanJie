using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowmenController : SoilderController
{
    protected override void Awake()
    {
        Speed = 0.1f;
        HealthMax = 5f;
        Attack = 1;

        base.Awake();
    }
}
