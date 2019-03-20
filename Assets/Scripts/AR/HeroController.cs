using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 英雄控制
/// </summary>
public class HeroController : MonoBehaviour
{
    /// <summary>
    /// 子弹预设体路径
    /// </summary>
    private string bulletPrefab = "Assets/GameData/Prefabs/AR/Bullet.prefab";

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();
    }
#endif

    public void Shoot()
    {

        Transform ARCamera = GameObject.Find("ARCamera").transform;

        GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);

        go.transform.up = ARCamera.forward;
        go.transform.position = ARCamera.position;
    }

}
