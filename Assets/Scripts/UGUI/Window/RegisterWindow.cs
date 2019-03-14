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

    public override bool OnMessage(UIMsgID msgId, object[] paramList)
    {
        if(msgId == UIMsgID.OK)
            //Toast("提示", "注册成功！");
            Debug.Log("注册成功");
        else
            //Toast("提示", "注册失败！");
            Debug.Log("注册失败");
        return true;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void OnClickRegisterBtn()
    {
        //校验TODO
        Debug.Log("用户名："+m_MainPanel.UsernameTxt.text+" 昵称："+ m_MainPanel.NicknameTxt.text+" 密码："+ m_MainPanel.PasswordTxt.text+"确认密码："+ m_MainPanel.AgainPasswordTxt.text);
        //发注册请求
        if (m_MainPanel.PasswordTxt.text != m_MainPanel.AgainPasswordTxt.text)
            return;
        GData.REGISTER sign = new GData.REGISTER();
        sign.Id = m_MainPanel.UsernameTxt.text;
        sign.Password = m_MainPanel.PasswordTxt.text;
        sign.name = m_MainPanel.NicknameTxt.text;
        SocketClient.Instance.SendAsyn(sign);
    }
    /// <summary>
    /// 关闭界面返回登录界面
    /// </summary>
    private void OnClickCloseBtn()
    {
        UIManager.Instance.OpenWnd(ConStr.LOGINPANEL,true);
        UIManager.Instance.CloseWindow(ConStr.REGISTERPANEL,true);
    }
}
