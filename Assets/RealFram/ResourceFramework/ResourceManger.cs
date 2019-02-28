using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 异步加载优先级枚举类型
/// </summary>
public enum LoadResPriority
{
    RES_HIGH = 0,//高优先级
    RES_MIDDLE,//一般优先级
    RES_SLOW,//低优先级
    RES_NUM,//数量
}
/// <summary>
/// 资源obj块
/// </summary>
public class ResourceObj
{
    public uint m_Crc = 0;//路径对应crc
    public ResourceItem m_ResourceItem;//缓存resItem
    public GameObject m_CloneObj = null;//实例化的gameobject游戏对象
    public bool m_bClear = true;//是否跳场景清楚
    public long m_Guid = 0; //存储GUID
    public bool m_Already = false;//是否已经释放
    //-------------异步加载参数----------------
    public bool m_SetSceneParent = false;//是否放到场景节点下
    public OnAsyncObjFinish m_DealFinish = null;//回调函数
    public object m_Param1, m_Param2, m_Param3 = null;//参数
    public OffineData m_OffineData = null;//离线数据

    public void Reset()
    {
        m_Crc = 0;
        m_CloneObj = null;
        m_ResourceItem = null;
        m_bClear = true;
        m_Guid = 0;
        m_Already = false;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
        m_OffineData = null;
    }
}

/// <summary>
/// 异步加载资源参数
/// </summary>
public class AsyncLoadResParam
{
    public List<AsyncCallBack> m_CallBack = new List<AsyncCallBack>();
    public string m_Path = "";
    public uint m_Crc = 0;
    public bool m_IsSprite = false;//是否是图片，图片加载方式有异
    public LoadResPriority m_Priority = LoadResPriority.RES_SLOW;

    public void Reset()
    {
        m_CallBack.Clear();
        m_Path = "";
        m_Crc = 0;
        m_IsSprite = false;
        m_Priority = LoadResPriority.RES_SLOW;
    }
}
/// <summary>
/// 每一个回调对应一个回调参数
/// </summary>
public class AsyncCallBack
{
    //回调函数
    public OnAsyncObjFinish m_DealObjFinish = null;
    public object m_Param1 = null, m_Param2 = null, m_Param3 = null;
    //---obj对象异步加载参数---
    public OnAsyncFinish m_DealFinish = null;//加载完成的回调（针对objectMgr）
    public ResourceObj m_ResObj = null;//资源对象
    public void Reset()
    {
        m_DealObjFinish = null;
        m_DealFinish = null;
        m_ResObj = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
    }
}

//异步加载回调函数
public delegate void OnAsyncObjFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);
//实例化对象加载完成回调函数
public delegate void OnAsyncFinish(string path, ResourceObj resObj);

public class ResourceManger : Singleton<ResourceManger>
{
    public bool m_LoadFromAssetBundle = false;//是否是通过ab包加载资源 false 编辑器，true ab包
    //缓存使用的资源列表
    public Dictionary<uint, ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();

