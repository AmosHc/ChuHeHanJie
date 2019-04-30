using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;
using UnityEngine.UI;

/// <summary>
/// 英雄控制
/// </summary>
public class HeroController : MonoBehaviour
{
    public WarData.Types.CampState Camp  = WarData.Types.CampState.Blue;

    private WarData.Types.Player Data_Player = null;
    private WarData.Types.Soilder Data_Soilder = null;

    [Tooltip("玩家血量")]
    public int Health = 50;
    public int m_Total = -1;
    //[Tooltip("玩家血量文字框")]
    //public Text HealthText;

    [Tooltip("多台设备参照物")]
    public Transform ImageTarget;

    //ui窗口
    private HUDWindow m_hudWnd;
    /// <summary>
    /// 该玩家当前的子弹ID
    /// </summary>
    private int currentBulletID = 0;
    /// <summary>
    /// 子弹预设体路径
    /// </summary>
    private string bulletPrefab = "Assets/GameData/Prefabs/AR/Bullet.prefab";

    private Transform ARCamera;


    private void Start()
    {
        m_Total = Health;
        ARCamera = GameObject.Find("ARCamera").transform;
        System_Event.m_Events.AddListener(System_Event.GAMEPLAYERDATA, OnMessage);
        System_Event.m_Events.AddListener(System_Event.GAMESOILDERDATA, OnMessage);
        StartCoroutine(WaitForMessage());
    }

    private void OnMessage(object []paramlist)
    {
        if ((_RequestType)paramlist[0] == _RequestType.PLAYERDATA)
            Data_Player = (WarData.Types.Player)paramlist[1];
        else if ((_RequestType)paramlist[0] == _RequestType.SOILDERDATA)
            Data_Soilder = (WarData.Types.Soilder)paramlist[1];
        else
            Debug.LogWarning("消息类型错误：" + paramlist[0]);
    }

    IEnumerator WaitForMessage()
    {
        yield return new WaitUntil(() => Data_Player != null || Data_Soilder != null);
        if (Data_Player != null && Data_Soilder != null)
            Debug.LogWarning("既是Player又是Soilder");
        else if (Data_Player != null)
            ReceiveMessage(Data_Player);
        else if (Data_Soilder != null)
            ReceiveMessage(Data_Soilder);
        Data_Player = null;
        Data_Soilder = null;
        StartCoroutine(WaitForMessage());
    }


    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();
#endif
        if (m_hudWnd == null)
        {
            m_hudWnd = UIManager.Instance.FindWndByName<HUDWindow>(ConStr.HUDPANEL);
            if (m_hudWnd == null) return;
            m_hudWnd.SetHp(ConStr.campBlue, m_Total, m_Total); //初始化双端血量
            m_hudWnd.SetHp(ConStr.campRed, m_Total, m_Total);//初始化双端血量
        }
        if (m_hudWnd == null) return;
        if (Camp == WarData.Types.CampState.Blue)
            //HealthText.text = "蓝方剩余血量: " + Health.ToString();
            m_hudWnd.SetHp(ConStr.campBlue, Health, m_Total);
        else
            //HealthText.text = "红方剩余血量: " + Health.ToString();
            m_hudWnd.SetHp(ConStr.campRed, Health, m_Total);

    }

#if UNITY_EDITOR
    public void Shoot()
    {
        GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);

        go.transform.up = ARCamera.forward;
        go.transform.position = ARCamera.position;
    }
#endif

    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage()
    {
        Vector3 localPosition = ImageTarget.InverseTransformPoint(ARCamera.position);
        Vector3 localForward = ImageTarget.InverseTransformDirection(ARCamera.forward);
        WarData.Types.Player pd = new WarData.Types.Player
        {
            Camp = Camp,
            Self = new WarData.Types.Vector3
            {
                X = localPosition.x,
                Y = localPosition.y,
                Z = localPosition.z
            },
            Forward = new WarData.Types.Vector3()
            {
                X = localForward.x,
                Y = localForward.y,
                Z = localForward.z
            }
        };
        SocketClient.Instance.SendAsyn(pd, _RequestType.PLAYERDATA);
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    public void ReceiveMessage(WarData.Types.Player pd)
    {
        // 拆包发现包中的阵营和该玩家所属阵营不符合，就直接返回
        // 不实例化子弹
        if (pd.Camp != Camp)
            return;
        GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);
        //go.transform.SetParent(null);
        //go.GetComponent<BulletController>().Init();
        go.GetComponent<BulletController>().Camp = pd.Camp;
        go.GetComponent<BulletController>().ID = currentBulletID % int.MaxValue;
        currentBulletID += 1;
        Vector3 worldPosition = ImageTarget.TransformPoint(new Vector3(pd.Self.X, pd.Self.Y, pd.Self.Z));
        Vector3 worldForward =  ImageTarget.TransformDirection(new Vector3(pd.Forward.X, pd.Forward.Y, pd.Forward.Z)); 

        go.transform.up = worldForward;
        go.transform.position = worldPosition;
    }

    public void ReceiveMessage(WarData.Types.Soilder sd)
    {
        // 拆包发现包中的阵营和该玩家所属阵营符合，就直接返回
        // 不计算伤害
        if (sd.Camp == Camp)
            return;
        Health = Health - sd.Attack;
        if (Health <= 0) Health = 0;
    }
}
