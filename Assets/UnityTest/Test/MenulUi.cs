//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MenulUi : Window
//{
//    private MenuPanel m_menuPanel;

//    public override void Awake(params object[] paramList)
//    {
//        m_menuPanel = GameObject.GetComponent<MenuPanel>(); //获得配置panel
//        AddButtonClickListener(m_menuPanel.StartButton, OnClickStartBtn);
//        AddButtonClickListener(m_menuPanel.LoadButton, OnClickLoadBtn);
//        AddButtonClickListener(m_menuPanel.ExitButton, OnClickExitBtn);
//        ResourceManger.Instance.AsyncLoadResource("Assets/GameData/UGUI/icons_stats_deploy_time.png", CallBack1,LoadResPriority.RES_MIDDLE, true);
//        ResourceManger.Instance.AsyncLoadResource("Assets/GameData/UGUI/icons_stats_dmg.png", CallBack2, LoadResPriority.RES_SLOW, true);
//        ResourceManger.Instance.AsyncLoadResource("Assets/GameData/UGUI/icons_stats_dmg_area.png", CallBack3, LoadResPriority.RES_MIDDLE, true);

//    }


//    public void CallBack1(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
//    {
//        if (obj != null)
//        {
//            m_menuPanel.img1.sprite = obj as Sprite;
//            Debug.Log("图片1加载完成");
//        }
//    }

//    public void CallBack2(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
//    {
//        if (obj != null)
//        {
//            m_menuPanel.img2.sprite = obj as Sprite;
//            Debug.Log("图片2加载完成");
//        }
//    }

//    public void CallBack3(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
//    {
//        if (obj != null)
//        {
//            m_menuPanel.img3.sprite = obj as Sprite;
//            Debug.Log("图片3加载完成");
//        }
//    }

//    void OnClickStartBtn()
//    {
//        Debug.Log("开始");
//    }

//    void OnClickLoadBtn()
//    {
//        Debug.Log("继续");
//    }

//    void OnClickExitBtn()
//    {
//        Debug.Log("退出");
//    }
//}
