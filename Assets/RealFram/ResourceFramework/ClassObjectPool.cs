using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T:class ,new()
{
    protected Stack<T> m_Pool = new Stack<T>();//对象池
    protected int m_MaxCount = 0;//最大对象数目
    protected int m_NoecycleCount = 0;//待回收的对象数

    public ClassObjectPool(int maxcount)
    {
        this.m_MaxCount = maxcount;
        for (int i = 0; i < maxcount; i++)
        {
            m_Pool.Push(new T());
        }
    }

    /// <summary>
    /// 从池里面取类对象
    /// </summary>
    /// <param name="createIfPoolEmpty">如果为空是否new出来 默认为true</param>
    /// <returns></returns>
    public T Spawn(bool createIfPoolEmpty = true)
    {
        T obj = null;
        if (m_Pool.Count > 0)
        {
            obj = m_Pool.Pop();
            if (obj == null)
            {
                if (createIfPoolEmpty)
                {
                    obj = new T();
                    m_NoecycleCount++;
                }
            }
            else m_NoecycleCount++;

        }
        else
        {
            if (createIfPoolEmpty)
            {
                obj = new T();
                m_NoecycleCount++;
            }
        }
        return obj;
    }

    /// <summary>
    /// 将对象还给资源池
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool Recycle(T obj)
    {
        if (obj == null) return false;
        if (m_MaxCount == 0 && m_Pool.Count >= m_MaxCount)
        {
            //如果够了就不需要额外回收
            obj = null;
            return false;
        }

        m_NoecycleCount--;
        m_Pool.Push(obj);
        return true;
    }
}
