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

    public int Health = 10;

    /// <summary>
    /// 子弹预设体路径
    /// </summary>
    private string bulletPrefab = "Assets/GameData/Prefabs/AR/Bullet.prefab";

    private Transform ARCamera;

    private void Start()
    {
        ARCamera = GameObject.Find("ARCamera").transform;
        System_Event.m_Events.AddListener(System_Event.GAMEPLAYERDATA, ReceiveMessage);
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
        WarData.Types.Player pd = new WarData.Types.Player();
        pd.Camp = Camp;
        pd.Self.X = ARCamera.position.x;
        pd.Self.Y = ARCamera.position.y;
        pd.Self.Z = ARCamera.position.z;
        pd.Forward.X = ARCamera.forward.x;
        pd.Forward.Y = ARCamera.forward.y;
        pd.Forward.Z = ARCamera.forward.z;
        SocketClient.Instance.SendAsyn(pd, _RequestType.PLAYERDATA);
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    public void ReceiveMessage(object[] paramlist)
    {
        if ((_RequestType)paramlist[0] == _RequestType.PLAYERDATA)
        {
            GameObject go = ObjectManger.Instance.InstantiateObject(bulletPrefab);
            WarData.Types.Player pd = (WarData.Types.Player)paramlist[1];
            if (pd.Camp != Camp)
                return;
            go.GetComponent<BulletController>().Camp = pd.Camp;
            go.transform.up = new Vector3(pd.Forward.X, pd.Forward.Y, pd.Forward.Z);
            go.transform.position = new Vector3(pd.Self.X, pd.Self.Y, pd.Self.Z);
        }

        else if ((_RequestType)paramlist[0] == _RequestType.SOILDERDATA)
        {
            WarData.Types.Soilder sd = (WarData.Types.Soilder)paramlist[1];
            if (sd.Camp == Camp)
                return;
            else
                Health -= sd.Attack;
        }

        else
            Debug.LogWarning("消息类型错误：" + (_RequestType)paramlist[0]);
    }

    //public void ReceiveMessage(SoilderData sd)
    //{
    //    if(sd.Camp == Camp)
    //    {
    //        return;
    //    }
    //    Health = Health - sd.Attack;
    //}
}
