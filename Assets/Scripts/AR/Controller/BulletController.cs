using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 子弹控制
/// </summary>
public class BulletController : MonoBehaviour
{
    public float Force = 1;

    private Rigidbody rigidBody;

    private Vector3 forward;

    public CampOption Camp { get; set; }

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.AddForce(transform.up * Force);
        Destroy(gameObject, 10);
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

    private void DestroySelf()
    {
        Destroy(gameObject); 
    }
}
