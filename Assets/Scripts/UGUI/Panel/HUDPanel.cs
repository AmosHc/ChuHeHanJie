using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : MonoBehaviour
{
    public Text RedHpText;//红色方血量
    public Text BlueHpText;//蓝色方血量
    public Slider RedHpBar;//红色方血条
    public Slider BlueHpBar;//蓝色方血条
    public Button SwitchGunsBtn;//选择枪械
    public Button ShootBtn;//射击
    public Image ShootBtnImg;//射击按钮图片
    public Image CampImg;//阵营显示

    public Sprite RedSprite;
    public Sprite BlueSprite;
}
