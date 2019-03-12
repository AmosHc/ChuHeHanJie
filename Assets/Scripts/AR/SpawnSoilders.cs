using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 自动生成小兵
/// </summary>
public class SpawnSoilders : MonoBehaviour
{
    /// <summary>
    /// 士兵存储
    /// </summary>
    private Dictionary<string,int> SoildersDictionary = new Dictionary<string, int>();


    private void Start()
    {
        SoildersDictionary.Add("Assets/GameData/Prefabs/Attack.prefab", 1);
        //ObjectManger.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab");
        StartSpawnSoilders();
    }

    public void StartSpawnSoilders()
    {
        foreach(KeyValuePair<string, int> pair in SoildersDictionary)
        {
            int count = pair.Value;
            for(int i=0;i<count;i++)
            {
                GameObject go = ObjectManger.Instance.InstantiateObject(pair.Key);
            }
        }
    }
}
