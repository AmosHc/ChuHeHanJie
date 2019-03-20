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

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.AddForce(transform.up * Force);
    }

    private void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        print("bullet collision" + collision.gameObject.name);
    }

}
