using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitWindow : BaseWindow {

    private ExitPanel m_MainPanel;
    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<ExitPanel>();
        AddButtonClickListener(m_MainPanel.NoBtn, OnClickCloseBtn);
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);
        AddButtonClickListener(m_MainPanel.YesBtn, OnClickYesBtn);
    }

    private void OnClickYesBtn()
    {
        Application.Quit();
    }

    private void OnClickCloseBtn()
    {
        UIManager.Instance.CloseWindow(this);
    }
}
