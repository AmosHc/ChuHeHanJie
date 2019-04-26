using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogfaceUIRoot : MonoBehaviour
{
    //主摄像机对象
    public Camera m_Camera;

    void Update()
    {
        //保持NPC一直面朝主角
        transform.LookAt(m_Camera.transform);
    }

}
