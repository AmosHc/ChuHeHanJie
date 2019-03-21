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
    PLAYERINFO = 202,   //登陆成功返回玩家信息
    LOGINFAIL = 203,    //登陆失败
    START = 210,         //开始游戏

    EMbattle = 18       //保存阵形
}

public class DataLocal : Singleton<DataLocal>
{
    public User User;

    public EMbattle PLAYERINFO;
}