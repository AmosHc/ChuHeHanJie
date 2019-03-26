using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;
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

    private const string ThiefPrefab = "Assets/GameData/Art/Character/Prefab/Hero/thief.prefab";
    private const string PolicePrefab = "Assets/GameData/Art/Character/Prefab/Hero/police.prefab";
    private const string RomanPrefab = "Assets/GameData/Art/Character/Prefab/Hero/roman.prefab";
    private const string ShamanPrefab = "Assets/GameData/Art/Character/Prefab/Hero/shaman.prefab";

    public int SoilderCount = 0;  //当前士兵数量
    private bool NewRoundStart = false; //是否开启新回合
    private int RoundNow = 0;   //当前回合，0为初始值

    private EMbattle MyFormation = DataLocal.Instance.PLAYERINFO;
    private EMbattle YouFormation = DataLocal.Instance.ENEMYINFO;

    protected override void Awake()
    {
        base.Awake();
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;
        yLocalAngle = transform.localEulerAngles.y;
        System_Event.m_Events.AddListener(System_Event.GAMENEWROUND, OnMessage);
        SocketClient.Instance.SendAsyn(_RequestType.ISREADY);
    }

    private void Start()
    {

        // 如果当前设备属于蓝方阵营，将蓝方的HeroController.cs中的SendMessage
        // 添加到射击按钮点击事件中
        if (DataLocal.Instance.MyCamp == WarData.Types.CampState.Blue)
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

    private void OnMessage(object []paramalist)
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

    void Update ()
    {
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


    /// <summary>
    /// 开始生产士兵
    /// </summary>
    public void StartSpawnSoilders()
    {
        Transform CampTrans = DataLocal.Instance.MyCamp == WarData.Types.CampState.Red ? RedCamp : BlueCamp;
        for (int i = 0; i < 10; i++)
        {
            GameObject go = null;
            switch (MyFormation.Embattle[RoundNow][i])
            {
                case ConStr.ArmsCavalry:go = ObjectManger.Instance.InstantiateObject(PolicePrefab);break;
                case ConStr.ArmsMauler:go = ObjectManger.Instance.InstantiateObject(ThiefPrefab);break;
                case ConStr.ArmsBowmen:go = ObjectManger.Instance.InstantiateObject(ShamanPrefab);break;
                case ConStr.ArmsInfantry:go = ObjectManger.Instance.InstantiateObject(RomanPrefab);break;
                case ConStr.ArmsNull:break;
                default:break;
            }
            if (go == null)
                continue;
            else
            {
                SoilderCount++;
                go.GetComponent<SoilderController>().NodeIndex = 0;
                go.transform.SetParent(CampTrans);
                go.transform.localPosition = Vector3.zero;
            }
        }

        CampTrans = CampTrans == RedCamp ? BlueCamp : RedCamp;
        for (int i = 0; i < YouFormation.Embattle[RoundNow].Length; i++)
        {
            GameObject go = null;
            switch (MyFormation.Embattle[RoundNow][i])
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
                go.GetComponent<SoilderController>().NodeIndex = 0;
                go.transform.SetParent(CampTrans);
                go.transform.localPosition = Vector3.zero;
            }
        }

        StartCoroutine(WaitForSoilerZero());
    }

    IEnumerator WaitForSoilerZero()
    {
        yield return new WaitUntil(() => SoilderCount == 0);
        SocketClient.Instance.SendAsyn(_RequestType.ISREADY);
    }

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
