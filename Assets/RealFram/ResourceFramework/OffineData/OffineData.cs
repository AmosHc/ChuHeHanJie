using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffineData : MonoBehaviour
{

    public Rigidbody m_Rigidbody;//刚体
    public Collider m_Collider;//碰撞体
    public Transform[] m_AllPoint;//全部的子物体
    public int[] m_AllPointChildCount;//每个节点下子物体个数
    public bool[] m_AllPointActive;//子物体禁用信息
    public Vector3[] m_Pos;//位置
    public Vector3[] m_Scale;//缩放
    public Quaternion[] m_Rot;//旋转

    /// <summary>
    /// 还原属性
    /// </summary>
    public virtual void ResetProp()
    {
        for (int i = 0; i < m_AllPoint.Length; i++)
        {
            Transform tempTrs = m_AllPoint[i];
            if (tempTrs != null)
            {
                //transform还原
                tempTrs.localPosition = m_Pos[i];
                tempTrs.localScale = m_Scale[i];
                tempTrs.localRotation = m_Rot[i];
                if (m_AllPointActive[i])
                {
                    if (!tempTrs.gameObject.activeSelf)
                        tempTrs.gameObject.SetActive(true);//如果是false设置为true，如果为false不改变，节约资源
                }
                else
                {
                    if (tempTrs.gameObject.activeSelf)
                        tempTrs.gameObject.SetActive(false);
                }
                if (tempTrs.childCount > m_AllPointChildCount[i] )
                {
                    for (int j = m_AllPointChildCount[i]; j < tempTrs.childCount; j++)
                    {
                        GameObject tempObj = tempTrs.GetChild(i).gameObject;
                        if (!ObjectManger.Instance.IsObjectManagerCreat(tempObj))
                        {
                            Destroy(tempObj);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 编辑器下保存初始数据
    /// </summary>
    public virtual void BindData()
    {
        m_Rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);//true代表不活动的也要获取到
        m_Collider = gameObject.GetComponentInChildren<Collider>(true);
        m_AllPoint = gameObject.GetComponentsInChildren<Transform>(true);
        m_AllPointChildCount = new int[m_AllPoint.Length];
        m_AllPointActive = new bool[m_AllPoint.Length];
        m_Pos = new Vector3[m_AllPoint.Length];//位置
        m_Scale = new Vector3[m_AllPoint.Length];//位置
        m_Rot = new Quaternion[m_AllPoint.Length];//位置
        for (int i = 0; i < m_AllPoint.Length; i++)
        {
            Transform tempTrs = m_AllPoint[i] as Transform;
            m_AllPointChildCount[i] = tempTrs.childCount;//子物体数量
            m_AllPointActive[i] = tempTrs.gameObject.activeSelf;//是否激活
            m_Pos[i] = tempTrs.localPosition;
            m_Scale[i] = tempTrs.localScale;
            m_Rot[i] = tempTrs.localRotation;
        }
    }
}
