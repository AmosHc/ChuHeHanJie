using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class System_Event
{
    public const string GAMEPLAYERDATA = "gameplayerdata";
    public const string GAMESOILDERDATA = "gamesoilderdata";
    public const string GAMENEWROUND = "gamenewround";
    public const string GAMEBULLETDATA = "gamebulletdata";

    public delegate void m_eventCallback(params object[] paramslist);

    private static System_Event m_event;
    public static System_Event m_Events
    {
        get
        {
            if (m_event == null)
                m_event = new System_Event();
            return m_event;
        }
    }

    private Dictionary<string, List<m_eventCallback>> dic_event = new Dictionary<string, List<m_eventCallback>>();

    public void AddListener(string _type,m_eventCallback eventCallback)
    {
        if (!dic_event.ContainsKey(_type))
        {
            List<m_eventCallback> callbacks = new List<m_eventCallback>();
            dic_event.Add(_type, callbacks);
        }
        dic_event[_type].Add(eventCallback);
    }

    public bool RemoveListener(string _type)
    {
        if (dic_event.ContainsKey(_type))
        {
            dic_event.Remove(_type);
            return true;
        }
        else
            return false;
    }

    public void RemoveAllListener()
    {
        dic_event.Clear();
    }

    public bool Dispatche(string _type,params object []paramslist)
    {

        if (dic_event.ContainsKey(_type))
        {
            foreach (m_eventCallback callback in dic_event[_type])
                callback(paramslist);
            return true;
        }
        else
            return false;
    }
}
