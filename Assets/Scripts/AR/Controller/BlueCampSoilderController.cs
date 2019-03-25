using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蓝方士兵控制器
/// </summary>
public class BlueCampSoilderController : SoilderController
{
    protected override void InitNodes(float offset)
    {
        NodesCollection = GameObject.Find("NodeCollections").GetComponentsInChildren<Transform>();
        int len = NodesCollection.Length;
        for (int i = 1; i < len; i++)
        {
            Nodes.Add(parentTransform.InverseTransformPoint(NodesCollection[len - i].position) + Vector3.forward * offset);
        }
        DestinationNode = Nodes[Nodes.Count - 1];
    }
}
