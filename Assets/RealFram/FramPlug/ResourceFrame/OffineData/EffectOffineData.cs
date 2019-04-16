using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效离线数据
/// </summary>
public class EffectOffineData : OffineData
{
    public ParticleSystem[] particleSystem;
    public TrailRenderer[] trailRenderer;

    public override void BindData()
    {
        base.BindData();
        particleSystem = gameObject.GetComponentsInChildren<ParticleSystem>();
        trailRenderer = gameObject.GetComponentsInChildren<TrailRenderer>();
    }

    public override void ResetProp()
    {
        base.ResetProp();

        for (int i = 0; i < particleSystem.Length; i++)
        {
            particleSystem[i].Clear(true);
            particleSystem[i].Play();
        }

        for (int i = 0; i < trailRenderer.Length; i++)
        {
            trailRenderer[i].Clear();
        }
    }
}
