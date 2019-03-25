using System.Collections.Generic;
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
    public float AnimSpeed = 1;

    [Tooltip("行走速度")]
    public float WalkSpeed = 1;

    public WarData.Types.CampState Camp;
    /// <summary>
    /// 代理经过的节点集合
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

    /// <summary>
    /// 节点索引
    /// </summary>
    private int nodeIndex = 0;

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
    }

    private void Update()
    {
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
        BulletController bc = collision.gameObject.GetComponent<BulletController>();

        //只有当对方的子弹撞击到身上时，才会销毁自身
        if (bc != null && bc.Camp == Camp)
            return;
        DestroySelf();
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
            return;
        }
        nodeIndex++;
        transform.forward = parentTransform.TransformDirection(Nodes[nodeIndex] - transform.localPosition);
    }

    /// <summary>
    /// 初始化节点
    /// </summary>
    protected virtual void InitNodes(float offset)
    {

    }

    /// <summary>
    /// 对象销毁
    /// </summary>
    private void DestroySelf()
    {
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
