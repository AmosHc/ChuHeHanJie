using UnityEngine;
using System.Collections.Generic;

//[CreateAssetMenu(fileName = "AssetSerialize", menuName = "AssetSerialize", order = 0)]
public class AssetSerialize : ScriptableObject
{
    public int id;
    public string name;
    public List<string> list;
}