    //缓存引用计数为0的资源，保证资源不会被gc，如果达到缓存上限，根据使用时间，释放最早无活动资源。
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
    //mono脚本
    protected MonoBehaviour m_StartMono;
    //正在异步加载的资源列表
    protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
    //正在异步加载的资源dic（检测资源是否正在被加载）
    protected Dictionary<uint,AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();
    //创建资源池
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);

    protected const int MAXCACHECOUNT = 500;//最大无用资源缓存上限，用来清理多余资源
    //异步加载最长等待时间
    private const long MAXLOADRESTIME = 200000;
    private long m_Guid = 0;
    /// <summary>
    /// 创建唯一的Guid
    /// </summary>
    /// <returns></returns>
    public long CreatGuid()
    {
        return m_Guid++;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
        {
            m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }
        m_StartMono = mono;
        m_StartMono.StartCoroutine(AsyncLoadCor());
    }
    /// <summary>
    /// 清空缓存
    /// </summary>
    public void ClearCache()
    {
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach (ResourceItem item in AssetDic.Values)
        {
            if (item.m_Clear)//如果需要清空则清空
            {
                tempList.Add(item);
            }
        }
        //因为无法在遍历时移除assetDic资源，所以需要出来重新循环一次
        for (int i = 0; i < tempList.Count; i++)
        {
            DestroyResouceItem(tempList[i], true);
        }
        
    }

    /// <summary>
    /// 根据obj增加引用计数
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int IncreaseResourceRef(ResourceObj resObj, int count = 1)
    {
        return resObj == null ? 0 : IncreaseResourceRef(resObj.m_Crc, count);
    }
    /// <summary>
    /// 根据路径增加引用计数
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int IncreaseResourceRef(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(crc, out item) || item == null)
            return 0;
        item.RefCount += count;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        return item.RefCount;
    }

    /// <summary>
    /// 根据obj减少引用计数
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int DecreaseResourceRef(ResourceObj resObj, int count = 1)
    {
        return resObj == null ? 0 : DecreaseResourceRef(resObj.m_Crc, count);
    }
    /// <summary>
    /// 根据路径减少引用计数
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int DecreaseResourceRef(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(crc, out item) || item == null)
            return 0;
        item.RefCount -= count;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        return item.RefCount;
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="path"></param>
    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        uint crc = Crc.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc,0);//因为是预加载，不需要引用计数
        if (item != null)
        {
            return;
        }

        //如果不存在则自己加载
        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManger.Instance.FindResourceItem(crc);//查找已有的资源块直接加载

            if (item!=null && item.m_obj != null)
                obj = item.m_obj;
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = Crc.GetCRC32(path);
                }
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManger.Instance.LoadResourceAssetBundle(crc);//在这里加载资源块同时加载ab包
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_obj != null)
                    obj = item.m_obj;
                else
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
            }
        }

        CacheResource(path, ref item, crc, obj);
        //设置跳场景不清空缓存
        item.m_Clear = false;
        ReleaseResouce(obj, false);
    }
    /// <summary>
    /// 同步资源加载，针对ResourceObj
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <returns></returns>
    public ResourceObj LoadResource(string path, ResourceObj resObj)
    {
        if (resObj == null) return null;
        resObj.m_Crc = resObj.m_Crc == 0 ? Crc.GetCRC32(path) : resObj.m_Crc;
        //查看是否在缓存中
        ResourceItem item = GetCacheResourceItem(resObj.m_Crc);
        if (item != null)
        {
            resObj.m_ResourceItem = item;
            return resObj;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManger.Instance.FindResourceItem(resObj.m_Crc);//查找已有的资源块直接加载

            if (item != null && item.m_obj != null)
                obj = item.m_obj;
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = Crc.GetCRC32(path);
                }
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManger.Instance.LoadResourceAssetBundle(resObj.m_Crc);//在这里加载资源块同时加载ab包
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_obj != null)
                    obj = item.m_obj;
                else
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
            }
        }

        CacheResource(path, ref item, resObj.m_Crc, obj);
        item.m_Clear = resObj.m_bClear;
        resObj.m_ResourceItem = item;
        return resObj;
    }

    /// <summary>
    /// 同步资源加载，用于加载非实例化资源，如音频，图片
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
            return null;
        uint crc = Crc.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return item.m_obj as T;
        }

        //如果不存在则自己加载
        T obj = null;
