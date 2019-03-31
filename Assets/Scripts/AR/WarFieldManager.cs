﻿using ProtoUser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 战斗场景的基本控制
/// 场景管理
/// 小兵管理
/// </summary>
public class WarFieldManager : MonoSingleton<WarFieldManager>
{
    #region 场景管理 
    [Tooltip("为了突出战斗场景的背景")]
    public GameObject BGRFX = null;

    [Tooltip("战斗场景上升速度")]
    public float RiseSpeed = 1f;

    [Tooltip("战斗场景最大旋转角度")]
    public float MaxRotateAngle = 180f;

    [Tooltip("战场缩放和旋转UI")]
    public GameObject AR_UI;

    /// <summary>
    /// 战场初始位置
    /// </summary>
    private Vector3 originalLocalPosition;

    /// <summary>
    /// 战场初始大小
    /// </summary>
    private Vector3 originalLocalScale;

    /// <summary>
    /// 战场初始旋转y分量
    /// </summary>
    private float yLocalAngle;
    #endregion

    [Tooltip("蓝方士兵根节点")]
    public Transform BlueCamp;

    [Tooltip("红方士兵根节点")]
    public Transform RedCamp;

    [Tooltip("射击按钮")]
    public Button ShootButton;

    #region 蔡林烽
    /// <summary>
    /// 当前生产小兵的ID
    /// </summary>
    private int currentSoilderID = 0;
    #endregion

    #region 李锐
    private const string ThiefPrefab = "Assets/GameData/Prefabs/AR/thief.prefab";
    private const string PolicePrefab = "Assets/GameData/Prefabs/AR/police.prefab";
    private const string RomanPrefab = "Assets/GameData/Prefabs/AR/roman.prefab";
    private const string ShamanPrefab = "Assets/GameData/Prefabs/AR/shaman.prefab";

    public int SoilderCount = 0;  //当前士兵数量
    private bool NewRoundStart = false; //是否开启新回合
    private int RoundNow = 0;   //当前回合，0为初始值

    private EMbattle MyFormation = DataLocal.Instance.PLAYERINFO;
    private EMbattle YouFormation = DataLocal.Instance.ENEMYINFO;
    #endregion

    /// <summary>
    /// 红方士兵存储
    /// </summary>
    private Dictionary<string, int> RedCampSoildersDictionary = new Dictionary<string, int>();

