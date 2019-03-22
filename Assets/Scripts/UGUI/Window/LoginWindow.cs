using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;

public class LoginWindow : BaseWindow
{
    private LoginPanel m_MainPanel;

    private UIMsgID LogInState = UIMsgID.NONE;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<LoginPanel>();
        AddButtonClickListener(m_MainPanel.CloseBtn, OnClickCloseBtn);//关闭监听
        if (SocketClient.IsOnline)
        {
            if (!SocketClient.Instance.IsConnected)
                m_MainPanel.StartCoroutine(WaitForConnect());
            m_MainPanel.StartCoroutine(WaitForLogSuccess());
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
//            Toast("提示", "正在连接到服务器...");
            SocketClient.Instance.Connect();
            float t = 0f;
            yield return new WaitUntil(() => {
                t += Time.deltaTime;
                if (t > 5.0f)
                    return true;
                return SocketClient.Instance.IsConnected;
                });
            if (t > 5.0f)
            {
                Toast("提示", "连接服务器失败");
                AddButtonClickListener(m_MainPanel.LoginBtn, () => Toast("提示", "连接服务器失败"));
                AddButtonClickListener(m_MainPanel.RegisterBtn, () => Toast("提示", "连接服务器失败"));
                yield break;
            }
            else
                Debug.Log("连接服务器成功");
        }
        else
            Toast("提示", "离线模式.");
        AddButtonClickListener(m_MainPanel.LoginBtn, OnClickLoginBtnOnline);//登陆监听
        AddButtonClickListener(m_MainPanel.RegisterBtn, OnClickRegisterBtn);//注册监听
    }

    /// <summary>
    /// 等待登陆成功进行场景跳转
    /// （资源加载只能在主线程执行）
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForLogSuccess()
    {
        yield return new WaitUntil(() => LogInState != UIMsgID.NONE);
        if (LogInState == UIMsgID.OK)    //登陆成功
        {
            UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
            UIManager.Instance.CloseWindow(ConStr.LOGINPANEL, true);
        }
        else if(LogInState == UIMsgID.FAIL)   //登陆失败
        {
            Toast("提示","登陆失败！");
            LogInState = UIMsgID.NONE;
            m_MainPanel.StartCoroutine(WaitForLogSuccess());
        }
    }

    public override bool OnMessage(UIMsgID msgId, params object[] paramList)
    {
        LogInState = msgId;
        return true;
    }

    /// <summary>
    /// 登录点击事件(在线)
    /// </summary>
    private void OnClickLoginBtnOnline()
    {

        Debug.Log("登录");
        Debug.Log("账号："+m_MainPanel.UserNameTxt.text+"密码："+m_MainPanel.PassWordTxt.text);
        //DataOnline.LOGIN login = new DataOnline.LOGIN();
        //login.Id = m_MainPanel.UserNameTxt.text;
        //login.Password = m_MainPanel.PassWordTxt.text;

        User user = new User
        {
            Id = m_MainPanel.UserNameTxt.text,
            Password = m_MainPanel.PassWordTxt.text
        };
        
        Debug.Log(user.GetType().Name);

        SocketClient.Instance.SendAsyn(_RequestType.LOGIN, user);
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
        Debug.Log("点击关闭按钮");
        Application.Quit();
    }
}
