using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;


/// <summary>
/// 子弹控制
/// </summary>
public class BulletController : MonoBehaviour
{
    public float Force = 1;

    private Rigidbody rigidBody;

    private Vector3 forward;

    ////测试用
    //private Transform ImageTarget;

    public WarData.Types.CampState Camp { get; set; }

    private void Start()
    {
        ////测试用
        //ImageTarget = GameObject.Find("ImageTarget").transform;
        Init();
    }

    public void Init()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.AddForce(transform.up * Force);
        //StartCoroutine(DestroyObjectAfterTime(10));
        Destroy(gameObject, 10);
    }

    private IEnumerator DestroyObjectAfterTime(float seconds)
    {
        yield return new WaitForSeconds(10);
        if (transform.parent != ObjectManger.Instance.RecyclePoolTrs)
            DestroySelf();
    }

    private void OnCollisionEnter(Collision collision)
    {
        SoilderController sc = collision.gameObject.GetComponent<SoilderController>();
        //只有当子弹碰到的是对方小兵，才会销毁自身。
        if (sc != null && sc.Camp == Camp)
            return;
        DestroySelf();
        print("bullet collision" + collision.gameObject.name);
    }

    private void SendMessage()
    {
        BulletData bd = new BulletData();
    }

    ///// <summary>
    ///// 测试用
    ///// </summary>
    //private void OnGUI()
    //{
    //    GUIStyle fontStyle = new GUIStyle();
    //    fontStyle.normal.background = null;    //设置背景填充
    //    fontStyle.normal.textColor = new Color(1, 0, 0);   //设置字体颜色
    //    fontStyle.fontSize = 60;       //字体大小

    //    GUI.Label(new Rect(100, 300, 200, 200), "位置："+ ImageTarget.InverseTransformPoint(transform.position).ToString(), fontStyle);
    //    GUI.Label(new Rect(100, 500, 200, 200), "位置：" + ImageTarget.InverseTransformDirection(transform.forward).ToString(), fontStyle);

    //}

    private void DestroySelf()
    {
        //ObjectManger.Instance.ReleaseObject(gameObject);
        Destroy(gameObject);
    }
}
