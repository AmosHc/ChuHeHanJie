using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowmenController : SoilderController
{
    //弓箭手参数
    protected override void Awake()
    {
        base.Awake();

        Speed = 0.1f;//移动速度
        HealthMax = 10f;//血量
        Attack = 2;//伤害
        AttackDistance = 3f;//攻击距离
    }
}
