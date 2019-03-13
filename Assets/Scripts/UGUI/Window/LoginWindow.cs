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
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);//关闭监听
        if (SocketClient.IsOnline)
        {
            if (!SocketClient.Instance.IsConnected)
                m_MainPanel.StartCoroutine(WaitForConnect());
        }
        else
        {
            Debug.Log("离线模式");
            AddButtonClickListener(m_MainPanel.LoginBtn, OnClickLoginBtn);//登陆监听
            AddButtonClickListener(m_MainPanel.RegisterBtn, OnClickRegisterBtn);//注册监听
        }
    }

    /// <summary>
    /// 等待连接到服务器
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForConnect()
    {
        if (SocketClient.IsOnline)
        {
            Debug.Log("正在连接服务器...");
            SocketClient.Instance.Connect();
            yield return new WaitUntil(() => SocketClient.Instance.IsConnected);
            Debug.Log("已成功连接到服务器!");
        }
        else
            Debug.Log("离线模式");
        AddButtonClickListener(m_MainPanel.LoginBtn, OnClickLoginBtnOnline);//登陆监听
        AddButtonClickListener(m_MainPanel.RegisterBtn, OnClickRegisterBtn);//注册监听
    }

    public new void OnMessage(UIMsgID msgId, params object[] paramList)
    {
        switch (msgId)
        {
            case UIMsgID.OK:
                Debug.Log("登陆成功！");
                UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
                UIManager.Instance.CloseWindow(ConStr.LOGINPANEL, true);
                break;
            case UIMsgID.FIAL:
                Debug.Log("登陆失败！");
                break;
            default:break;
        }
    }

    /// <summary>
    /// 登录点击事件(在线)
    /// </summary>
    private void OnClickLoginBtnOnline()
    {
        Debug.Log("登录");
        Debug.Log("账号："+m_MainPanel.UserNameTxt.text+"密码："+m_MainPanel.PassWordTxt.text);
        GData.LOGIN login = new GData.LOGIN();
        login.Id = m_MainPanel.UserNameTxt.text;
        login.Password = m_MainPanel.PassWordTxt.text;
        SocketClient.Instance.SendAsyn(login);

    }

    /// <summary>
    /// 登录点击事件(离线)
    /// </summary>
    private void OnClickLoginBtn()
    {
        Debug.Log("登录");
        Debug.Log("账号：" + m_MainPanel.UserNameTxt.text + "密码：" + m_MainPanel.PassWordTxt.text);
        UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
        UIManager.Instance.CloseWindow(ConStr.LOGINPANEL, true);
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
        Toast("11", "22");
        Debug.Log("点击关闭按钮");
        Application.Quit();
    }
}