#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManger.Instance.FindResourceItem(crc);//查找已有的资源块直接加载

            if (item!=null && item.m_obj != null)
                obj = item.m_obj as T;
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = Crc.GetCRC32(path);
                }
                obj = LoadAssetByEditor<T>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManger.Instance.LoadResourceAssetBundle(crc);//在这里加载资源块同时加载ab包
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_obj != null)
                    obj = item.m_obj as T;
                else
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
            }
        }

        CacheResource(path, ref item, crc, obj);
        return obj;
    }
    /// <summary>
    /// 根据ResourceObj卸载资源
    /// </summary>
    /// <param name="resObj"></param>
    /// <param name="destoryObj"></param>
    /// <returns></returns>
    public bool ReleaseResouce(ResourceObj resObj, bool destoryObj = false)
    {
        if (resObj == null) return false;
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(resObj.m_ResourceItem.m_Crc, out item))
        {
            Debug.LogError("assetdic里不存在该资源，" + resObj.m_CloneObj.name + "可能被释放了多次");
        }
        GameObject.Destroy(resObj.m_CloneObj);
        item.RefCount--;
        DestroyResouceItem(item, destoryObj);//释放item
        return true;
    }

    /// <summary>
    /// 不需要实例化的资源卸载，根据obj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destoryObj"></param>
    public bool ReleaseResouce(Object obj , bool destoryObj = false)
    {
        if (obj == null) return false;
        ResourceItem res = null;
        foreach (ResourceItem temp in AssetDic.Values)
        {
            if (temp.m_Guid == obj.GetInstanceID()) res = temp;
        }

        if (res == null)
        {
            Debug.LogError("assetdic里不存在该资源，" + obj.name + "可能被释放了多次");
            return false;
        }

        res.RefCount--;
        DestroyResouceItem(res, destoryObj);//释放item
        return true;
    }
    /// <summary>
    ///  不需要实例化的资源卸载，根据路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destoryObj"></param>
    /// <returns></returns>
    public bool ReleaseResouce(string path, bool destoryObj = false)
    {
        if (string.IsNullOrEmpty(path)) return false;
        uint crc = Crc.GetCRC32(path);
        ResourceItem res = null;

        if (!AssetDic.TryGetValue(crc, out res) || null == res)
        {
            Debug.LogError("assetdic里不存在该资源，" + path + "可能被释放了多次");
            return false;
        }
        res.RefCount--;
        DestroyResouceItem(res, destoryObj);//释放item
        return true;
    }
#if UNITY_EDITOR
    /// <summary>
    /// 编辑器下加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    protected T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
