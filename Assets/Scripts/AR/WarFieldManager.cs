using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    /// <summary>
    /// 士兵存储
    /// </summary>
    private Dictionary<string, int> SoildersDictionary = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;
        yLocalAngle = transform.localEulerAngles.y;
    }

    private void Start()
    {
        SoildersDictionary.Add("Assets/GameData/Prefabs/AR/RedThief.prefab", 1);
        
    }

    void Update ()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
            StartSpawnSoilders();
#endif

#if UNITY_ANDROID
        foreach(Touch touch in Input.touches)
        {
            if(touch.phase == TouchPhase.Began)
            {
                StartSpawnSoilders();
            }
        }
#endif
        #region 当战场上升到一定高度，就停止上升，销毁背景
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
        foreach (KeyValuePair<string, int> pair in SoildersDictionary)
        {
            int count = pair.Value;
            for (int i = 0; i < count; i++)
            {
                GameObject go = ObjectManger.Instance.InstantiateObject(pair.Key);
                go.transform.SetParent(RedCamp);
                go.transform.localPosition = Vector3.zero;
            }
        }
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
