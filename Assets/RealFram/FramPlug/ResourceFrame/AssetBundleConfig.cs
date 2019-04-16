using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class AssetBundleConfig {
    [XmlElement("ABList")]
    public List<ABBase> ABList { get; set; }
}

[System.Serializable]
public class ABBase
{
    [XmlAttribute("Path")]
    public string Path { get; set; }//路径
    [XmlAttribute("Crc")]
    public uint Crc { get; set; }//校验码
    [XmlAttribute("ABName")]
    public string ABName { get; set; }//ab包名
    [XmlAttribute("AssetName")]
    public string AssetName { get; set; }//资源名
    [XmlElement("ABDependce")]
    public List<string> ABDependce { get; set; }//依赖关系
}
