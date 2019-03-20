using Ricimi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWindow : Window
{
    protected List<AnimatedButton> m_AllUIBehaviour = new List<AnimatedButton>();//所有的动画button
    /// <summary>
    /// 提供一个方法给重写了ui按钮的UIBehaviour类
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="action"></param>
    public void AddButtonClickListener(AnimatedButton btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            if (!m_AllUIBehaviour.Contains(btn))
                m_AllUIBehaviour.Add(btn);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(ButtonPlaySound);
        }
    }
    /// <summary>
    /// 提示信息（调用前确认已经添加提示窗体）
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="content">内容</param>
    public void Toast(string title,string content)
    {
        UIManager.Instance.PopUpWnd(this,ConStr.ALERTPANEL,title,content);
    }
}
