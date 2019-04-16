using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManger : Singleton<ConfigManger> {
    private Dictionary<string,ExcelBase> m_AllExcelData = new Dictionary<string, ExcelBase>();

    /// <summary>
    /// 根据路径加载类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadData<T>(string path) where T : ExcelBase
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("路径为空");
            return null;
        }

        if (m_AllExcelData.ContainsKey(path))
        {
            Debug.Log("类已加载:"+path);
            return m_AllExcelData[path] as T;
        }

        T data = BinarySerializeOpt.BinaryDeserializationRun<T>(path);
#if UNITY_EDITOR
        if (data == null)
        {
            Debug.Log(path+"二进制加载失败，改为xml加载");
            path = path.Replace("Binary", "Xml").Replace(".bytes", ".xml");
            data = BinarySerializeOpt.XmlDeserialization<T>(path);
        }
#endif
        if (data != null)
            data.Init();
        m_AllExcelData.Add(path, data);
        return data;
    }

    public T FindData<T>(string path) where T : ExcelBase
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("路径为空");
            return null;
        }

        ExcelBase excelBase = null;
        if (m_AllExcelData.TryGetValue(path, out excelBase))
        {
            return excelBase as T;
        }
        else
        {
            //加载
            excelBase = LoadData<T>(path);
            return excelBase as T;
        }

        //return null;
    }
}
/// <summary>
/// 配置路径类
/// </summary>
public class CFG
{
    /// <summary>
    /// 配置表配置路径
    /// </summary>
    public const string TABLE_MONSTER = "Assets/GameData/Data/Binary/MonsterData.bytes";
    public const string TABLE_BUFF = "Assets/GameData/Data/Binary/BuffData.bytes";
}
