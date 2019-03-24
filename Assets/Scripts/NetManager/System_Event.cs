using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class System_Event
{
    public const string GAMEPLAYERDATA = "gameplayerdata";
    public const string GAMESOILDERDATA = "gamesoilderdata";

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

    private Dictionary<string, m_eventCallback> dic_event = new Dictionary<string, m_eventCallback>();

    public bool AddListener(string _type,m_eventCallback eventCallback)
    {
        if (!dic_event.ContainsKey(_type))
        {
            dic_event.Add(_type, eventCallback);
            return true;
        }
        else
            return false;
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
            dic_event[_type](paramslist);
            return true;
        }
        else
            return false;
    }
}
