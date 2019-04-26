using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using ProtoUser;

public enum _RequestType
{
    REGISTER = 101,     //注册
    LOGIN = 102,        //登陆

    REGISTEROK = 200,   //注册成功
    REGISTERFAIL = 201, //注册失败
    LOGINOK = 202,   //登陆成功返回玩家信息
    LOGINFAIL = 203,    //登陆失败
    START = 16,         //开始游戏
    GAMEOVER = 17,      //结束游戏

    EMbattle = 18,       //布阵保存
    PLAYERDATA = 19,     //游戏中玩家数据
    SOILDERDATA = 20,     //游戏中士兵数据
    BULLETDATA = 21,    //游戏中子弹数据

    REDWIN = 95,    //红方胜利
    BLUEWIN = 96,   //蓝方胜利
    NONEWIN = 97,   //平局
    CAMPRED = 98,   //红方阵营
    CAMPBLUE = 99,   //蓝方阵营

    ISREADY = 50,   //请求出兵
    NEWROUND = 51,   //出兵消息

    FORMATION = 52,  //发送阵形到服务器

    ENEMYNETCLOSE = 60,     //对方掉线
    WINONNETCLOSE = 61      //确认对方掉线获得胜利
}

public class DataLocal : Singleton<DataLocal>
{
    public EMbattle PLAYERINFO;

    public EMbattle ENEMYINFO;

    public WarData.Types.CampState MyCamp;
}