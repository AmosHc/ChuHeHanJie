using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenulUi : Window
{
    private MenuPanel m_menuPanel;

    public override void Awake(params object[] paramList)
    {
        m_menuPanel = GameObject.GetComponent<MenuPanel>(); //获得配置panel
        AddButtonClickListener(m_menuPanel.StartButton, OnClickStartBtn);
        AddButtonClickListener(m_menuPanel.LoadButton, OnClickLoadBtn);
        AddButtonClickListener(m_menuPanel.ExitButton, OnClickExitBtn);
    }

    void OnClickStartBtn()
    {
        Debug.Log("开始");
    }

    void OnClickLoadBtn()
    {
        Debug.Log("继续");
    }

    void OnClickExitBtn()
    {
        Debug.Log("退出");
    }
}
