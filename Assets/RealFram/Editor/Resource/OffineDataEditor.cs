using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OffineDataEditor : ScriptableObject
{
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOffineData()
    {
        GameObject[] objs = Selection.gameObjects;//所有选择的prefab
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据","正在修改第:"+(i+1)+"个",1.0f/objs.Length*i);
            CreateOffineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 创建offinData
    /// </summary>
    /// <param name="obj"></param>
    public static void CreateOffineData(GameObject obj)
    {
        OffineData offineData = obj.GetComponent<OffineData>();
        if (offineData == null)
            offineData = obj.AddComponent<OffineData>();
        offineData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了："+obj.name+".parfab");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/生成UI离线数据")]
    public static void AssetCreateUIData()
    {
        GameObject[] objs = Selection.gameObjects;//所有选择的prefab
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在修改第:" + (i + 1) + "个", 1.0f / objs.Length * i);
            CreateUIData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("离线数据/所有UI prefab离线数据")]
    public static void AllCreateUIData()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/UGUI" });//返回gameobject的guid
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);//根据guid返回加载路径
            EditorUtility.DisplayProgressBar("添加UI离线数据","正在扫描路径："+path, 1.0f / prefabGuids.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(System.Object.ReferenceEquals(obj,null))
                continue;
            CreateUIData(obj);
        }
        Debug.Log("UI离线数据全部生成完毕！");
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 创建UIData
    /// </summary>
    /// <param name="obj"></param>
    public static void CreateUIData(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("UI");//设置layer为ui
        UIOffineData uiData = obj.GetComponent<UIOffineData>();
        if (uiData == null)
            uiData = obj.AddComponent<UIOffineData>();
        uiData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了：" + obj.name + ".parfab");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/生成特效离线数据")]
    public static void AssetCreateEffectData()
    {
        GameObject[] objs = Selection.gameObjects;//所有选择的prefab
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在修改第:" + (i + 1) + "个", 1.0f / objs.Length * i);
            CreateEffectData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("离线数据/所有特效 prefab离线数据")]
    public static void AllCreateEffectData()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/Effect" });//返回gameobject的guid
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);//根据guid返回加载路径
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在扫描路径：" + path, 1.0f / prefabGuids.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (System.Object.ReferenceEquals(obj, null))
                continue;
            CreateEffectData(obj);
        }
        Debug.Log("特效离线数据全部生成完毕！");
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 创建特效Data
    /// </summary>
    /// <param name="obj"></param>
    public static void CreateEffectData(GameObject obj)
    {
        EffectOffineData effectOffineData = obj.GetComponent<EffectOffineData>();
        if (effectOffineData == null)
            effectOffineData = obj.AddComponent<EffectOffineData>();
        effectOffineData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了：" + obj.name + ".parfab");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}
