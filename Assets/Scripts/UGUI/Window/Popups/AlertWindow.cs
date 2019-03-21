using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertWindow : Window
{
    private AlertPanel m_MainPanel;
    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<AlertPanel>();
        m_MainPanel.TitleTxt.text = paramList[0] as string;
        m_MainPanel.ContentTxt.text = paramList[1] as string;
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);
    }

    private void OnClickCloseBtn()
    {
        UIManager.Instance.CloseWindow(this);
    }
}
