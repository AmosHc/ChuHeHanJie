using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingWindow : Window {

    private SettingPanel m_MainPanel;
    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<SettingPanel>();
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);
    }

    private void OnClickCloseBtn()
    {
        UIManager.Instance.CloseWindow(this);
    }
}
