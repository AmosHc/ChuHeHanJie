using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;
using UnityEngine.UI;
using Google.Protobuf;

public class EmbattleWindow : BaseWindow
{
    private EmbattlePanel m_EmbattalePanel;

    EMbattle PLAYERINFO = DataLocal.Instance.PLAYERINFO;
    //DataOnline.PLAYERINFO PLAYERINFO = new DataOnline.PLAYERINFO();
    int RoundNow = 0;  //当前回合
    int ChooseNow = 0; //当前选中位置
    int CostTotalNow = 1;    //当前总费用 
    int CostNow = 1;    //当前剩余费用
    Sprite imageold;

    public override void Awake(params object[] paramList)
    {
        base.Awake(paramList);
        Debug.Log("Awake");
        m_EmbattalePanel = GameObject.GetComponent<EmbattlePanel>();
        imageold = m_EmbattalePanel.ChooseImg[0].sprite;   //暂时获取UImask作为初始图片
        for (int i = 0; i < m_EmbattalePanel.ChooseBtn.Length; i++)
        {
            m_EmbattalePanel.ChooseBtn[i].onClick.AddListener(OnChooseClick);
            m_EmbattalePanel.RoundBtn[i].onClick.AddListener(OnRoundClick);
        }
        m_EmbattalePanel.SaveBtn.onClick.AddListener(OnSaveClick);
        ShowFormation();

    }

    /// <summary>
    /// 显示当前回合的阵形
    /// </summary>
    /// <param name="Round_Index"></param>
    private void ShowFormation()
    {
        CostNow = CostTotalNow;
        ByteString FormIndex = PLAYERINFO.Embattle[RoundNow];
        int index = 9;
        for (int i = 9; i >= 0; i--)
        {
            switch (FormIndex[index])
            {
                case ConStr.ArmsNull:
                    m_EmbattalePanel.ChooseImg[i].sprite = m_EmbattalePanel.Sprite_null;
                    break;
                case ConStr.ArmsCavalry:
                    CostNow -= ConStr.CostCavalry;
                    m_EmbattalePanel.ChooseImg[i].sprite = m_EmbattalePanel.Sprite_Cavalry;
                    break;
                case ConStr.ArmsBowmen:
                    CostNow -= ConStr.CostBowmen;
                    m_EmbattalePanel.ChooseImg[i].sprite = m_EmbattalePanel.Sprite_Bowmen;
                    break;
                case ConStr.ArmsMauler:
                    CostNow -= ConStr.CostMauler;
                    m_EmbattalePanel.ChooseImg[i].sprite = m_EmbattalePanel.Sprite_Mauler;
                    break;
                case ConStr.ArmsInfantry:
                    CostNow -= ConStr.CostInfantry;
                    m_EmbattalePanel.ChooseImg[i].sprite = m_EmbattalePanel.Sprite_Infantry;
                    break;
                default:
                    Debug.LogWarning("阵形消息错误。formation:" + FormIndex.ToBase64());
                    break;
            }
            index--;
            if (index < 0)
            {
                for (i = i - 1; i >= 0; i--)
                    m_EmbattalePanel.ChooseImg[i].sprite = m_EmbattalePanel.Sprite_null;
                break;
            }
        }
        m_EmbattalePanel.CostNow.text = "当前剩余费用：" + CostNow.ToString();
    }

    public override bool OnMessage(UIMsgID msgId, object[] paramList)
    {
        if (CostNow - (int)paramList[1] < 0)
        {
            Toast("提示", "当前费用不足");
            return true;
        }

        byte[] bytearr = PLAYERINFO.Embattle[RoundNow].ToByteArray();
        bytearr[ChooseNow] = (byte)paramList[0];
        PLAYERINFO.Embattle[RoundNow] = ByteString.CopyFrom(bytearr);
        ShowFormation();
        return true;
    }

    void OnChooseClick()
    {
        GameObject go = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Transform EmbattleGroup = go.transform.parent.parent;
        for (int i = 0; i < EmbattleGroup.childCount; i++)
        {
            if(EmbattleGroup.GetChild(i).GetChild(0).gameObject == go)
            {
                ChooseNow = i;
                break;
            }
        }
        UIManager.Instance.OpenWnd(ConStr.SELECTPANEL, true);
    }

    void OnRoundClick()
    {
        GameObject go = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Transform NumGroup = go.transform.parent.parent;
        for (int i = 0; i < NumGroup.childCount; i++)
        {
            if (NumGroup.GetChild(i).GetChild(0).gameObject == go)
            {
                RoundNow = i;
                CostTotalNow = RoundNow + 1;
                break;
            }
        }
        ShowFormation();
    }
    
    void OnSaveClick()
    {
        //发送本地阵形配置到服务器
        //for (int i = 0; i < 10; i++)
        //{
        //    byte[] vs = new byte[10];
        //    for (int j = 0; j < 10; j++)
        //        vs[j] = PLAYERINFO.Embattle[i].ToByteArray()[j];
        //    PLAYERINFO.Embattle[i] = ByteString.CopyFrom(vs);
        //}

        DataLocal.Instance.PLAYERINFO = PLAYERINFO;
        SocketClient.Instance.SendAsyn(_RequestType.EMbattle, PLAYERINFO);
        UIManager.Instance.CloseWindow(ConStr.SELECTPANEL, true);
        UIManager.Instance.CloseWindow(ConStr.EMBATTLEPANEL, true);
        UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
    }
}
