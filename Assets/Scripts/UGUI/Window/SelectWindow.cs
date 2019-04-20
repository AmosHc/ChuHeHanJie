using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectWindow : BaseWindow
{
    private SelectPanel m_SelectPanel;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        Debug.Log("Awake");
        if (m_SelectPanel == null)
            m_SelectPanel = GameObject.GetComponent<SelectPanel>();
        //m_SelectPanel.CloseBtn.onClick.AddListener(delegate () { OnChooseClick(ConStr.ArmsNull, ConStr.CostNull); });
        AddButtonClickListener(m_SelectPanel.CloseBtn, OnClickCloseBtn);
        m_SelectPanel.CavalryBtn.onClick.AddListener(delegate () { OnChooseClick(ConStr.ArmsCavalry, ConStr.CostCavalry); });
        m_SelectPanel.BowmenBtn.onClick.AddListener(delegate () { OnChooseClick(ConStr.ArmsBowmen, ConStr.CostBowmen); });
        m_SelectPanel.MaulerBtn.onClick.AddListener(delegate () { OnChooseClick(ConStr.ArmsMauler, ConStr.CostMauler); });
        m_SelectPanel.InfantryBtn.onClick.AddListener(delegate () { OnChooseClick(ConStr.ArmsInfantry, ConStr.CostInfantry); });
        m_SelectPanel.NoneBtn.onClick.AddListener(delegate () { OnChooseClick(ConStr.ArmsNull, ConStr.CostNull); });
    }

    public override void OnClose()
    {
        base.OnClose();
        m_SelectPanel.CloseBtn.onClick.RemoveAllListeners();
        m_SelectPanel.CavalryBtn.onClick.RemoveAllListeners();
        m_SelectPanel.BowmenBtn.onClick.RemoveAllListeners();
        m_SelectPanel.MaulerBtn.onClick.RemoveAllListeners();
        m_SelectPanel.InfantryBtn.onClick.RemoveAllListeners();
    }
    private void OnClickCloseBtn()
    {
        UIManager.Instance.CloseWindow(this);
    }
    void OnChooseClick(byte ArmsIndex, int CostNew)
    {
        UIManager.Instance.SendMessageToWindow(ConStr.EMBATTLEPANEL, UIMsgID.OK, ArmsIndex, CostNew);
        UIManager.Instance.CloseWindow(ConStr.SELECTPANEL, false);
    }
}
