using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : BaseWindow
{
    private MenuPanel m_MainPanel;
    private bool IsWaitStart = false;
    private bool IsStart = false;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<MenuPanel>();
        if (!SocketClient.IsOnline)
            DataLocal.Instance.PLAYERINFO = new ProtoUser.EMbattle();
        m_MainPanel.WelcomeTxt.text = "欢迎您：" + DataLocal.Instance.PLAYERINFO.Name;
        AddButtonClickListener(m_MainPanel.PlayBtn, OnClickPlayBtn);
        AddButtonClickListener(m_MainPanel.SettingBtn, OnClickSettingBtn);
        AddButtonClickListener(m_MainPanel.RembattleBtn, OnClickRembattleBtn);
        AddButtonClickListener(m_MainPanel.FriendBtn, OnClickFriendBtn);
        AddButtonClickListener(m_MainPanel.ShopBtn, OnClickShopBtn);
        AddButtonClickListener(m_MainPanel.HelpBtn, OnClickHelpBtn);
        AddButtonClickListener(m_MainPanel.InfoBtn, OnClickInfoBtn);
        AddButtonClickListener(m_MainPanel.ExitBtn, OnClickExitBtn);
    }

    public override bool OnMessage(UIMsgID msgId, object[] paramList)
    {
        Debug.Log("1...");

        if (msgId == UIMsgID.OK)
        {
            Debug.Log("2...");
            IsStart = true;
        }
        return true;
    }

    IEnumerator WaitForStartGame()
    {
        Debug.Log("Wait...");
        yield return new WaitUntil(() => IsStart);
        Debug.Log("Start...");
        GameMapManger.Instance.LoadScene(ConStr.ARSCENE);
        UIManager.Instance.CloseWindow(ConStr.ALERTPANEL);
        UIManager.Instance.CloseWindow(this);
    }

    /// <summary>
    /// 对战
    /// </summary>
    private void OnClickPlayBtn()
    {
        if(!SocketClient.IsOnline)
        {
            GameMapManger.Instance.LoadScene(ConStr.ARSCENE);
            UIManager.Instance.CloseWindow(this);
            return;
        }
        Toast("提示", "匹配玩家中...");
        if (IsWaitStart)
            return;
        else
        {
            IsWaitStart = true;
            m_MainPanel.StartCoroutine(WaitForStartGame());
            SocketClient.Instance.SendAsyn(DataLocal.Instance.PLAYERINFO, _RequestType.START);
        }
    }
    /// <summary>
    /// 设置
    /// </summary>
    private void OnClickSettingBtn()
    {
        UIManager.Instance.PopUpWnd(this, ConStr.SETTINGPANEL);
    }
    /// <summary>
    /// 布阵模块
    /// </summary>
    private void OnClickRembattleBtn()
    {
        UIManager.Instance.OpenWnd(ConStr.EMBATTLEPANEL, true);
        UIManager.Instance.CloseWindow(ConStr.MENUPANEL, false);
    }
    /// <summary>
    /// 朋友
    /// </summary>
    private void OnClickFriendBtn()
    {

    }
    /// <summary>
    /// 商店
    /// </summary>
    private void OnClickShopBtn()
    {

    }
    /// <summary>
    /// 帮助
    /// </summary>
    private void OnClickHelpBtn()
    {
        UIManager.Instance.PopUpWnd(this, ConStr.HELPPANEL);
    }
    /// <summary>
    /// 信息
    /// </summary>
    private void OnClickInfoBtn()
    {
        UIManager.Instance.PopUpWnd(this,ConStr.INFOPANEL);
    }
    /// <summary>
    /// 退出
    /// </summary>
    private void OnClickExitBtn()
    {
        UIManager.Instance.PopUpWnd(this, ConStr.EXITPANEL);
    }

    public override void OnShow(params object[] paramList)
    {
        //Debug.Log("OnShow");
    }

    public override void OnDisable()
    {
        //Debug.Log("OnDisable");
    }

    public override void OnClose()
    {
        //Debug.Log("OnClose");
    }
}