    private Dictionary<string, int> BlueCampSoildersDictionary = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;
        yLocalAngle = transform.localEulerAngles.y;
    }

    private void Start()
    {
        RedCampSoildersDictionary.Add("Assets/GameData/Prefabs/AR/RedThief.prefab", 1);
        BlueCampSoildersDictionary.Add("Assets/GameData/Prefabs/AR/BlueThief.prefab", 1);
        
        // 如果当前设备属于蓝方阵营，将蓝方的HeroController.cs中的SendMessage
        // 添加到射击按钮点击事件中
        if(DataLocal.Instance.MyCamp==WarData.Types.CampState.Blue)
        {
            HeroController hc = BlueCamp.GetComponent<HeroController>();
            ShootButton.onClick.AddListener(hc.SendMessage);
        }
        else
        {
            HeroController hc = RedCamp.GetComponent<HeroController>();
            ShootButton.onClick.AddListener(hc.SendMessage);
        }
        StartCoroutine(WaitForNewRound());
    }

    #region 李锐
    private void OnMessage(object[] paramalist)
    {
        NewRoundStart = true;
        RoundNow++;
    }

    IEnumerator WaitForNewRound()
    {
        yield return new WaitUntil(() => NewRoundStart);
        NewRoundStart = false;
        StartSpawnSoilders();
        StartCoroutine(WaitForNewRound());
    }
    #endregion

    void Update ()
    {
        //测试：点击生产小兵
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
        {
            RoundNow++;
            StartSpawnSoilders();
        }
#endif

//#if UNITY_ANDROID
//        foreach (Touch touch in Input.touches)
//        {
//            if (touch.phase == TouchPhase.Began)
//            {
//                StartSpawnSoilders();
//            }
//        }
//#endif
        #region 当战场上升到一定高度，就停止上升，销毁背景,开始生产小兵
        if (Mathf.Abs(transform.localPosition.y - originalLocalPosition.y) < 0.01f)
        {
            if (BGRFX != null)
            {
                #region 李锐
                System_Event.m_Events.AddListener(System_Event.GAMENEWROUND, OnMessage);
                SocketClient.Instance.SendAsyn(_RequestType.ISREADY);
                #endregion

                Destroy(BGRFX);
            }
            if (AR_UI != null)
                AR_UI.SetActive(true);
            
            return;
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, RiseSpeed * Time.deltaTime);
        #endregion
    }


    public void StartSpawnSoilders()
    {
        Transform CampTrans = DataLocal.Instance.MyCamp == WarData.Types.CampState.Red ? RedCamp : BlueCamp;
        StartSpawnSoilders(CampTrans, MyFormation);     //我方出兵
        CampTrans = CampTrans == RedCamp ? BlueCamp : RedCamp;
        StartSpawnSoilders(CampTrans, YouFormation);    //对方出兵
        StartCoroutine(WaitForSoilerZero());
    }

    /// <summary>
    /// 开始生产士兵
    /// </summary>
    /// <param name="CampTrans">阵营父节点</param>
    /// <param name="mbattle">阵形信息</param>
    public void StartSpawnSoilders(Transform CampTrans, EMbattle mbattle)
    {
        float offset = -0.15f;   //初始偏移量
        for (int i = 0; i < 10; i++)
        {
            if (i == 5)
                offset = -0.15f;
            offset += 0.05f;
            GameObject go = null;
            switch (mbattle.Embattle[RoundNow-1][i])
            {
                case ConStr.ArmsCavalry: go = ObjectManger.Instance.InstantiateObject(PolicePrefab); break;
                case ConStr.ArmsMauler: go = ObjectManger.Instance.InstantiateObject(ThiefPrefab); break;
                case ConStr.ArmsBowmen: go = ObjectManger.Instance.InstantiateObject(ShamanPrefab); break;
                case ConStr.ArmsInfantry: go = ObjectManger.Instance.InstantiateObject(RomanPrefab); break;
                case ConStr.ArmsNull: break;
                default: break;
            }
            if (go == null)
                continue;
            else
            {
                SoilderCount++;
                if (CampTrans == RedCamp)
                {
                    go.AddComponent<RedCampSoilderController>();
                    go.GetComponent<RedCampSoilderController>().NodeIndex = 0;
                    go.GetComponent<RedCampSoilderController>().OffSet = offset;
                    go.GetComponent<RedCampSoilderController>().ID = currentSoilderID % int.MaxValue;
                    go.GetComponent<RedCampSoilderController>().Camp = WarData.Types.CampState.Red;
                    currentSoilderID += 1;
                }
                else
                {
                    go.AddComponent<BlueCampSoilderController>();
                    go.GetComponent<BlueCampSoilderController>().NodeIndex = 0;
                    go.GetComponent<BlueCampSoilderController>().OffSet = offset;
                    go.GetComponent<BlueCampSoilderController>().ID = currentSoilderID % int.MaxValue;
                    go.GetComponent<BlueCampSoilderController>().Camp = WarData.Types.CampState.Blue;
                    currentSoilderID += 1;
                }
                go.transform.SetParent(CampTrans);
                go.transform.localPosition = Vector3.zero + Vector3.forward * offset;

                //#region 蔡林烽
                //go.GetComponent<SoilderController>().ID = currentSoilderID % int.MaxValue;
                //currentSoilderID += 1;
                //#endregion

            }
        }
    }

    #region 李锐
    IEnumerator WaitForSoilerZero()
    {
        Debug.Log("开启协程");
        yield return new WaitUntil(() => SoilderCount == 0);
        SocketClient.Instance.SendAsyn(_RequestType.ISREADY);
    }
    #endregion

    #region 场景缩放及旋转
    public void RotateWarField(float factor)
    {
        transform.localEulerAngles  = new Vector3(0, yLocalAngle + factor * MaxRotateAngle, 0);
    }

    public void ScaleWarField(float factor)
    {
        float xScale = originalLocalScale.x * (factor + 1);
        float yScale = originalLocalScale.y;
        float zScale = originalLocalScale.z * (factor + 1);
        transform.localScale = new Vector3(xScale,yScale,zScale);
    }
    #endregion
}