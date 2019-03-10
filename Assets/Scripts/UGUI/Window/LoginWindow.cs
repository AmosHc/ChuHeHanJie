using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginWindow : BaseWindow
{
    private LoginPanel m_MainPanel;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<LoginPanel>();
        AddButtonClickListener(m_MainPanel.LoginBtn, OnClickLoginBtn);//登录监听
        AddButtonClickListener(m_MainPanel.RegisterBtn, OnClickRegisterBtn);//注册监听
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);//注册监听
    }

    /// <summary>
    /// 登录点击事件
    /// </summary>
    private void OnClickLoginBtn()
    {
        Debug.Log("登录");
        Debug.Log("账号："+m_MainPanel.UserNameTxt.text+"密码："+m_MainPanel.PassWordTxt.text);
        UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
        UIManager.Instance.CloseWindow(ConStr.LOGINPANEL,true);

    }
    /// <summary>
    /// 注册点击事件
    /// </summary>
    private void OnClickRegisterBtn()
    {
        Debug.Log("注册");
        UIManager.Instance.OpenWnd(ConStr.REGISTERPANEL, true);
        UIManager.Instance.CloseWindow(ConStr.LOGINPANEL);
    }
    /// <summary>
    /// 关闭按钮
    /// </summary>
    private void OnClickCloseBtn()
    {
        Debug.Log("点击关闭按钮");
        Application.Quit();
    }
}
