using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 红方士兵控制器
/// </summary>
public class RedCampSoilderController : SoilderController
{
    protected override void InitNodes()
    {
        NodesCollection = GameObject.Find("NodeCollections").GetComponentsInChildren<Transform>();
        for (int i = 1; i<NodesCollection.Length; i++)
        {
            Nodes.Add(parentTransform.InverseTransformPoint(NodesCollection[i].position));
        }
        DestinationNode = Nodes[Nodes.Count - 1];
    }
}
