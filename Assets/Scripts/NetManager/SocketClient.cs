using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Collections;
using System.Collections.Generic;

public class SocketClient
{
    private byte[] buffer = new byte[1024];
    private Socket m_Socket = null;
    public Queue<byte[]> MsgQueue { get; } = new Queue<byte[]>();   //消息队列

    private static SocketClient m_instance = null;
    public static SocketClient Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new SocketClient();
                m_instance.OnInit();
            }
            return m_instance;
        }
    }

    /// <summary>
    /// 向服务发送消息
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="data">消息内容</param>
    public void SendAsyn<T>(T data) where T : IMessage
    {
        CodedOutputStream cos = new CodedOutputStream(buffer);
        data.WriteTo(cos);
        int len = GetBytesLenth(buffer);
        m_Socket.BeginSend(buffer, 0, len, SocketFlags.None, new AsyncCallback(SendMess), m_Socket);
    }

    /// <summary>
    /// 在消息队列中获取消息
    /// </summary>
    /// <returns>消息内容</returns>
    public object ReadAsyn()
    {
        if (MsgQueue.Count == 0)
            return null;
        byte[] bytes = MsgQueue.Dequeue();

        //对消息进行解析
        Data.GetRequestType getRequestType = new Data.GetRequestType();
        getRequestType.MergeFrom(bytes);
        switch (getRequestType.RequestType)
        {
            case Data.RequestType.LogIn:Data.LogIn t = new Data.LogIn();t.MergeFrom(bytes);return t;
        }
        Debug.LogWarning("Error:" + getRequestType.RequestType);
        return null;
    }

    /// <summary>
    /// 初始化工作，包括Socket的初始化、连接服务器以及开启消息异步接受
    /// </summary>
    void OnInit()
    {
        int port = 8888;
        string host = "39.105.149.213";
        //            string host = "127.0.0.1";

        IPAddress ip = IPAddress.Parse(host);
        IPEndPoint ipe = new IPEndPoint(ip, port);
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        m_Socket.Connect(ipe);

        m_Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMess), m_Socket);
    }

    /// <summary>
    /// 异步接收消息的函数回调
    /// </summary>
    /// <param name="ar"></param>
    void ReceiveMess(IAsyncResult ar)
    {
        Socket m_socket = ar.AsyncState as Socket;
        int len = m_socket.EndReceive(ar);
        if (len > 0)
        {
            MsgQueue.Enqueue(buffer);
            Debug.Log("Receive Message From Server");
            m_socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMess), m_socket);
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
    int GetBytesLenth(byte[] bytes)
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
}
