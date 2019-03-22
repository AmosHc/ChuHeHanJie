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

    public static bool IsOnline = false;    //在线模式

    private Socket m_Socket = null;
    public Queue<byte[]> MsgQueue { get; } = new Queue<byte[]>();   //消息队列
    public bool IsConnected = false;

    /// <summary>
    /// 向服务发送消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="data">消息内容</param>
    public void SendAsyn<T>(_RequestType _RequestType,T data) where T : IMessage
    {
        Write_Buffer = ObjectToBytes(_RequestType, data);
        m_Socket.Send(Write_Buffer);
        Debug.Log("消息发送成功！");
        //m_Socket.BeginSend(Write_Buffer, 0, GetBytesLenth(Write_Buffer), SocketFlags.None, new AsyncCallback(SendMess), m_Socket);
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
            len = GetBytesLenth(Read_Buffer);   //获取ReadBuffer的长度
            Debug.Log("收到服务器消息,消息类型："+ (_RequestType)Read_Buffer[0]);
            switch (Read_Buffer[0])
            {
                case (int)_RequestType.PLAYERINFO:       //登陆成功
                    DataLocal.Instance.PLAYERINFO = BytesToObject<EMbattle>(Read_Buffer, 1, len);
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
        byte[] array = new byte[memoryStream.Length + 1];
        array[0] = _type;
        memoryStream.Position = 0L;
        memoryStream.Read(array, 1, array.Length - 1);
        cos.Dispose();
        return array;
    }

    public static T BytesToObject<T>(byte[] bytesData, int offset, int length) where T : IMessage, new()
    {
        if (length <= 1)
            return default(T);
        byte[] array = new byte[length - offset];
        for (int i = 0; i < array.Length; i++)
            array[i] = bytesData[i + offset];

        CodedInputStream cis = new CodedInputStream(array, 0, array.Length);
        T result = new T();
        result.MergeFrom(cis);
        cis.Dispose();
        return result;
    }
}
