using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Soilder
{
    public GameObject SoilderPrefab;

    public int SoilderCount;
}

/// <summary>
/// 自动生成小兵
/// </summary>
public class SpawnSoilders : MonoBehaviour
{
    [Tooltip("士兵预设体")]
    public Soilder[] SoilderPrefabs;

    /// <summary>
    /// 士兵存储字典
    /// </summary>
    private Dictionary<GameObject,int> SoildersDictionary = null;


    private void Start()
    {
        //foreach(Soilder s in SoilderPrefabs)
        //{
        //    SoildersDictionary.Add(s.SoilderPrefab, s.SoilderCount);
        //}

        //StartSpawnSoilders();
    }

    public void StartSpawnSoilders()
    {
        GameObject go = ObjectManger.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab");
    }
}
