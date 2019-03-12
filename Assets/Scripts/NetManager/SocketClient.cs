﻿using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Text;

public class SocketClient
{
    static byte[] Read_Buffer = new byte[1024];
    static byte[] Write_Buffer = new byte[1024];

    private Socket m_Socket = null;
    public bool IsConnected = false;
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
    public void SendAsyn<T>(T data)
    {
        Write_Buffer = ObjectToBytes(data);

        m_Socket.BeginSend(Write_Buffer, 0, GetBytesLenth(Write_Buffer), SocketFlags.None, new AsyncCallback(SendMess), m_Socket);
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
        if (bytes[0] == 200)
            Debug.Log("OK!");
        return BytesToObject(bytes, 0, GetBytesLenth(bytes));
    }

    /// <summary>
    /// 初始化工作，包括Socket的初始化、连接服务器以及开启消息异步接受
    /// </summary>
    void OnInit()
    {
        int port = 8888;
        string host = "39.105.149.213";
        //                   string host = "127.0.0.1";
        IPAddress ip = IPAddress.Parse(host);
        IPEndPoint ipe = new IPEndPoint(ip, port);
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        m_Socket.BeginConnect(ipe, Connect, m_Socket);
        m_Socket.BeginReceive(Read_Buffer, 0, Read_Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMess), m_Socket);
    }

    void Connect(IAsyncResult ar)
    {
        Socket m_socket = ar as Socket;
        m_socket.EndConnect(ar);
        IsConnected = m_socket.Connected;
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
            byte[] msgcell = new byte[len];
            for (int i = 0; i < len; i++)
                msgcell[i] = Read_Buffer[i];
            MsgQueue.Enqueue(msgcell);
            Debug.Log("收到服务器消息");
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


    public static byte[] ObjectToBytes<T>(T instance)
    {
        _RequestType _RequestType = (_RequestType)Enum.Parse(typeof(_RequestType), instance.ToString());
        byte _type = (byte)_RequestType;
        MemoryStream memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, instance);
        byte[] array = new byte[memoryStream.Length + 1];
        array[0] = _type;
        memoryStream.Position = 0L;
        memoryStream.Read(array, 1, array.Length-1);
        memoryStream.Dispose();
        return array;
    }

    public static object BytesToObject(byte[] bytesData, int offset, int length)
    {
        if (length <= 1)
            return null;
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.Write(bytesData, 1, length);
        memoryStream.Position = 0L;
        _RequestType _RequestType = (_RequestType)bytesData[0];
        object r;
        switch (_RequestType)
        {
            case _RequestType.LOG_IN:r = new LOG_IN();r = Serializer.Deserialize<LOG_IN>(memoryStream);break;
            default: r = null;break;
        }
        memoryStream.Dispose();
        return r;
    }
}
