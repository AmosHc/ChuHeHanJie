using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

public class GData
{
    [ProtoContract]
    public class LOGIN
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }

        public LOGIN()
        {
        }
    }

    [ProtoContract]
    public class REGISTER
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }

        [ProtoMember(3)]
        public string name { get; set; }

        public REGISTER()
        {
        }
    }

    [ProtoContract]
    public class PLAYERINFO
    {
        [ProtoMember(1)]
        public string Name { get; set; }
    }

    public enum _RequestType
    {
        REGISTER = 101,     //注册
        LOGIN = 102,        //登陆
        PLAYERINFO = 103,   //玩家信息

        REGISTEROK = 200,   //注册成功
        REGISTERFAIL = 201, //注册失败
        LOGINOK = 202,      //登陆成功
        LOGINFAIL = 203     //登陆失败
    }
}