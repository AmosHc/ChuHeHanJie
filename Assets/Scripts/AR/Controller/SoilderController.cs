using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ProtoUser;
using UnityEngine.UI;


/// <summary>
/// 士兵控制器，主要包括：
/// 动画控制
/// 位移控制
/// </summary>
public class SoilderController : MonoBehaviour
{
    public float Speed { get; protected set; }
    public float HealthMax { get; protected set; }
    public float HealthNow { get; set; }
    public int Attack { get; protected set; }
    public float AttackDistance { get; protected set; }

    [Tooltip("动画播放速度")]
    public float AnimSpeed = 1f;

    [Tooltip("行走速度")]
    public float WalkSpeed = 0.3f;
    [Tooltip("AR相机")]
    public Camera ARCamera;
    //顶部uiroot
    private GameObject UIRoot = null;
    private Slider hpBar;
    /// <summary>
    /// 兵的标志
    /// </summary>
    public int ID { get; set; }

    public WarData.Types.CampState Camp;
    /// <summary>
    /// 经过的节点集合
    /// </summary>
    protected Transform[] NodesCollection;

    protected List<Vector3> Nodes = new List<Vector3>();

    /// <summary>
    /// 目的地节点
    /// </summary>
    protected Vector3 DestinationNode;

    /// <summary>
    /// 动画组件
    /// </summary>
    private Animator animator;

    public bool IsGameOver = false;

    /// <summary>
    /// 节点索引
    /// </summary>
    private int nodeIndex = 0;

    private WarData.Types.Bullet BulletData = null;

    public int NodeIndex
    {
        get
        {
            return nodeIndex;
        }
        set
        {
            nodeIndex = value;
        }
    }

    public float OffSet = 0;

    /// <summary>
    /// 父对象
    /// </summary>
    protected Transform parentTransform;

    protected virtual void Awake()
    {
    }

    private void Start()
    {
        WalkSpeed = Speed;
        HealthNow = HealthMax;
        GetComponent<SphereCollider>().radius = AttackDistance;
        animator = transform.GetComponent<Animator>();
        animator.speed = AnimSpeed;
        parentTransform = transform.parent;
        InitNodes(OffSet);
        System_Event.m_Events.AddListener(System_Event.GAMEBULLETDATA, OnMessage);
        AddUIRoot();
    }

    /// <summary>
    /// 在角色顶部添加uiroot节点
    /// </summary>
    private void AddUIRoot()
    {
        UIRoot = new GameObject("UI");
        UIRoot.layer = 5;
        UIRoot.transform.SetParent(gameObject.transform, false);
        UIRoot.transform.localPosition = new Vector3(0, 1.5f, 0);
        Canvas uiCanvas = UIRoot.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform rect = UIRoot.GetComponent<RectTransform>();
        rect.sizeDelta = Vector2.one;

        DogfaceUIRoot uiRootMono = UIRoot.AddComponent<DogfaceUIRoot>();
        uiRootMono.m_Camera = ARCamera;
        GameObject hpObj = ObjectManger.Instance.InstantiateObject(ConStr.DOGFACEHPBAR);
        hpObj.transform.SetParent(UIRoot.transform, false);
        hpBar = hpObj.GetComponent<Slider>();
        UpdateHp();
    }

    //设置血量
    private void UpdateHp()
    {
        float pointer = HealthNow / HealthMax;//bar进度
        hpBar.value = pointer;
    }

    private void OnMessage(object[] paramlist)
    {
        Debug.Log("接受到子弹消息1");
        if ((_RequestType)paramlist[0] == _RequestType.BULLETDATA)
            BulletData = (WarData.Types.Bullet)paramlist[1];
        else
            Debug.LogWarning("消息类型错误：" + paramlist[0]);
    }

    public IEnumerator WaitForMessage()
    {
        Debug.Log("接受到子弹消息2");
        yield return new WaitUntil(() => BulletData != null);
        if (IsGameOver)
            yield return null;
        ReceiveMessage(BulletData);
        BulletData = null;
    }

