using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;

public class HUDWindow : BaseWindow
{
    public HUDPanel m_MainPanel;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.transform.GetComponent<HUDPanel>();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnShow(params object[] paramList)
    {
        base.OnShow(paramList);
    }

    //设置阵营血量
    public void SetHp(int camp, float value,float total)
    {
        float pointer = Mathf.Round(value / total);//bar进度
        switch (camp)
        {
            case ConStr.campRed:
                m_MainPanel.RedHpText.text = "红方剩余血量："+value;
                m_MainPanel.RedHpBar.value = pointer;
                break;
            case ConStr.campBlue:
                m_MainPanel.BlueHpText.text = "蓝方剩余血量：" + value;
                m_MainPanel.BlueHpBar.value = pointer;
                break;
            default:
                Debug.Log("阵营不规范："+camp);
                break;
        }
    }
}
