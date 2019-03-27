using ProtoUser;
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

    #region 李锐
    private const string ThiefPrefab = "Assets/GameData/Art/Character/Prefab/Hero/thief.prefab";
    private const string PolicePrefab = "Assets/GameData/Art/Character/Prefab/Hero/police.prefab";
    private const string RomanPrefab = "Assets/GameData/Art/Character/Prefab/Hero/roman.prefab";
    private const string ShamanPrefab = "Assets/GameData/Art/Character/Prefab/Hero/shaman.prefab";

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

        #region 李锐
        System_Event.m_Events.AddListener(System_Event.GAMENEWROUND, OnMessage);
        SocketClient.Instance.SendAsyn(_RequestType.ISREADY);
        #endregion
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
            
    }

    #region 李锐
    private void OnMessage(object[] paramalist)
    {
        NewRoundStart = true;
        RoundNow++;
        StartCoroutine(WaitForNewRound());
    }

    IEnumerator WaitForNewRound()
    {
        yield return new WaitUntil(() => NewRoundStart);
        NewRoundStart = false;
        StartSpawnSoilders();

    }
    #endregion

    void Update ()
    {
        //测试：点击生产小兵
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
            StartSpawnSoilders();
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
                Destroy(BGRFX);
            if (AR_UI != null)
                AR_UI.SetActive(true);
            return;
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, RiseSpeed * Time.deltaTime);
        #endregion
    }


    public void StartSpawnSoilders()
    {
        StartSpawnSoilders(0);
        StartSpawnSoilders(0.2f);
    }

    /// <summary>
    /// 开始生产士兵
    /// </summary>
    public void StartSpawnSoilders(float offset)
    {
        foreach (KeyValuePair<string, int> pair in RedCampSoildersDictionary)
        {
            int count = pair.Value;
            for (int i = 0; i < count; i++)
            {
                GameObject go = ObjectManger.Instance.InstantiateObject(pair.Key);

                //士兵放回对象池中的时候，士兵的SoilderController.cs代码只会执行
                //Update函数，为了让士兵可以按照节点正常行走，需要重置NodeIndex。
                go.GetComponent<SoilderController>().NodeIndex = 0;
                go.GetComponent<SoilderController>().OffSet = offset;
                go.transform.SetParent(RedCamp);
                go.transform.localPosition = Vector3.zero + Vector3.forward * offset;

            }
        }

        foreach (KeyValuePair<string, int> pair in BlueCampSoildersDictionary)
        {
            int count = pair.Value;
            for (int i = 0; i < count; i++)
            {
                GameObject go = ObjectManger.Instance.InstantiateObject(pair.Key);
                go.GetComponent<SoilderController>().NodeIndex = 0;
                go.GetComponent<SoilderController>().OffSet = offset;
                go.transform.SetParent(BlueCamp);
                go.transform.localPosition = Vector3.zero + Vector3.forward * offset;
            }
        }
    }

    #region 李锐
    IEnumerator WaitForSoilerZero()
    {
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