# endif
    /// <summary>
    /// 缓存资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    /// <param name="addrefcount"></param>
    private void CacheResource(string path,ref ResourceItem item,uint crc,Object obj,int addrefcount = 1)
    {
        //检查是否需要释放资源
        WashOut();

        if (item == null)
        {
            Debug.LogError("ResourceItem is null,path" + path);
        }

        if (obj == null)
        {
            Debug.LogError("Object is null,path" + path);
        }

        item.RefCount += addrefcount;
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.m_obj = obj;
        item.m_Guid = obj.GetInstanceID();

        if (!AssetDic.ContainsKey(crc))
        {
            AssetDic.Add(crc,item);
        }
        else
        {
            AssetDic[crc] = item;
        }
    }
    /// <summary>
    /// 如果缓存太多则释放最早没用的资源
    /// </summary>
    protected void WashOut()
    {
        while (m_NoRefrenceAssetMapList.Size() >= MAXCACHECOUNT)
        {
            for (int i = 0; i < MAXCACHECOUNT / 2; i++)
            {
                ResourceItem item = m_NoRefrenceAssetMapList.Peek();
                DestroyResouceItem(item);
            }
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroyCache">是否需要销毁，默认缓存</param>
    protected void DestroyResouceItem(ResourceItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0) return;
        if (!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);//没有使用的资源
            return;
        }
        //销毁
        if (!AssetDic.Remove(item.m_Crc)) return;
        m_NoRefrenceAssetMapList.Remove(item);
        AssetBundleManger.Instance.ReleaseAsset(item);//释放引用
        ObjectManger.Instance.ClearPoolObject(item.m_Crc);//清空资源对应的资源池
        if (item.m_obj != null) item.m_obj = null;
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();//编辑器下卸载所有未使用的资源
#endif
    }

    /// <summary>
    /// 获得缓存的item（如果存在）
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="addrefcount"></param>
    /// <returns></returns>
    private ResourceItem GetCacheResourceItem(uint crc,int addrefcount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item) && item != null)
        {
            //已经缓存,更新缓存
            item.RefCount += addrefcount;
            item.m_LastUseTime = Time.realtimeSinceStartup;
        }

        return item;
    }
    /// <summary>
    /// 取消异步加载
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool CancleLoad(ResourceObj resObj)
    {
        AsyncLoadResParam param;
        if (m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out param) &&
            m_LoadingAssetList[(int) param.m_Priority].Contains(param))
        {
            //存在且还存在加载队列时
            for (int i = param.m_CallBack.Count; i >= 0 ; i--)
            {
                AsyncCallBack back = param.m_CallBack[i];
                if (back!=null && back.m_ResObj == resObj)
                {
                    back.Reset();
                    param.m_CallBack.Remove(back);
                    m_AsyncCallBackPool.Recycle(back);
                }
            }

            if (param.m_CallBack.Count <= 0)
            {
                //只有没有任何引用的时候才彻底取消异步，清空param
                param.Reset();
                m_LoadingAssetList[(int) param.m_Priority].Remove(param);
                m_LoadingAssetDic.Remove(resObj.m_Crc);
                m_AsyncLoadResParamPool.Recycle(param);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 异步加载资源（仅用于加载非实例化的资源，如音频，图片等）
    /// </summary>
    public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority, object param1 = null,
        object param2 = null, object param3 = null, uint crc = 0)
    {
        if (crc == 0) crc = Crc.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            //资源存在且回调不为空
            if (dealFinish != null) dealFinish(path, item.m_obj, param1, param2, param3);
        }
        //检测是否正在被加载
        AsyncLoadResParam param = null;
        if (!m_LoadingAssetDic.TryGetValue(crc, out param) || param == null)
        {
            param = m_AsyncLoadResParamPool.Spawn();
            param.m_Crc = crc;
            param.m_Path = path;
            param.m_Priority = priority;
            m_LoadingAssetDic.Add(crc,param);
            m_LoadingAssetList[(int)priority].Add(param);
        }
        //不管有没有正在被加载都要加上一条回调进去
        AsyncCallBack back = m_AsyncCallBackPool.Spawn();
        back.m_DealObjFinish = dealFinish;
        back.m_Param1 = param1;
        back.m_Param2 = param2;
        back.m_Param3 = param3;
        param.m_CallBack.Add(back);
    }

    /// <summary>
    /// 异步加载资源（针对objectMgr）
    /// </summary>
    public void AsyncLoadResource(string path, ResourceObj resObj, OnAsyncFinish dealFinish, LoadResPriority priority)
    {
        if (resObj == null) return;
        ResourceItem item = GetCacheResourceItem(resObj.m_Crc);
        if (item != null)
        {
            //资源存在且回调不为空
            if (dealFinish != null)
                dealFinish(path, resObj);
        }
        //检测是否正在被加载
        AsyncLoadResParam param = null;
        if (!m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out param) || param == null)
        {
            param = m_AsyncLoadResParamPool.Spawn();
            param.m_Crc = resObj.m_Crc;
            param.m_Path = path;
            param.m_Priority = priority;
            m_LoadingAssetDic.Add(resObj.m_Crc, param);
            m_LoadingAssetList[(int)priority].Add(param);
        }
        //不管有没有正在被加载都要加上一条回调进去
        AsyncCallBack back = m_AsyncCallBackPool.Spawn();
        back.m_DealFinish = dealFinish;
        back.m_ResObj = resObj;
        param.m_CallBack.Add(back);
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        long lastYiledTime = System.DateTime.Now.Ticks;//最后一次异步调用时间
        List<AsyncCallBack> callBackList = null;
        while (true)
        {
            bool haveYiled = false;//是否已经return了
            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                if(loadingList.Count <= 0 )continue;
                //取第一个
                AsyncLoadResParam loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callBackList = loadingItem.m_CallBack;
                Object obj = null;
                ResourceItem item = null;
#if UNITY_EDITOR
                if (!m_LoadFromAssetBundle)
                {
                    //编辑器加载
                    obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
                    //模拟异步
                    yield return new WaitForSeconds(0.5f);
                    item = AssetBundleManger.Instance.FindResourceItem(loadingItem.m_Crc);
                    if (item == null)
                    {
                        item = new ResourceItem();
                        item.m_Crc = loadingItem.m_Crc;
                    }
                }
#endif
                if (obj == null)
                {
                    //ab包加载
                    item = AssetBundleManger.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {
                        //获得ab包的异步加载对象
                        AssetBundleRequest abRequest = null;
                        if (loadingItem.m_IsSprite)
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        else
                            abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        yield return abRequest;
                        if (abRequest.isDone)//如果加载完成
                        {
                            obj = abRequest.asset;
                        }

                        lastYiledTime = System.DateTime.Now.Ticks;//更新最后异步时间
                    }   
                }
                CacheResource(loadingItem.m_Path,ref item,loadingItem.m_Crc,obj, callBackList.Count);

                //依次调用回调函数
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsyncCallBack callBack = callBackList[i];

                    if (callBack != null && callBack.m_DealFinish != null && callBack.m_ResObj != null)
                    {
                        callBack.m_ResObj.m_ResourceItem = item;
                        callBack.m_DealFinish(loadingItem.m_Path, callBack.m_ResObj);
                    }

                    if (callBack != null && callBack.m_DealObjFinish != null)
                    {
                        callBack.m_DealObjFinish(loadingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2,
                            callBack.m_Param3);
                    }
                    callBack.Reset();//清空
                    m_AsyncCallBackPool.Recycle(callBack); //返回资源池
                }
                //资源回收
                obj = null;
                callBackList.Clear();
                m_LoadingAssetDic.Remove(loadingItem.m_Crc);
                loadingItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loadingItem);
                //异步等待
                if (System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
                {
                    yield return null;
                    lastYiledTime = System.DateTime.Now.Ticks;
                    haveYiled = true;
                }
            }
            //异步等待
            if (!haveYiled || System.DateTime.Now.Ticks - lastYiledTime > MAXLOADRESTIME)
            {
                lastYiledTime = System.DateTime.Now.Ticks;
                yield return null;
            }
        }
    }
}

