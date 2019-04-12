using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;


/// <summary>
/// 子弹控制
/// </summary>
public class BulletController : MonoBehaviour
{

    [Tooltip("子弹受力")]
    public float Force = 1;

    /// <summary>
    /// 子弹的标志
    /// </summary>
    public int ID { get; set; }

    private Rigidbody rigidBody;

    private Vector3 forward;

    private WarData.Types.Bullet BulletData = null;

    ////测试用
    //private Transform ImageTarget;

    public WarData.Types.CampState Camp { get; set; }

    private void Start()
    {
        ////测试用
        //ImageTarget = GameObject.Find("ImageTarget").transform;
        Init();
        System_Event.m_Events.AddListener(System_Event.GAMEBULLETDATA, OnMessage);
        StartCoroutine(WaitForMessage());
    }

    private void OnMessage(object[] paramlist)
    {
        if ((_RequestType)paramlist[0] == _RequestType.BULLETDATA)
            BulletData = (WarData.Types.Bullet)paramlist[1];
        else
            Debug.LogWarning("消息类型错误：" + paramlist[0]);
    }

    IEnumerator WaitForMessage()
    {
        yield return new WaitUntil(() => BulletData != null);
        if (BulletData != null)
            ReceiveMessage(BulletData);
        BulletData = null;
        StartCoroutine(WaitForMessage());
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
        if (collision.collider.isTrigger)
            return;
        SoilderController sc = collision.gameObject.GetComponent<SoilderController>();

        if (sc == null || sc.Camp == Camp)
        {
            DestroySelf();
            print("子弹击中环境或者己方小兵：" + collision.gameObject.name);
        }
        else
        {
            if (Camp != DataLocal.Instance.MyCamp)
                return;

            if (sc.Camp != Camp)
            {
                //只有当子弹碰到的是对方小兵，才会销毁自身或者发送消息。
                SendMessage(sc);
                print(string.Format("发送的消息是：\n子弹阵营：{0}，子弹ID，{1}，小兵阵营：{2}，小兵ID：{3}", Camp, ID, sc.Camp, sc.ID));
            }

        }  
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage(SoilderController sc)
    {
        WarData.Types.Bullet bd = new WarData.Types.Bullet
        {
            BulletCamp = Camp,
            BulletID = ID,
            SoilderCamp = sc.Camp,
            SoilderID = sc.ID,
        };
        SocketClient.Instance.SendAsyn(bd, _RequestType.BULLETDATA);
    }


    public void ReceiveMessage(WarData.Types.Bullet bd)
    {
        //如果包中的子弹阵营和ID与当前子弹的阵营和ID相符，就销毁子弹
        if (bd.BulletCamp == Camp && bd.BulletID == ID)
            DestroySelf();
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
        Debug.Log(string.Format("{0},{1}子弹被摧毁",Camp,ID));
    }
}
