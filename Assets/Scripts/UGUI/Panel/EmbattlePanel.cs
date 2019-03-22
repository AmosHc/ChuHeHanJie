using Ricimi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmbattlePanel : MonoBehaviour {

    public Image[] ChooseImg;   //十个位置图片
    public Button[] ChooseBtn;   //十个位置按钮
    public Button[] RoundBtn;   //十个回合按钮
    public AnimatedButton SaveBtn;      //保存按钮
    public Text CostNow;    //当前费用

    public Sprite Sprite_null;
    public Sprite Sprite_Cavalry;
    public Sprite Sprite_Bowmen;
    public Sprite Sprite_Mauler;
    public Sprite Sprite_Infantry;

    private void Awake()
    {
        Transform EmbattleGroupTrans = transform.Find("EmbattleGroup");
        Transform NumGroupTrans = transform.Find("NumGroup");
        for (int i = 0; i < 10; i++)
            ChooseImg[i] = EmbattleGroupTrans.GetChild(i).Find("bg/Icon").GetComponent<Image>();
        for (int i = 0; i < 10; i++)
            ChooseBtn[i] = EmbattleGroupTrans.GetChild(i).Find("bg").GetComponent<Button>();
        for (int i = 0; i < 10; i++)
            RoundBtn[i] = NumGroupTrans.GetChild(i).Find("numBtn").GetComponent<Button>();
    }
}