    public void ReceiveMessage(WarData.Types.Bullet bd)
    {
        Debug.Log("接受到子弹消息3" + " 子弹信息：" + bd.SoilderCamp.ToString() + " " +
            bd.SoilderID.ToString() + "小兵信息： " + Camp.ToString() + ID.ToString());
        //如果包中的小兵阵营和ID与当前小兵的阵营和ID相符，就销毁子弹
        if (bd.SoilderCamp == Camp && bd.SoilderID == ID)
        {
            HealthNow--;
            UpdateHp();
            Debug.Log("小兵被集中，收到1点伤害");
        }
    }

    private void Update()
    {
        if (IsGameOver)
        {
            animator.speed = 0;
            WalkSpeed = 0;
            return;
        }
        if (HealthNow <= 0)
        {
            DestroySelf();
            Debug.Log(string.Format("{0},{1}小兵被摧毁", Camp, ID));
        }
        //如果达到终点，销毁自身，并且对玩家造成伤害
        if(ReachDestination())
        {
            // 只有属于当前设备所属阵营的小兵才会发送消息
            if (DataLocal.Instance.MyCamp == Camp)
                SendMessage();
            animator.speed = 0;
            DestroySelf();
        }
        else
        { 
            float remainingDistance = Vector3.Distance(transform.localPosition, Nodes[nodeIndex]);
            if (nodeIndex <= Nodes.Count - 1 && remainingDistance < 0.1f)
            {
                GoNextPoint();
                return;
            }
            transform.Translate(WalkSpeed * Time.deltaTime * Vector3.forward, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<SoilderController>() == null) //碰撞到非小兵单位不做判定
            return;
        if (animator.GetBool("attack")) //已经对一个小兵进行攻击则不攻击第二个小兵
            return;
        if (collision.gameObject.GetComponent<SoilderController>().Camp != Camp)
        {
            if (collision.isTrigger)    //如果碰撞到的只是对方小兵的攻击距离碰撞器不进行攻击
                return;
            animator.SetBool("attack", true);
            WalkSpeed = 0;
            StartCoroutine(Attacking(collision.gameObject.GetComponent<SoilderController>()));
        }
    }
    /// <summary>
    /// 每秒进行攻击判定
    /// </summary>
    /// <param name="sc">被攻击小兵</param>
    /// <returns></returns>
    IEnumerator Attacking(SoilderController sc)
    {
        yield return new WaitForSeconds(0.8f);
        if (sc)
        {
            sc.HealthNow -= Attack;
            UpdateHp();
            StartCoroutine(Attacking(sc));
        }
        else
        {
            animator.SetBool("attack", false);
            WalkSpeed = Speed;
        }
    }

    /// <summary>
    /// 判断是否移动到终点
    /// </summary>
    /// <returns></returns>
    private bool ReachDestination()
    {
        if(Vector3.Distance(transform.localPosition, DestinationNode) <0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 去下一个节点
    /// </summary>
    private void GoNextPoint()
    {
        if (Nodes.Count == 0)
        {
            Debug.LogWarning("Node.Count == 0！！！");
            return;
        }
        nodeIndex++;
        transform.forward = parentTransform.TransformDirection(Nodes[nodeIndex] - transform.localPosition);
    }

    /// <summary>
    /// 初始化节点
    /// </summary>
    public  void InitNodes(float offset)
    {
        NodesCollection = GameObject.Find("NodeCollections").GetComponentsInChildren<Transform>();
        int len = NodesCollection.Length;
        for (int i = 1; i < len; i++)
        {
            if (Camp == WarData.Types.CampState.Blue)
            {
                Nodes.Add(parentTransform.InverseTransformPoint(NodesCollection[len - i].position) + Vector3.forward * offset);
            }
            else
            {
                Nodes.Add(parentTransform.InverseTransformPoint(NodesCollection[i].position) + Vector3.forward * offset);
            }
        }
        DestinationNode = Nodes[Nodes.Count - 1];
    }

    /// <summary>
    /// 对象销毁
    /// </summary>
    private void DestroySelf()
    {
        WarFieldManager.Instance.SoilderCount--;
        StopAllCoroutines();
        Destroy(GetComponent<SoilderController>());
        Destroy(UIRoot);
        ObjectManger.Instance.ReleaseObject(gameObject);
        //Destroy(gameObject);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage()
    {
        WarData.Types.Soilder sd = new WarData.Types.Soilder
        {
            Camp = Camp,
            Attack = Attack,
        };
        SocketClient.Instance.SendAsyn(sd, _RequestType.SOILDERDATA);
    }
}
