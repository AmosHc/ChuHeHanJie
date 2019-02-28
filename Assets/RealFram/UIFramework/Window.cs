
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window {

	public GameObject GameObject { get; set; }//引用obj
    public Transform Transform { get; set; }//引用transform
    public string Name { get; set; }//引用transform
    protected List<Button> m_AllButton = new List<Button>();//所有的button
    protected List<Toggle> m_Toggle = new List<Toggle>();//所有的toggle
    

    //事件调用顺序
    //Awake -> OnShow -> OnUpdate -> OnDisAble -> OnClose

    public virtual void Awake(params object[] paramList)
    {

    }

    public virtual void OnShow(params object[] paramList)
    {

    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnDisable()
    {

    }

    public virtual void OnClose()
    {
        RemoveAllButtonListener();
        RemoveAllToggleListener();
        m_AllButton.Clear();
        m_Toggle.Clear();
    }
    
    public virtual bool OnMessage(UIMsgID msgId, object[] paramList)
    {
        return true;
    }

    /// <summary>
    /// 设置图像(同步加载)
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="img">图片控件</param>
    /// <param name="setNativeSize">是否调整为最佳大小</param>
    public bool ChangeImageSprite(string path, Image img, bool setNativeSize)
    {
        if (img == null) return false;
        Sprite sp = ResourceManger.Instance.LoadResource<Sprite>(path);
        if (sp != null)
        {
            if (img.sprite != null)
                img.sprite = null;
            img.sprite = sp;
            if(setNativeSize)
                img.SetNativeSize();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="img">图片控件</param>
    /// <param name="setNativeSize">是否调整为最佳大小</param>
    /// <returns></returns>
    public void ChangeImageSpriteAsync(string path, Image img, bool setNativeSize)
    {
        if (img == null) return;
        ResourceManger.Instance.AsyncLoadResource(path,OnLoadSpriteFinish,LoadResPriority.RES_MIDDLE,img,setNativeSize);
       
    }
    /// <summary>
    /// 图片加载完成后的回调
    /// </summary>
    public void OnLoadSpriteFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            Image img = param1 as Image;
            bool setNativeSize = (bool)param2;

            if (sp != null)
            {
                if (img.sprite != null)
                    img.sprite = null;
                img.sprite = sp;
                if (setNativeSize)
                    img.SetNativeSize();
                return;
            }
        }
    }


    /// <summary>
    /// 移除所有按钮点击事件
    /// </summary>
    public void RemoveAllButtonListener()
    {
        for (int i = 0; i < m_AllButton.Count; i++)
        {
            m_AllButton[i].onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// 移除所有单选状态改变事件
    /// </summary>
    public void RemoveAllToggleListener()
    {
        for (int i = 0; i < m_Toggle.Count; i++)
        {
            m_Toggle[i].onValueChanged.RemoveAllListeners();
        }
    }
    /// <summary>
    /// 添加按钮事件监听
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="action"></param>
    public void AddButtonClickListener(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            if (!m_AllButton.Contains(btn))
                m_AllButton.Add(btn);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(ButtonPlaySound);
        }

    }

    /// <summary>
    /// 添加单选按钮事件监听
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="action"></param>
    public void AddToggleClickListener(Toggle toggle,UnityEngine.Events.UnityAction<bool> action)
    {
        if (toggle != null)
        {
            if (!m_Toggle.Contains(toggle))
                m_Toggle.Add(toggle);
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(action);
            toggle.onValueChanged.AddListener(TogglePlaySound);
        }
           
    }
    /// <summary>
    /// 播放按钮声音
    /// </summary>
    public void ButtonPlaySound()
    {

    }

    /// <summary>
    /// 播放toggle声音
    /// </summary>
    /// <param name="isOn"></param>
    public void TogglePlaySound(bool isOn)
    {

    }
}
