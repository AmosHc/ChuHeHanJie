using System.Collections.Generic;
using UnityEngine;


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

    /// <summary>
    /// 父对象
    /// </summary>
    protected Transform parentTransform;


    private void Start()
    {
        animator = transform.GetComponent<Animator>();
        animator.speed = AnimSpeed;

        parentTransform = transform.parent;
        InitNodes();
    }

    private void Update()
    {
        if(ReachDestination())
        {
            animator.speed = 0;
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
        Destroy(gameObject);
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
    protected virtual void InitNodes()
    {

    }
    
}
