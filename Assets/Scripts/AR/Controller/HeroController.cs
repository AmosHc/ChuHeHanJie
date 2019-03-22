using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 英雄控制
/// </summary>
public class HeroController : MonoBehaviour
{
    public CampOption Camp  = CampOption.Blue;

    public int Health = 10;

    /// <summary>
    /// 子弹预设体路径
    /// </summary>
    private string bulletPrefab = "Assets/GameData/Prefabs/AR/Bullet.prefab";

    private Transform ARCamera;

    private void Start()
    {
        ARCamera = GameObject.Find("ARCamera").transform;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();
    }
#endif

    public void Shoot()
    {
        GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);

        go.transform.up = ARCamera.forward;
        go.transform.position = ARCamera.position;
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void SendMessage()
    {
        PlayerData pd = new PlayerData();
        pd.Camp = Camp;
        pd.Position = ARCamera.position;
        pd.Forward = ARCamera.forward;
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="pd">玩家数据</param>
    public void ReceiveMessage(PlayerData pd)
    {
        GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);
        if (pd.Camp != Camp)
            return;
        go.GetComponent<BulletController>().Camp = pd.Camp;
        go.transform.up = pd.Forward;
        go.transform.position = pd.Position;
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="sd">士兵数据</param>
    public void ReceiveMessage(SoilderData sd)
    {
        if(sd.Camp == Camp)
        {
            return;
        }
        Health = Health - sd.Attack;
    }
}