public class DoubleLinkedListNode<T> where T: class, new()
{
    public DoubleLinkedListNode<T> prev = null;//前节点
    public DoubleLinkedListNode<T> next = null;//后节点
    public T value = null;//值
} 

public class DoubleLinkedList<T> where T : class, new()
{
    public DoubleLinkedListNode<T> head = null;//头节点
    public DoubleLinkedListNode<T> last = null;//尾节点

    public ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool = ObjectManger.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);//双向链表对象池
    private int m_Count = 0;//列表个数
    public int Count
    {
        get { return m_Count; }
    }

    /// <summary>
    /// 头插法
    /// </summary>
    /// <param name="value">值</param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHeader(T value)
    {
        DoubleLinkedListNode<T> node = m_DoubleLinkNodePool.Spawn();
        node.prev = null;
        node.next = null;
        node.value = value;
        return AddToHeader(node);
    }
    /// <summary>
    /// 头插法
    /// </summary>
    /// <param name="node">节点</param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> node)
    {
        if (node == null) return null;
        node.prev = null;
        node.next = head;
        if (head == null)
        {
            head = last = node;
        }
        else
        {
            head = node;
        }

        m_Count++;
        return head;
    }
    /// <summary>
    /// 尾插法
    /// </summary>
    /// <param name="value">值</param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(T value)
    {
        DoubleLinkedListNode<T> node = m_DoubleLinkNodePool.Spawn();
        node.prev = null;
        node.next = null;
        node.value = value;
        return AddToTail(node);
    }
    /// <summary>
    /// 尾插法
    /// </summary>
    /// <param name="node">节点</param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> node)
    {
        if (node == null) return null;
        node.next = null;
        node.prev = last;
        if (last == null)
        {
            head = last = node;
        }
        else
        {
            last = node;
        }

        m_Count++;
        return last;
    }
    /// <summary>
    /// 移除节点
    /// </summary>
    public void RemoveNode(DoubleLinkedListNode<T> node)
    {
        if (node == null) return;
        if (node == head)
        { 
            head = node.next;
        }

        if (node == last)
        {
            last = node.prev;
        }

        if (node.prev != null)
        {
            node.prev.next = node.next;
        }

        if (node.next != null)
        {
            node.next.prev = node.prev;
        }

        node.prev = null;
        node.next = null;
        node.value = null;
        m_DoubleLinkNodePool.Recycle(node);
        m_Count--;
    }

    /// <summary>
    /// 移动节点到头部
    /// </summary>
    public void MoveToHead(DoubleLinkedListNode<T> node)
    {
        if (node == null || (node.prev == null && node.next == null) || node == head) return;
        if (node.prev != null) node.prev.next = node.next;
        if (node.next != null) node.next.prev = node.prev;
        if (node == last) last = node.prev; //如果是尾节点移动，需要指定新的尾节点
        node.prev = null;
        node.next = head;
        head = node;
    }
}

