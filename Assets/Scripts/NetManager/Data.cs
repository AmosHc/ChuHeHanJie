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

    EMbattle = 18,       //布阵
    PLAYERDATA = 19,     //游戏中玩家数据
    SOILDERDATA = 20,     //游戏中士兵数据
    CAMPRED = 21,   //红方阵营
    CAMPBLUE = 22,   //蓝方阵营

    ISREADY = 23,   //请求出兵
    NEWROUND = 24   //出兵消息
}

public class DataLocal : Singleton<DataLocal>
{
    public EMbattle PLAYERINFO;

    public EMbattle ENEMYINFO;

    public WarData.Types.CampState MyCamp;
}