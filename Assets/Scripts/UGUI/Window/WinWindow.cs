using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinWindow : BaseWindow
{
    private WinPanel m_MainPanel;

    private bool IsWaitStart = false;
    private bool IsStart = false;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        m_MainPanel = GameObject.GetComponent<WinPanel>();
        AddButtonClickListener(m_MainPanel.BackMenuBtn, OnClickBackMenu);
        AddButtonClickListener(m_MainPanel.AgainBtn, OnClickAgain);
    }
    //跳菜单
    private void OnClickBackMenu()
    {
        UIManager.Instance.OpenWnd(ConStr.MENUPANEL,true);
        UIManager.Instance.CloseWindow(this);
    }

    public override bool OnMessage(UIMsgID msgId, object[] paramList)
    {

        if (msgId == UIMsgID.OK)
        {
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

    //再来一把
    private void OnClickAgain()
    {
        if (!SocketClient.IsOnline)
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

}
