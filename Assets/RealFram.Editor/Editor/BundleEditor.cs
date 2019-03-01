using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

/// <summary>
/// 通用离线数据
/// </summary>
public class BundleEditor : ScriptableObject
{
    private static string ABOUTPUTPATH = Application.dataPath+"/../AssetBundle/"+EditorUserBuildSettings.activeBuildTarget.ToString();
    private static string ABCONFIGPATH = "Assets/RealFram.Editor/Editor/ABConfig.asset";
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();//所有文件夹ab包路径
    private static List<string> m_AllFileAB = new List<string>();//ab包已有的所有文件资源,不重复
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();//所有prefab的关联文件路径数组
    private static List<string> m_ConfigFil = new List<string>();

    [MenuItem("Tools/打包")]
    public static void Build()
    {
        Init();

        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        if (abConfig == null) { Debug.LogError("ab包配置加载失败"); return; }
        LoadFolder(abConfig);//加载文件夹
        LoadFile(abConfig);//加载prefab
        //设置ab包
        foreach(string name in m_AllFileDir.Keys)
        {
            SetAB(name, m_AllFileDir[name]);
        }
        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetAB(name, m_AllPrefabDir[name]);
        }
        BuildAssetBundle();//打包
        DeletAB(); //删除多余的ab包
        //清理ab包
        string[] allABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < allABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(allABNames[i], true);
            EditorUtility.DisplayProgressBar("清理ab包设置", "正在清理：" + allABNames[i],  1.0f / allABNames.Length * i);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    //删除多余的ab包
    static void DeletAB()
    {
        string[] allABNames = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo dire = new DirectoryInfo(ABOUTPUTPATH);
        FileInfo[] files = dire.GetFiles("*", SearchOption.AllDirectories);

        Dictionary<string,bool> dict = new Dictionary<string, bool>();//字典，用来作查找

        for (int i = 0; i < allABNames.Length; i++)
        {
            dict.Add(allABNames[i], true);
            dict.Add(allABNames[i]+ ".manifest", true);
        }
        dict.Add("StreamingAssets", true);
        dict.Add("StreamingAssets.manifest", true);

        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".meta") && !dict.ContainsKey(files[i].Name))
            {
                //删除
                Debug.Log("此文件已经被改名或删除，故删除："+ files[i].Name);
                files[i].Delete();
            }
        }
    }

    //打包 
    static void BuildAssetBundle()
    {
        string[] allABNames = AssetDatabase.GetAllAssetBundleNames();
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();//文件路径，用来生成自己的配置表
        for (int i = 0; i < allABNames.Length; i++)
        {
            string[] path = AssetDatabase.GetAssetPathsFromAssetBundle(allABNames[i]);
            
            for(int j = 0; j < path.Length; j++)
            {
                if (path[j].EndsWith(".cs")) continue;
                resPathDic.Add(path[j], allABNames[i]);
            }
        }

        WriteData(resPathDic);//写入配置表
        if (!Directory.Exists(ABOUTPUTPATH)) Directory.CreateDirectory(ABOUTPUTPATH);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(ABOUTPUTPATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if(manifest == null)
            Debug.LogError("打包失败");
        else
            Debug.Log("打包成功");
    }

    //写入配置表
    static void WriteData(Dictionary<string, string> resPathDic)
    {
        //初始化数据
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<ABBase>();
        foreach (string path in resPathDic.Keys)
        {
            if (!ValidPath(path)) continue;//不是有效路径

            ABBase ab = new ABBase();
            ab.ABName = resPathDic[path];
            ab.Path = path;
            ab.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
            ab.Crc = Crc.GetCRC32(path);
            ab.ABDependce = new List<string>();

            //查找关联
            string[] depes = AssetDatabase.GetDependencies(path);
            foreach(string depe in depes)
            {
                if (depe.EndsWith(".cs") || depe == path)
                    continue;
                //查看包名
                string abName = "";
                if (resPathDic.TryGetValue(depe, out abName))
                {
                    if(abName == ab.ABName) continue;
                    else if(!ab.ABDependce.Contains(abName))
                        ab.ABDependce.Add(abName);
                }
            }
            config.ABList.Add(ab);
        }

        //写入xml
        WriteXml(config);
        //写入二进制
        WriteBinary(config);

    }
    //写入二进制
    static void WriteBinary(AssetBundleConfig config)
    {
        //清除path字符串，这个内容和解析后的crc一样
        foreach (ABBase ab in config.ABList)
        {
            ab.Path = "";
        }

        string bytesSavePath = Application.dataPath + "/AssetBundleConfig.bytes";
        if (File.Exists(bytesSavePath)) File.Delete(bytesSavePath);
        FileStream fileStream = new FileStream(bytesSavePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fileStream, config);
        fileStream.Close();
    }

    //写入xml
    static void WriteXml(AssetBundleConfig config)
    {
        string xmlSavePath = Application.dataPath + "/AssetBundleConfig.xml";
        if (File.Exists(xmlSavePath)) File.Delete(xmlSavePath);
        FileStream fileStream = new FileStream(xmlSavePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fileStream);
        XmlSerializer serializer = new XmlSerializer(config.GetType());
        serializer.Serialize(sw, config);
        sw.Close();
        fileStream.Close();
    }

    //整个文件夹加载
    static void LoadFolder(ABConfig abConfig)
    {
        //先遍历所有的文件夹做一个键值对索引
        for (int i = 0; i < abConfig.m_FileDirABs.Count; i++)
        {
            if (m_AllFileDir.ContainsKey(abConfig.m_FileDirABs[i].ABName))
            {
                Debug.LogError("ab包配置名重复，请检查");
            }
            else
            {
                m_AllFileDir.Add(abConfig.m_FileDirABs[i].ABName, abConfig.m_FileDirABs[i].Path);
                m_AllFileAB.Add(abConfig.m_FileDirABs[i].Path);
                m_ConfigFil.Add(abConfig.m_FileDirABs[i].Path);
            }
        }
    }

    //单个文件加载
    static void LoadFile(ABConfig abConfig)
    {
        string[] allGuid = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPaths.ToArray());
        for (int i = 0; i < allGuid.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allGuid[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "prefab:" + path, 1.0f / allGuid.Length * i);
            m_ConfigFil.Add(path);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            string[] depenArr = AssetDatabase.GetDependencies(path);//得到obj的所有依赖
            List<string> depenList = new List<string>();//obj所有依赖（去重后）
            foreach (string item in depenArr)
            {
                //过滤脚本和已存在文件
                if (!ContainAllFileAB(item) && !item.EndsWith(".cs"))
                {
                    m_AllFileAB.Add(item);
                    depenList.Add(item);
                }
            }
            //添加
            if (m_AllPrefabDir.ContainsKey(obj.name))
            {
                Debug.LogError("prefab重复！");
            }
            else
            {
                m_AllPrefabDir.Add(obj.name, depenList);
            }
        }
    }

    //设置资源所属ab包
    static void SetAB(string abName,string path)
    {
        AssetImporter importer = AssetImporter.GetAtPath(path);//通过资源路径得到ab设置包
        if (importer == null) Debug.Log("路径不存在！");
        else importer.assetBundleName = abName;
    }

    //设置资源所属ab包
    static void SetAB(string abName, List<string> paths)
    {
        foreach (string path in paths)
        {
            SetAB(abName, path);
        }
    }
    //初始化
    static void Init()
    {
        m_ConfigFil.Clear();
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
    }

    //是否在已有的ab包里，ab包冗余剔除
    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path == m_AllFileAB[i] ||
                (m_AllFileAB[i].Contains(path) && (m_AllFileAB[i].Replace(path, "")[0] == '/')))
            {
                return true;
            }
        }

        return false;
    }

    //有效路径校验
    static bool ValidPath(string path)
    {
        foreach (string temp in m_ConfigFil)
        {
            if (path.Contains(temp)) return true;
        }
        return false;
    }
}