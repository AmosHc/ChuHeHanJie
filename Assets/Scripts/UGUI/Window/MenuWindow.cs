using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : BaseWindow
{
    private MenuPanel m_MainPanel;
    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.PlayBtn, OnClickPlayBtn);
        AddButtonClickListener(m_MainPanel.SettingBtn, OnClickSettingBtn);
        AddButtonClickListener(m_MainPanel.RembattleBtn, OnClickRembattleBtn);
        AddButtonClickListener(m_MainPanel.FriendBtn, OnClickFriendBtn);
        AddButtonClickListener(m_MainPanel.ShopBtn, OnClickShopBtn);
        AddButtonClickListener(m_MainPanel.HelpBtn, OnClickHelpBtn);
        AddButtonClickListener(m_MainPanel.InfoBtn, OnClickInfoBtn);
        AddButtonClickListener(m_MainPanel.ExitBtn, OnClickExitBtn);
    }
    /// <summary>
    /// 对战
    /// </summary>
    private void OnClickPlayBtn()
    {

    }
    /// <summary>
    /// 设置
    /// </summary>
    private void OnClickSettingBtn()
    {

    }
    /// <summary>
    /// 布阵模块
    /// </summary>
    private void OnClickRembattleBtn()
    {

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

    }
    /// <summary>
    /// 信息
    /// </summary>
    private void OnClickInfoBtn()
    {

    }
    /// <summary>
    /// 退出
    /// </summary>
    private void OnClickExitBtn()
    {
        Application.Quit();
    }
}
