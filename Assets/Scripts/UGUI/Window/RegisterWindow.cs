using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterWindow : BaseWindow
{
    private RegisterPanel m_MainPanel;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<RegisterPanel>();
        AddButtonClickListener(m_MainPanel.RegisterBtn, OnClickRegisterBtn);
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);
    }
    /// <summary>
    /// 注册事件
    /// </summary>
    private void OnClickRegisterBtn()
    {
        //校验TODO
        Debug.Log("用户名："+m_MainPanel.UsernameTxt.text+" 昵称："+ m_MainPanel.NicknameTxt.text+" 密码："+ m_MainPanel.PasswordTxt.text+"确认密码："+ m_MainPanel.AgainPasswordTxt.text);
        //发注册请求
        Debug.Log("发送注册请求");
    }
    /// <summary>
    /// 关闭界面返回登录界面
    /// </summary>
    private void OnClickCloseBtn()
    {
        UIManager.Instance.PopUpWnd(ConStr.LOGINPANEL,true);
        UIManager.Instance.CloseWindow(ConStr.REGISTERPANEL,true);
    }
}
