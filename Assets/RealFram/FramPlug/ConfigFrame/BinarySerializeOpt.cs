using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

public class BinarySerializeOpt {

    /// <summary>
    /// 将类序列化为xml
    /// </summary>
    /// <param name="path">存储路径</param>
    /// <param name="obj">类对象</param>
    /// <returns></returns>
    public static bool XmlSerialize(string path , System.Object obj)
    {
        try
        {
            using (FileStream fs = new FileStream(path,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs,System.Text.Encoding.UTF8))
                {
                    //去除顶部的前缀，看需求
                    //XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                    //xmlSerializerNamespaces.Add(string.Empty,string.Empty);
                    //xmlSerializer.Serialize(sw, obj, xmlSerializerNamespaces);
                    //序列化
                    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                    xmlSerializer.Serialize(sw,obj);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("此类无法转换成Xml:" + obj.GetType() + "," + e);
        }
        return false;
    }
    /// <summary>
    /// 编辑器下读取XML方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T XmlDeserialization<T>(string path) where T : class
    {
        T t = default(T);
        try
        {
            using (FileStream fs = new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                t = (T)xmlSerializer.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("xml反序列化失败，请检查：" + path + "," + e);
        }
        return t;
    }

    public static object XmlDeserialization(string path, Type type)
    {
        object obj = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                obj = xmlSerializer.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("xml反序列化失败，请检查：" + path + "," + e);
        }
        return obj;
    }

    /// <summary>
    /// 运行时读取XML
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T XmlDeserializationRun<T>(string path) where T : class
    {
        T t = default(T);
        TextAsset asset = ResourceManger.Instance.LoadResource<TextAsset>(path);
        if (asset == null)
        {
            Debug.Log("Asset加载为空，请检查："+path);
        }

        try
        {
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                t = (T)xmlSerializer.Deserialize(ms);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("加载textasset异常:" + path + "," + e);
        }

        return t;
    }

    /// <summary>
    /// 将类序列化为二进制
    /// </summary>
    /// <param name="path">存储路径</param>
    /// <param name="obj">类对象</param>
    /// <returns></returns>
    public static bool BinarySerialize(string path, System.Object obj)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs,obj);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("此类无法转换成二进制:" + obj.GetType() + "," + e);
        }
        return false;
    }

    /// <summary>
    /// 运行时读取二进制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T BinaryDeserializationRun<T>(string path) where T : class
    {
        T t = default(T);
        TextAsset asset = ResourceManger.Instance.LoadResource<TextAsset>(path);
        if (asset == null)
        {
            Debug.Log("Asset加载为空，请检查：" + path);
        }

        try
        {
            using (MemoryStream ms = new MemoryStream(asset.bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                t = (T)formatter.Deserialize(ms);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("加载textasset异常:" + path + "," + e);
        }

        return t;
    }

}
