using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Text;
using System.Reflection;
using ProtoUser;
using Google.Protobuf;

public class SocketClient:Singleton<SocketClient>
{
    static byte[] Read_Buffer = new byte[1024];
    static byte[] Write_Buffer = new byte[1024];

    public static bool IsOnline = true;    //在线模式

    public Socket m_Socket = null;
    public Queue<byte[]> MsgQueue { get; } = new Queue<byte[]>();   //消息队列
    public bool IsConnected = false;

    /// <summary>
    /// 向服务发送消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="data">消息内容</param>
    public void SendAsyn<T>(T data, _RequestType _RequestType) where T : IMessage
    {
        lock (Write_Buffer)
        {
            Write_Buffer = ObjectToBytes(_RequestType, data);
            m_Socket.Send(Write_Buffer);
        }
        Debug.Log("消息发送成功！消息类型：" + _RequestType);
        //m_Socket.BeginSend(Write_Buffer, 0, GetBytesLenth(Write_Buffer), SocketFlags.None, new AsyncCallback(SendMess), m_Socket);
    }

    public void SendAsyn(_RequestType _RequestType)
    {
        byte[] array = new byte[3];
        array[0] = (byte)_RequestType;
        array[1] = 3;
        array[2] = 0;
        m_Socket.Send(array);
        Debug.Log("消息发送成功！消息类型：" + _RequestType);
    }