public class CMapList<T> where T : class, new()
{
    DoubleLinkedList<T> m_Dlink = new DoubleLinkedList<T>();
    Dictionary<T,DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();//搜索map，用于快速查找

    ~CMapList()//析构函数,垃圾回收的时候自动调用
    {
        Clear();
    }
    /// <summary>
    /// 长度
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return m_FindMap.Count;
    }

    /// <summary>
    /// 插入一个节点头表头
    /// </summary>
    /// <param name="value"></param>
    public void InsertToHead(T value)
    {
        DoubleLinkedListNode<T> node = null;

        if (m_FindMap.TryGetValue(value, out node) && node != null)
        {
            m_Dlink.AddToHeader(node);
            return;
        }
        else
        {
            node = m_Dlink.AddToHeader(value);
            m_FindMap.Add(value,node);
        }
    }
    /// <summary>
    /// 从表尾弹出一个节点
    /// </summary>
    public T Pop()
    {
        DoubleLinkedListNode<T> node = m_Dlink.last;
        if (m_Dlink.last != null)
        {
            m_Dlink.RemoveNode(m_Dlink.last);
        }

        return node.value;
    }
    /// <summary>
    /// 删除某个节点
    /// </summary>
    /// <param name="t">节点值</param>
    public void Remove(T value)
    {
        DoubleLinkedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(value, out node) || node == null) return;
        if (m_Dlink.Count <= 0) return;
        m_Dlink.RemoveNode(node);
        m_FindMap.Remove(value);

    }

    /// <summary>
    /// 访问顶部的节点(不弹出)
    /// </summary>
    /// <returns></returns>
    public T Peek()
    {
        if (m_Dlink.last != null) return m_Dlink.last.value;
        else return null;
    }
    /// <summary>
    /// 查找是否存在节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Find(T key)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(key,out node) && node != null) return true;
        else return false;
    }
    /// <summary>
    /// 刷新某个节点，将指定节点移动到头部
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Reflesh(T value)
    {
        DoubleLinkedListNode<T> node = null;
        if (m_FindMap.TryGetValue(value, out node) && node != null)
        {
            m_Dlink.MoveToHead(node);
            return true;
        }
        else return false;
    }
    /// <summary>
    /// 清空链表
    /// </summary>
    public void Clear()
    {
        while (m_Dlink.last != null)
        {
            Remove(m_Dlink.last.value);
        }

    }
}