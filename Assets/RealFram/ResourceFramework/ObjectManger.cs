using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManger : Singleton<ObjectManger>
{
    //设置隐藏的objc资源池回收节点
    public Transform RecyclePoolTrs;
    //场景节点
    public Transform SceenTrs;

    //资源池
    protected Dictionary<uint,List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
    //ResourceObj资源池
    protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool = null;
    //已加载的obj资源
    protected Dictionary<long, ResourceObj> m_ResourceObjDic = new Dictionary<long, ResourceObj>();
    //根据异步加载的guid来判断是否正在加载中
    protected Dictionary<long, ResourceObj> m_AsyncResObjs = new Dictionary<long, ResourceObj>();
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="recycleTrs">回收节点</param>
    /// <param name="sceenTrs">场景节点</param>
    public void Init(Transform recycleTrs,Transform sceenTrs)
    {
        m_ResourceObjClassPool = ObjectManger.Instance.GetOrCreateClassPool<ResourceObj>(1000);//初始化池对象
        this.RecyclePoolTrs = recycleTrs;
        this.SceenTrs = sceenTrs;
    }
    /// <summary>
    /// 清楚资源池缓存
    /// </summary>
    public void ClearCache()
    {
        List<uint> tempList = new List<uint>();

        foreach (uint crc in m_ObjectPoolDic.Keys)
        {
            List<ResourceObj> resObjs = m_ObjectPoolDic[crc];
            for (int i = resObjs.Count - 1; i >= 0; i--)
            {
                ResourceObj item = resObjs[i];
                if (!System.Object.ReferenceEquals(item, null) && resObjs[i].m_bClear)
                {
                    GameObject.Destroy(item.m_CloneObj);
                    m_ResourceObjDic.Remove(item.m_CloneObj.GetInstanceID());
                    item.Reset();
                    m_ResourceObjClassPool.Recycle(item);
                    resObjs.Remove(item);
                }
            }

            if (resObjs.Count <= 0)
                tempList.Add(crc);
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            if(m_ObjectPoolDic.ContainsKey(tempList[i]))
                m_ObjectPoolDic.Remove(tempList[i]);
        }
        tempList.Clear();
    }
    /// <summary>
    /// 清除某个资源在对象中所有的对象
    /// </summary>
    /// <param name="crc"></param>
    public void ClearPoolObject(uint crc)
    {
        List<ResourceObj> resObjs = new List<ResourceObj>();
        if (!m_ObjectPoolDic.TryGetValue(crc, out resObjs) || resObjs == null)
            return;
        for (int i = resObjs.Count - 1 ; i >= 0; i--)
        {
            if (resObjs[i].m_bClear)
            {
                //销毁对象
                GameObject.Destroy(resObjs[i].m_CloneObj);
                m_ResourceObjDic.Remove(resObjs[i].m_CloneObj.GetInstanceID());
                resObjs.Remove(resObjs[i]);
                resObjs[i].Reset();
                m_ResourceObjClassPool.Recycle(resObjs[i]);
            }
        }

        if (resObjs.Count <= 0)
        {
            m_ObjectPoolDic.Remove(crc);
        }
    }

    /// <summary>
    /// 查询资源池中是否有对应资源，取得对象
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    protected ResourceObj GetObjectFromPool(uint crc)
    {
        //查询是否已经缓存了资源，如果有直接返回
        List<ResourceObj> resourceObjs = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out resourceObjs) && resourceObjs != null && resourceObjs.Count > 0)
        {
            //resMgr引用计数
            ResourceManger.Instance.IncreaseResourceRef(crc);
            //取得缓存的obj
            ResourceObj resObj = resourceObjs[0];
            resourceObjs.RemoveAt(0);
            GameObject obj = resObj.m_CloneObj;
            //校验是否为null，比obj == null还快。
            if (!System.Object.ReferenceEquals(obj, null))
            {
                resObj.m_Already = false;
                if(resObj.m_OffineData != null)
                    resObj.m_OffineData.ResetProp();//还原
                //if(!obj.activeSelf)
                //    obj.SetActive(true);
#if UNITY_EDITOR
                //编辑器模式下为了方便查看，改个名，如果是实机模式下改名消耗gc，所以不改名。
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }

            return resObj;
        }
        return null;
    }
    /// <summary>
    /// 获得这个游戏对象的离线数据，（getCompute耗时）
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public OffineData FindOffineData(GameObject obj)
    {
        ResourceObj resObj = null;
        if (m_ResourceObjDic.TryGetValue(obj.GetInstanceID(), out resObj) && resObj != null)
        {
            return resObj.m_OffineData;
        }

        return null;
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="count">预加载个数</param>
    /// <param name="clear">调场景是否清空</param>
    public void PreLoadGameObject(string path, int count = 1, bool clear = false)
    {
        List<GameObject> tempList = new List<GameObject>();
        //不一个循环里销毁的原因，如果拿了就清，一直操作的就是第一个资源池对象，没有预加载效果
        for (int i = 0; i < count; i++)
        {
            tempList.Add(InstantiateObject(path, false, clear));
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            ReleaseObject(tempList[i]);
        }
        tempList.Clear();
    }

    /// <summary>
    /// 同步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="setSceneObj">是否设置父节点为场景节点</param>
    /// <param name="bClear">跳场景是否清楚，默认清除</param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
    {
        uint crc = Crc.GetCRC32(path);
        ResourceObj resObj = GetObjectFromPool(crc);

        if (resObj == null)
        {
            resObj = m_ResourceObjClassPool.Spawn();
            resObj.m_Crc = crc;
            resObj.m_bClear = bClear;
            //resMgr提供加载方法
            resObj = ResourceManger.Instance.LoadResource(path, resObj);
            if (resObj.m_ResourceItem.m_obj != null)
            {
                resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResourceItem.m_obj) as GameObject;
                resObj.m_OffineData = resObj.m_CloneObj.GetComponent<OffineData>();
            }
        }

        if (setSceneObj)
        {
            //因为SetParent十分耗费性能，所以加一个开关
            resObj.m_CloneObj.transform.SetParent(SceenTrs,true);
        }

        //resObj.m_OffineData.ResetProp();//还原
        long guid = resObj.m_CloneObj.GetInstanceID();
        if (!m_ResourceObjDic.ContainsKey(guid))
            m_ResourceObjDic.Add(guid, resObj);

        return resObj.m_CloneObj;
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="dealFinish"></param>
    /// <param name="priority"></param>
    /// <param name="setSceneObject"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="bClear"></param>
    public long InstantiateObject(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority,
        bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null, bool bClear = true)
    {
        if (string.IsNullOrEmpty(path)) return 0;
        uint crc = Crc.GetCRC32(path);
        ResourceObj resObj = GetObjectFromPool(crc);//检查缓存中是否存在
        if (resObj != null)
        {
            if(setSceneObject)
                resObj.m_CloneObj.transform.SetParent(SceenTrs);
            if (dealFinish != null) 
                dealFinish(path, resObj.m_CloneObj, param1, param2, param3);
            return resObj.m_Guid;
        }

        long guid = ResourceManger.Instance.CreatGuid();
        //不存在则创建
        resObj = m_ResourceObjClassPool.Spawn();
        resObj.m_Crc = crc;
        resObj.m_DealFinish = dealFinish;
        resObj.m_bClear = bClear;
        resObj.m_SetSceneParent = setSceneObject;
        resObj.m_Param1 = param1;
        resObj.m_Param2 = param2;
        resObj.m_Param3 = param3;
        //resMgr异步加载
        ResourceManger.Instance.AsyncLoadResource(path,resObj, OnLoadResourceObjFinish , priority);
        return guid;
    }
    /// <summary>
    /// resobj加载完成后回调
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="resObj">中间类</param>
    /// <param name="param1">参数1</param>
    /// <param name="param2">参数2</param>
    /// <param name="param3">参数3</param>
    protected void OnLoadResourceObjFinish(string path, ResourceObj resObj)
    {
        if (resObj == null) return;
        if (resObj.m_ResourceItem.m_obj == null)
        {
#if UNITY_EDITOR
            Debug.Log("异步加载的资源为空！"+path);
#endif
        }
        else
        {
            resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResourceItem.m_obj) as GameObject;
            resObj.m_OffineData = resObj.m_CloneObj.GetComponent<OffineData>();
        }

        //加载完成后就可以从正在加载列表移除
        if (m_AsyncResObjs.ContainsKey(resObj.m_Guid))
            m_AsyncResObjs.Remove(resObj.m_Guid);

        if (resObj.m_SetSceneParent)
        {
            resObj.m_CloneObj.transform.SetParent(SceenTrs,false);
        }
        //resObj.m_OffineData.ResetProp();//还原
        //调回调
        if (resObj.m_DealFinish != null)
        {
            long tempID = resObj.m_CloneObj.GetInstanceID();
            if(!m_ResourceObjDic.ContainsKey(tempID))
                m_ResourceObjDic.Add(tempID,resObj);
            resObj.m_DealFinish(path, resObj.m_CloneObj, resObj.m_Param1, resObj.m_Param2, resObj.m_Param3);
        }

    }
    /// <summary>
    /// 外部调用，是否正在异步加载 true正在，false不在
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool IsingAsyncLoad(long guid)
    {
        return m_AsyncResObjs[guid] != null;
    }
    /// <summary>
    /// 检查对象是否是objectMgr所创建
    /// </summary>
    /// <returns></returns>
    public bool IsObjectManagerCreat(GameObject obj)
    {
        if (m_ResourceObjDic.ContainsKey(obj.GetInstanceID()))
            return true;
        else
            return false;
    }

    /// <summary>
    /// 取消异步加载
    /// </summary>
    /// <param name="guid"></param>
    public void CancleLoad(long guid)
    {
        ResourceObj resObj = null;
        if (m_AsyncResObjs.TryGetValue(guid, out resObj) && ResourceManger.Instance.CancleLoad(resObj))
        {
            resObj.Reset();
            m_AsyncResObjs.Remove(guid);
            m_ResourceObjClassPool.Recycle(resObj);
        }
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    /// <param name="obj">删除的游戏对象</param>
    /// <param name="maxCacheCount">最大缓存个数,-1代表不限</param>
    /// <param name="destoryCache">是否需要缓存</param>
    /// <param name="recycleParent">回收的gameobject是否回收到回收节点下</param>
    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destoryCache = false, bool recycleParent = true)
    {
        if (obj == null) return;
        long tempID = obj.GetInstanceID();
        ResourceObj resObj = null;
        if (!m_ResourceObjDic.TryGetValue(tempID , out resObj))
        {
            Debug.Log(obj.name+ "对象不是ObjectManger创建的！");
            return;
        }

        if (resObj == null)
        {
            Debug.Log("缓存的resouceObj为空");
            return;
        }

        if (resObj.m_Already)
        {
            Debug.Log("该对象已被回收，检测是否清空引用！");
            return;
        }

#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        if (maxCacheCount == 0)
        {
            m_ResourceObjDic.Remove(tempID);
            ResourceManger.Instance.ReleaseResouce(resObj, destoryCache);
            //资源回收
            resObj.Reset();
            m_ResourceObjClassPool.Recycle(resObj);
        }
        else
        {
            List<ResourceObj> objList = null;
            //回收到对象池
            if (!m_ObjectPoolDic.TryGetValue(resObj.m_Crc, out objList) || objList == null)
            {
                objList = new List<ResourceObj>();
                m_ObjectPoolDic.Add(resObj.m_Crc,objList);
            }

            if (resObj.m_CloneObj != null)
            {
                if (recycleParent) 
                    resObj.m_CloneObj.transform.SetParent(RecyclePoolTrs);//回收到回收节点
                else
                    resObj.m_CloneObj.SetActive(false);//隐藏
            }

            //如果缓存个数没达到上限则添加缓存
            if (maxCacheCount < 0 || objList.Count < maxCacheCount)
            {
                objList.Add(resObj);
                resObj.m_Already = true;
                //resMgr引用计数
                ResourceManger.Instance.DecreaseResourceRef(resObj);
            }
            else
            {
                m_ResourceObjDic.Remove(tempID);
                ResourceManger.Instance.ReleaseResouce(resObj, destoryCache);
                //资源回收
                resObj.Reset();
                m_ResourceObjClassPool.Recycle(resObj);
            }
        }
    }

    #region 类对象池的使用
    Dictionary<Type,object> m_ClassPoolDic = new Dictionary<Type, object>();//按类型存储的池字典
    /// <summary>
    /// 获得或创建一个类对象池,创建后外界可以调用ClassObjectPool<T>,然后调用spwn和recycle来创建和回收类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxcount"></param>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class , new()
    {
        Type type = typeof(T);
        object pool = null;
        if (!m_ClassPoolDic.TryGetValue(type,out pool) || pool == null)
        {
            pool = new ClassObjectPool<T>(maxcount);
            m_ClassPoolDic.Add(type,pool);
        }

        return pool as ClassObjectPool<T>;
    }
    #endregion
}
