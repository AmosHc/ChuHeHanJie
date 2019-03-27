using UnityEngine;
/// <summary>
/// 阵营
/// </summary>
public enum CampOption
{
    Red,
    Blue
}

/// <summary>
/// 士兵
/// Camp表示士兵所属阵营
/// Attack表示士兵伤害
/// </summary>
public class SoilderData
{
    public CampOption Camp;
    public int Attack;
}

/// <summary>
/// 玩家数据
/// Camp表示玩家所属阵营
/// Position表示玩家位置
/// Forward表示玩家面向的方向
/// </summary>
public class PlayerData
{
    public CampOption Camp;
    public Vector3 Position;
    public Vector3 Forward;
}

/// <summary>
/// 子弹数据
/// </summary>
public class BulletData
{
    public CampOption SoilderCamp;
    public CampOption BulletCamp;
    public int SoilderID;
    public int BulletID;
}

/// <summary>
/// 客户端向服务器发送游戏准备就绪
/// </summary>
public class GameReady
{
    public bool IsReady;
}


/// <summary>
/// 服务器向客户端传回开始发兵指令
/// </summary>
public class SpawnSoilders
{
    public bool StartSpawn;
}