    /// <summary>
    /// 初始化工作，包括Socket的初始化、连接服务器以及开启消息异步接受
    /// </summary>
    public void Connect()
    {
        IsConnected = false;
        int port = 8888;
        string host = "39.105.149.213";
        //                   string host = "127.0.0.1";
        IPAddress ip = IPAddress.Parse(host);
        IPEndPoint ipe = new IPEndPoint(ip, port);
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        m_Socket.BeginConnect(ipe, new AsyncCallback(EndConnect), null);
    }
    void EndConnect(IAsyncResult ar)
    {
        m_Socket.EndConnect(ar);
        IsConnected = true;
        m_Socket.BeginReceive(Read_Buffer, 0, Read_Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMess), m_Socket);
    }


    /// <summary>
    /// 异步接收消息的函数回调
    /// </summary>
    /// <param name="ar"></param>
    void ReceiveMess(IAsyncResult ar)
    {
        Socket m_socket = ar.AsyncState as Socket;
        int len = m_socket.EndReceive(ar);  //这里获取的lenth太不靠谱
        if (len > 0)
        {
            len = ByteToLenth(Read_Buffer[1], Read_Buffer[2]) - 3;  //获取ReadBuffer的长度
            Debug.Log("收到服务器消息,消息类型："+ (_RequestType)Read_Buffer[0]);
            switch (Read_Buffer[0])
            {
                case (int)_RequestType.LOGINOK:       //登陆成功
                    DataLocal.Instance.PLAYERINFO = BytesToObject<EMbattle>(Read_Buffer, 3, len);
                    UIManager.Instance.SendMessageToWindow(ConStr.LOGINPANEL, UIMsgID.OK);
                    break;
                case (int)_RequestType.LOGINFAIL:     //登陆失败
                    UIManager.Instance.SendMessageToWindow(ConStr.LOGINPANEL, UIMsgID.FAIL);
                    break;
                case (int)_RequestType.REGISTEROK:    //注册成功
                    UIManager.Instance.SendMessageToWindow(ConStr.REGISTERPANEL, UIMsgID.OK);
                    break;
                case (int)_RequestType.REGISTERFAIL:  //注册失败
                    UIManager.Instance.SendMessageToWindow(ConStr.REGISTERPANEL, UIMsgID.FAIL);
                    break;
                case (int)_RequestType.CAMPRED:     //红方阵营
                    DataLocal.Instance.MyCamp = WarData.Types.CampState.Red;
                    SendAsyn(DataLocal.Instance.PLAYERINFO, _RequestType.FORMATION);
                    break;
                case (int)_RequestType.CAMPBLUE:    //蓝方阵营
                    DataLocal.Instance.MyCamp = WarData.Types.CampState.Blue;
                    SendAsyn(DataLocal.Instance.PLAYERINFO, _RequestType.FORMATION);
                    break;
                case (int)_RequestType.FORMATION:   //保存对方阵形
                    DataLocal.Instance.ENEMYINFO = BytesToObject<EMbattle>(Read_Buffer, 3, len);
                    UIManager.Instance.SendMessageToWindow(ConStr.MENUPANEL, UIMsgID.OK);
                    break;
                case (int)_RequestType.PLAYERDATA:  //游戏中玩家数据
                    WarData.Types.Player data_player = new WarData.Types.Player();
                    data_player = BytesToObject<WarData.Types.Player>(Read_Buffer, 3, len);
                    System_Event.m_Events.Dispatche(System_Event.GAMEPLAYERDATA,_RequestType.PLAYERDATA, data_player);
                    break;
                case (int)_RequestType.SOILDERDATA:  //游戏中小兵数据
                    WarData.Types.Soilder data_soilder = new WarData.Types.Soilder();
                    data_soilder = BytesToObject<WarData.Types.Soilder>(Read_Buffer, 3, len);
                    System_Event.m_Events.Dispatche(System_Event.GAMESOILDERDATA,_RequestType.SOILDERDATA, data_soilder);
                    break;
                case (int)_RequestType.BULLETDATA:  //游戏中子弹数据
                    WarData.Types.Bullet data_bullet = new WarData.Types.Bullet();
                    data_bullet = BytesToObject<WarData.Types.Bullet>(Read_Buffer, 3, len);
                    System_Event.m_Events.Dispatche(System_Event.GAMEBULLETDATA, _RequestType.BULLETDATA, data_bullet);
                    break;
                case (int)_RequestType.NEWROUND:    //下一回合开始可以进行出兵操作
                    System_Event.m_Events.Dispatche(System_Event.GAMENEWROUND);
                    break;
                default:break;
            }
            m_socket.BeginReceive(Read_Buffer, 0, Read_Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMess), m_socket);
        }
    }

    /// <summary>
    /// 异步发送消息的函数回调
    /// </summary>
    /// <param name="ar"></param>
    void SendMess(IAsyncResult ar)
    {
        Socket m_socket = ar.AsyncState as Socket;
        m_socket.EndSend(ar);
        Debug.Log("消息发送成功！");
    }

    /// <summary>
    /// 获取字节数组实际长度，用于控制发送的字节流长度
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns>有效长度</returns>
    public static int GetBytesLenth(byte[] bytes)
    {
        int i = 0;
        for (; i < bytes.Length; i++)
        {
            if (bytes == null)
                return 0;
            if (bytes[i] == '\0')
                break;
        }
        return i;
    }


    public static byte[] ObjectToBytes<T>(_RequestType _RequestType,T instance) where T : IMessage
    {
        byte _type = (byte)_RequestType;
        MemoryStream memoryStream = new MemoryStream();
        CodedOutputStream cos = new CodedOutputStream(memoryStream);
        instance.WriteTo(cos);
        cos.Flush();
        byte[] array = new byte[memoryStream.Length + 3];
        array[0] = _type;
        array[1] = LenthToByte((int)memoryStream.Length + 3)[0];
        array[2] = LenthToByte((int)memoryStream.Length + 3)[1];
        Debug.Log("Lenth---->" + array.Length);
        memoryStream.Position = 0L;
        memoryStream.Read(array, 3, array.Length - 3);
        cos.Dispose();
        return array;
    }

    public static T BytesToObject<T>(byte[] bytesData, int offset, int length) where T : IMessage, new()
    {
        if (length <= 1)
            return default(T);
        byte[] array = new byte[length];
        for (int i = 0; i < array.Length; i++)
            array[i] = bytesData[i + offset];
        T result = new T();
        try
        {
            CodedInputStream cis = new CodedInputStream(array, 0, array.Length);
            result.MergeFrom(cis);
            cis.Dispose();
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        return result;
    }

    public static byte[] LenthToByte(int a)
    {
        byte[] Lenth = new byte[2];
        if (a > 255)
        {
            Lenth[1] = Convert.ToByte(a / 256);
            Lenth[0] = Convert.ToByte(a % 256);  
        }
        else
        {
            Lenth[1] = 0;
            Lenth[0] = Convert.ToByte(a);
        }
        return Lenth;
    }

    public static int ByteToLenth(byte a,byte b)
    {
        return b * 256 + a;
    }
}
