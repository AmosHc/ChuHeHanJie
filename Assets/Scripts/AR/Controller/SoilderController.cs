using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ProtoUser;


/// <summary>
/// 士兵控制器，主要包括：
/// 动画控制
/// 位移控制
/// </summary>
public class SoilderController : MonoBehaviour
{
    [Tooltip("动画播放速度")]
    public float AnimSpeed = 0.2f;

    [Tooltip("行走速度")]
    public float WalkSpeed = 0.3f;

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


    private void Start()
    {
        animator = transform.GetComponent<Animator>();
        animator.speed = AnimSpeed;
        parentTransform = transform.parent;
        InitNodes(OffSet);
        System_Event.m_Events.AddListener(System_Event.GAMEBULLETDATA, OnMessage);
    }

    private void OnMessage(object[] paramlist)
    {
        if ((_RequestType)paramlist[0] == _RequestType.BULLETDATA)
            BulletData = (WarData.Types.Bullet)paramlist[1];
        else
            Debug.LogWarning("消息类型错误：" + paramlist[0]);
    }

    public IEnumerator WaitForMessage()
    {
        yield return new WaitUntil(() => BulletData != null);
        if (IsGameOver)
            yield return null;
        ReceiveMessage(BulletData);
        BulletData = null;
    }

    public void ReceiveMessage(WarData.Types.Bullet bd)
    {
        //如果包中的小兵阵营和ID与当前小兵的阵营和ID相符，就销毁子弹
        if (bd.SoilderCamp == Camp && bd.SoilderID == ID)
        {
            DestroySelf();
            Debug.Log(string.Format("{0},{1}小兵被摧毁", Camp, ID));
        }
    }

    private void Update()
    {
        if (IsGameOver)
            return;
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

    private void OnCollisionEnter(Collision collision)
    {
        //单机用
        //BulletController bc = collision.gameObject.GetComponent<BulletController>();

        ////只有当对方的子弹撞击到身上时，才会销毁自身
        //if (bc != null && bc.Camp == Camp)
        //    return;
        //DestroySelf();
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
        #region 李锐
        WarFieldManager.Instance.SoilderCount--;
        #endregion
        Destroy(GetComponent<SoilderController>());
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
            Attack = 1
        };
        SocketClient.Instance.SendAsyn(sd, _RequestType.SOILDERDATA);
    }
}
