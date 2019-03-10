using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 战斗场景的基本控制
/// </summary>
public class WarFieldController : MonoSingleton<WarFieldController>
{
    [Tooltip("为了突出战斗场景的背景")]
    public GameObject BGRFX = null;

    [Tooltip("战斗场景上升速度")]
    public float RiseSpeed = 1f;

    [Tooltip("战斗场景最大旋转角度")]
    public float MaxRotateAngle = 180f;

	
	// Update is called once per frame
	void Update ()
    {
        if (Mathf.Abs(transform.localPosition.y - 0.1f) < 0.01f)
        {
            if(BGRFX != null)
                Destroy(BGRFX);
            return;
        }  
        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 0.1f, 0), RiseSpeed * Time.deltaTime);
	}

    public void RotateWarField(float factor)
    {
        transform.localEulerAngles  = new Vector3(0, factor * MaxRotateAngle, 0);
    }

    public void ScaleWarField(float factor)
    {
        transform.localScale = new Vector3(5, 0.2f, 3) * (factor + 1);
    }
}
