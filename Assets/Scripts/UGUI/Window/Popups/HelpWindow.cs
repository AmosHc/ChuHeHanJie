using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpWindow : BaseWindow {

    private HelpPanel m_MainPanel;
    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<HelpPanel>();
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);
    }

    private void OnClickCloseBtn()
    {
        UIManager.Instance.CloseWindow(this);
    }
}
