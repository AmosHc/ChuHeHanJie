using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;
using UnityEngine.UI;
using Google.Protobuf;

public class EmbattleWindow : BaseWindow
{
    private EmbattlePanel m_EmbattalePanel;
    private string[] AllTitleStr =
    {
        "请您布置第一回合阵型", "请您布置第二回合阵型", "请您布置第三回合阵型", "请您布置第四回合阵型",
        "请您布置第五回合阵型", "请您布置第六回合阵型", "请您布置第七回合阵型", "请您布置第八回合阵型",
        "请您布置第九回合阵型", "请您布置第十回合阵型"
    };
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
        Button tempBtn;
        for (int i = 0; i < m_EmbattalePanel.ChooseBtn.Length; i++)
        {
            int tempIdx = i;//去除delegate干扰
            m_EmbattalePanel.ChooseBtn[i].onClick.RemoveAllListeners();
            m_EmbattalePanel.ChooseBtn[i].onClick.AddListener(delegate () { OnChooseClick(tempIdx); });
            m_EmbattalePanel.RoundBtn[i].onClick.RemoveAllListeners();
            m_EmbattalePanel.RoundBtn[i].onClick.AddListener(delegate () { OnRoundClick(tempIdx); });
        }
        m_EmbattalePanel.TitleTxt.text = AllTitleStr[0];
        //m_EmbattalePanel.SaveBtn.onClick.AddListener(OnSaveClick);
        AddButtonClickListener(m_EmbattalePanel.SaveBtn, OnSaveClick);
        AddButtonClickListener(m_EmbattalePanel.CloseBtn, OnClickCloseBtn);
        if(SocketClient.IsOnline)
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

    void OnChooseClick(int index)
    {
        ChooseNow = index;
        UIManager.Instance.OpenWnd(ConStr.SELECTPANEL, true);
    }

    void OnRoundClick(int index)
    {
        //GameObject go = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        //Transform NumGroup = go.transform.parent.parent;
        //for (int i = 0; i < NumGroup.childCount; i++)
        //{
        //    if (NumGroup.GetChild(i).GetChild(0).gameObject == go)
        //    {
        //        RoundNow = i;
        //        CostTotalNow = RoundNow + 1;
        //        break;
        //    }
        //}

        RoundNow = index;
        CostTotalNow = RoundNow + 1;
        m_EmbattalePanel.TitleTxt.text = AllTitleStr[RoundNow];
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
        SocketClient.Instance.SendAsyn(PLAYERINFO, _RequestType.EMbattle);
        UIManager.Instance.CloseWindow(ConStr.SELECTPANEL);
        UIManager.Instance.CloseWindow(ConStr.EMBATTLEPANEL);
        UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
    }

    private void OnClickCloseBtn()
    {
        UIManager.Instance.CloseWindow(this);
        UIManager.Instance.OpenWnd(ConStr.MENUPANEL, true);
    }
}
