using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
[System.Serializable]
public class XmlSerializeTest{
    [XmlAttribute("id")]
    public int id;
    [XmlAttribute("name")]
    public string name;
    [XmlElement("list")]
    public List<string> list;

    public override string ToString()
    {
        string liststr = "list:";
        for (int i = 0; i < list.Count; i++) {
            liststr += "index=" + i + ":" + list[i] + " ";
        }
        return "id = " + id + "name = " + name + liststr;
    }
}
