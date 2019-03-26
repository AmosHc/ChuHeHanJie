using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoUser;

/// <summary>
/// 英雄控制
/// </summary>
public class HeroController : MonoBehaviour
{
    public WarData.Types.CampState Camp  = WarData.Types.CampState.Blue;

    private WarData.Types.Player Data_Player = null;
    private WarData.Types.Soilder Data_Soilder = null;

    public int Health = 10;

    /// <summary>
    /// 子弹预设体路径
    /// </summary>
    private string bulletPrefab = "Assets/GameData/Prefabs/AR/Bullet.prefab";

    private Transform ARCamera;

    private void Start()
    {
        ARCamera = GameObject.Find("ARCamera").transform;
        StartCoroutine(WaitForMessage());
        System_Event.m_Events.AddListener(System_Event.GAMEPLAYERDATA, OnMessage);
    }

    private void OnMessage(object []paramlist)
    {
        if ((_RequestType)paramlist[0] == _RequestType.PLAYERDATA)
            Data_Player = (WarData.Types.Player)paramlist[1];
        else if ((_RequestType)paramlist[0] == _RequestType.SOILDERDATA)
            Data_Soilder = (WarData.Types.Soilder)paramlist[1];
        else
            Debug.LogWarning("消息类型错误：" + paramlist[0]);
    }

    IEnumerator WaitForMessage()
    {
        yield return new WaitUntil(() => Data_Player != null || Data_Soilder != null);
        if (Data_Player != null && Data_Soilder != null)
            Debug.LogWarning("既是Player又是Soilder");
        else if (Data_Player != null)
            ReceiveMessage(Data_Player);
        else if (Data_Soilder != null)
            ReceiveMessage(Data_Soilder);
        Data_Player = null;
        Data_Soilder = null;
        StartCoroutine(WaitForMessage());
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
        WarData.Types.Player pd = new WarData.Types.Player
        {
            Camp = Camp,
            Self = new WarData.Types.Vector3
            {
                X = ARCamera.position.x,
                Y = ARCamera.position.y,
                Z = ARCamera.position.z
            },
            Forward = new WarData.Types.Vector3()
            {
                X = ARCamera.forward.x,
                Y = ARCamera.forward.y,
                Z = ARCamera.forward.z
            }
        };
        SocketClient.Instance.SendAsyn(pd, _RequestType.PLAYERDATA);
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    public void ReceiveMessage(WarData.Types.Player pd)
    {
        GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);
        if (pd.Camp != Camp)
            return;
        go.GetComponent<BulletController>().Camp = pd.Camp;
        go.transform.up = new Vector3(pd.Forward.X, pd.Forward.Y, pd.Forward.Z);
        go.transform.position = new Vector3(pd.Self.X, pd.Self.Y, pd.Self.Z);
    }

    public void ReceiveMessage(WarData.Types.Soilder sd)
    {
        if (sd.Camp == Camp)
            return;
        Health = Health - sd.Attack;
    }
}
