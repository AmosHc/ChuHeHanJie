using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI离线数据
/// </summary>
public class UIOffineData : OffineData
{
    public Vector2[] m_AnchorMax;
    public Vector2[] m_AnchorMin;
    public Vector2[] m_Pivot;
    public Vector2[] m_SizeDelta;
    public Vector3[] m_AnchoredPos;
    public ParticleSystem[] m_Particle;
    /// <summary>
    /// 还原数据
    /// </summary>
    public override void ResetProp()
    {
        int allPointCount = m_AllPoint.Length;
        for (int i = 0; i < allPointCount; i++)
        {
            RectTransform rescRectTransform = m_AllPoint[i] as RectTransform;
            if (rescRectTransform != null)
            {
                if (m_AllPointActive[i])
                {
                    if (!rescRectTransform.gameObject.activeSelf)
                        rescRectTransform.gameObject.SetActive(true);//如果是false设置为true，如果为false不改变，节约资源
                }
                else
                {
                    if (rescRectTransform.gameObject.activeSelf)
                        rescRectTransform.gameObject.SetActive(false);
                }
                rescRectTransform.anchorMax = m_AnchorMax[i];
                rescRectTransform.anchorMin = m_AnchorMin[i];
                rescRectTransform.localPosition = m_Pos[i];
                rescRectTransform.localRotation = m_Rot[i];
                rescRectTransform.localScale = m_Scale[i];
                rescRectTransform.pivot = m_Pivot[i];
                rescRectTransform.sizeDelta = m_SizeDelta[i];
                rescRectTransform.anchoredPosition3D = m_AnchoredPos[i];
            }
        }
        //粒子系统重置
        for (int i = 0; i < m_Particle.Length; i++)
        {
            m_Particle[i].Clear(true);
            m_Particle[i].Play();
        }

    }
    /// <summary>
    /// 绑定数据
    /// </summary>
    public override void BindData()
    {
        Transform[] AllTrs = gameObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < AllTrs.Length; i++)
        {
            if (!(AllTrs[i] is RectTransform))
                AllTrs[i].gameObject.AddComponent<RectTransform>();
        }

        m_AllPoint = gameObject.GetComponentsInChildren<RectTransform>();
        m_Particle = gameObject.GetComponentsInChildren<ParticleSystem>();//粒子系统
        int allPointCount = m_AllPoint.Length;
        m_AllPointChildCount = new int[allPointCount];
        m_AllPointActive = new bool[allPointCount];
        m_Pos = new Vector3[allPointCount];
        m_Rot = new Quaternion[allPointCount];
        m_Scale = new Vector3[allPointCount];
        m_Pivot = new Vector2[allPointCount];
        m_AnchorMax = new Vector2[allPointCount];
        m_AnchorMin = new Vector2[allPointCount];
        m_SizeDelta = new Vector2[allPointCount];
        m_AnchoredPos = new Vector3[allPointCount];
        for (int i = 0; i < allPointCount; i++)
        {
            RectTransform tempReTrs = m_AllPoint[i] as RectTransform;
            m_AllPointChildCount[i] = tempReTrs.childCount;
            m_AllPointActive[i] = tempReTrs.gameObject.activeSelf;
            m_Pos[i] = tempReTrs.localPosition;
            m_Rot[i] = tempReTrs.localRotation;
            m_Scale[i] = tempReTrs.localScale;
            m_Pivot[i] = tempReTrs.pivot;
            m_AnchorMax[i] = tempReTrs.anchorMax;
            m_AnchorMin[i] = tempReTrs.anchorMin;
            m_SizeDelta[i] = tempReTrs.sizeDelta;
            m_AnchoredPos[i] = tempReTrs.anchoredPosition3D;
        }

    }
}
