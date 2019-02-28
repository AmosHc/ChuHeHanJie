using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ABConfig",menuName = "CreateABConfig",order = 0)]
public class ABConfig : ScriptableObject {

    //单个文件所在的文件夹路径，会遍历这个文件夹下面的所有prefab，所有的prefab的名字不能重复，必须保证唯一性
    public List<string> m_AllPrefabPaths = new List<string>();
    //文件夹路径
    public List<FileDirName> m_FileDirABs = new List<FileDirName>();

    [System.Serializable]
    public struct FileDirName
    {
        public string ABName;
        public string Path;
    }

}
