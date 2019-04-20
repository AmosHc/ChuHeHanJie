using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManger : Singleton<AssetBundleManger>
{
    protected string m_ABConfigABName = "assetbundleconfig";
    protected Dictionary<uint,ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();//资源依赖关系表，根据crc找到对应资源块
    protected Dictionary<uint,AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();//ab包字典集

    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool =
        ObjectManger.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

    public string ABLoadpath
    {
        get { return Application.streamingAssetsPath + "/AssetBundle/"; }
    }
    /// <summary>
    /// 加载AssetBundleConfig
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
        //        TextAsset textAsset = null;
        //#if UNITY_EDITOR
        //            textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(m_AssetConfigPath);
        //#endif
        //        if (textAsset == null)
        //        {
        //            Debug.LogError("assetbundleConfig不存在！"+ m_AssetConfigPath);
        //            return false;
        //        }
#if UNITY_EDITOR
        if (!ResourceManger.Instance.m_LoadFromAssetBundle)
            return false;
#endif
        m_ResourceItemDic.Clear();
        string assetConfigPath = ABLoadpath + m_ABConfigABName;
        AssetBundle abConfigBundle = AssetBundle.LoadFromFile(assetConfigPath);
        TextAsset textAsset = abConfigBundle.LoadAsset<TextAsset>(m_ABConfigABName);
        if (textAsset == null)
        {
            Debug.LogError("assetbundleConfig不存在！" );
            return false;
        }
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        AssetBundleConfig config = formatter.Deserialize(stream) as AssetBundleConfig;
        stream.Close();
        for (int i = 0; i < config.ABList.Count; i++)
        {
            ABBase ab = config.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = ab.Crc;
            item.m_AssetName = ab.AssetName;
            item.m_AssetBundleName = ab.ABName;
            item.m_DependAssetBundle = ab.ABDependce;
            if(m_ResourceItemDic.ContainsKey(item.m_Crc))
                Debug.LogError("重复的Crc 资源名："+ item.m_AssetName +"ab包名："+item.m_AssetBundleName);
            else
                m_ResourceItemDic.Add(item.m_Crc, item);
        }
        return true;
    }

    /// <summary>
    /// 根据路径加载资源块
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem LoadResourceAssetBundle(uint crc)
    {
        ResourceItem res = null;
        if (!m_ResourceItemDic.TryGetValue(crc, out res))
        {
            Debug.LogError("传递的CRC"+crc+"路径不存在于资源依赖表中，请检查参数");
            return null;
        }

        //if (res.m_AssetBundle != null) return res;
        else
        {
            res.m_AssetBundle = LoadAssetBundle(res.m_AssetBundleName);
            if (res.m_AssetBundle == null) return null;
            else
            {
                if (res.m_DependAssetBundle != null && res.m_DependAssetBundle.Count > 0)
                {
                    for (int i = 0; i < res.m_DependAssetBundle.Count; i++)
                    {
                        LoadAssetBundle(res.m_DependAssetBundle[i]);
                    }
                }

                return res;
            }
        }
    }
    /// <summary>
    /// 加载ab包
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public AssetBundle LoadAssetBundle(string abName)
    {
        uint crc = Crc.GetCRC32(abName);
        AssetBundleItem item = null;
        if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
        {
            string path = ABLoadpath + abName;
            AssetBundle ab = null;
            //if (File.Exists(path))//移动端不支持File访问
            ab = AssetBundle.LoadFromFile(path);
            if (ab == null)
            {
                Debug.LogError("ab包加载失败，请检查" + path);
                return null;
            }

            item = m_AssetBundleItemPool.Spawn();
            item.assetBundle = ab;
            item.refCount++;
            m_AssetBundleItemDic.Add(crc,item);

        }
        else
        {
            item.refCount++;
        }
        return item.assetBundle;
    }
    /// <summary>
    /// 资源释放
    /// </summary>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null) return;
        if (item.m_DependAssetBundle != null && item.m_DependAssetBundle.Count > 0)
        {
            for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
            {
                UnLoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }
        UnLoadAssetBundle(item.m_AssetBundleName);
    }

    /// <summary>
    /// 根据ab包名卸载ab包
    /// </summary>
    /// <param name="abName"></param>
    public void UnLoadAssetBundle(string abName)
    {
        uint crc = Crc.GetCRC32(abName);
        AssetBundleItem item = null;
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) || item != null)
        {
            item.refCount--;
            if (item.refCount <= 0 && item.assetBundle != null)
            {
                Debug.Log(abName);
                //没有引用
                item.assetBundle.Unload(true);
                item.Rest();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }
    /// <summary>
    /// 根据crc返回查找的resources
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        ResourceItem item = null;
        m_ResourceItemDic.TryGetValue(crc, out item);
        return item;
    }
}

public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int refCount = 0;//引用个数
    //重置
    public void Rest()
    {
        assetBundle = null;
        refCount = 0;
    }
}

public class ResourceItem
{
    public uint m_Crc = 0;//资源路径的crc
    public string m_AssetName = String.Empty;//资源文件名
    public string m_AssetBundleName = String.Empty;//资源所在assbundle名字
    public List<string> m_DependAssetBundle = null;//该资源依赖的ab包
    public AssetBundle m_AssetBundle;//资源所在的完整的ab包

    //----------------------------------
    public UnityEngine.Object m_obj = null;//资源对象
    public int m_Guid = 0;//对象id
    public float m_LastUseTime = 0.0f;//资源最后使用时间
    protected int m_RefCount = 0;//引用计数
    public bool m_Clear = true;//跳场景是否清理 true清理，false不清理

    public int RefCount
    {
        get { return m_RefCount; }
        set
        {
            m_RefCount = value;
            if (m_RefCount < 0)
            {
                Debug.LogError("refcount < 0 :" + m_RefCount + " ," + ((m_obj == null) ? "对象为空" : m_obj.name));
            }
        }
    }
}
