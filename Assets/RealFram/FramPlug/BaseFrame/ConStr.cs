﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConStr {
    //界面名称
    public const string LOGINPANEL = "LoginPlane";
    public const string LOADPANEL = "LoadPanel";
    public const string REGISTERPANEL = "RegisterPanel";
    public const string MENUPANEL = "MenuPanel";
    public const string INFOPANEL = "InfoPanel";
    public const string HELPPANEL = "HelpPanel";
    public const string EXITPANEL = "ExitPanel";
    public const string SETTINGPANEL = "SettingPanel";
    public const string ALERTPANEL = "AlertPanel";
    public const string EMBATTLEPANEL = "EmbattlePanel";
    public const string SELECTPANEL = "SelectPanel";
    public const string HUDPANEL = "HUDPanel";
    public const string WINPANEL = "WinPanel";
    public const string LOSEPANEL = "LosePanel";
    //场景名称
    public const string EMPTYSCENE = "Empty";
    public const string MENUSCENE = "Menu";
    public const string ARSCENE = "ARTest";

    //预加载资源
    public const string ALERTPRE = "Assets/GameData/Prefabs/UGUI/Panel/AlertPanel.prefab";
    //阵营
    public const int campRed = 0;
    public const int campBlue = 1;

    //小兵编号
    public const byte ArmsNull = 48;
    public const byte ArmsCavalry = 49;
    public const byte ArmsBowmen = 50;
    public const byte ArmsMauler = 51;
    public const byte ArmsInfantry = 52;

    public const int CostNull = 0;
    public const int CostCavalry = 1;
    public const int CostBowmen = 2;
    public const int CostMauler = 3;
    public const int CostInfantry = 4;

    //小兵资源路径
    public const string MaulerPrefab = "Assets/GameData/Prefabs/AR/thief.prefab";
    public const string CavalryPrefab = "Assets/GameData/Prefabs/AR/police.prefab";
    public const string InfantryPrefab = "Assets/GameData/Prefabs/AR/roman.prefab";
    public const string BowmenPrefab = "Assets/GameData/Prefabs/AR/shaman.prefab";
    //血条资源路径
    public const string DOGFACEHPBAR = "Assets/GameData/Prefabs/UGUI/DogfaceHPBar.prefab";

}
