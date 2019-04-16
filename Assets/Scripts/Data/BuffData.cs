using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
[System.Serializable]
public class BuffData : ExcelBase
{
    public override void Construction()
    {
        AllBuffList = new List<BuffBase>();
        MosterBuffList = new List<BuffBase>();
        for (int i = 0; i < 15; i++)
        {
            BuffBase buff = new BuffBase();
            buff.AllList = new List<string>();
            buff.Id = i;
            buff.BuffType = (BuffEnum)Random.Range(0,4);
            buff.Name = "Name" + i;
            buff.OutLook = i + ".prefab";
            buff.Time = Random.Range(0.5f,5f);
            buff.AllList.Add("测试数据1");
            buff.AllList.Add("测试数据2");
            buff.AllList.Add("测试数据3");
            buff.AllBuffList = new List<BuffTestClass>();
            int count = Random.Range(0, 5);
            for (int j = 0; j < count; j++)
            {
                BuffTestClass testClass = new BuffTestClass()
                {
                    Id = j,
                    Name = "name" + j,
                };
                buff.AllBuffList.Add(testClass);
            }
            AllBuffList.Add(buff);
        }
        for (int i = 0; i < 2; i++)
        {
            BuffBase buff = new BuffBase();
            buff.AllList = new List<string>();
            buff.Id = i;
            buff.BuffType = (BuffEnum)Random.Range(0, 4);
            buff.Name = "Name" + i;
            buff.OutLook = i + ".prefab";
            buff.Time = Random.Range(0.5f, 5f);
            buff.AllList.Add("测试数据1");
            buff.AllList.Add("测试数据2");
            buff.AllBuffList = new List<BuffTestClass>();
            int count = Random.Range(1, 5);
            for (int j = 0; j < count; j++)
            {
                BuffTestClass testClass = new BuffTestClass()
                {
                    Id = j,
                    Name = "name" + j,
                };
                buff.AllBuffList.Add(testClass);
            }
            MosterBuffList.Add(buff);
        }
    }

    public override void Init()
    {
        AllBuffDic.Clear();
        for (int i = 0; i < AllBuffList.Count; i++)
        {
            if(!AllBuffDic.ContainsKey(AllBuffList[i].Id))
                AllBuffDic.Add(AllBuffList[i].Id,AllBuffList[i]);
        }
    }

    public BuffBase GetBuffById(int id)
    {
        return AllBuffDic[id];
    }

    [XmlIgnore]
    public Dictionary<int, BuffBase> AllBuffDic = new Dictionary<int, BuffBase>();
    [XmlElement("AllBuffList")]
    public List<BuffBase> AllBuffList { get; set; }
    [XmlElement("MosterBuffList")]
    public List<BuffBase> MosterBuffList { get; set; }
}

public enum BuffEnum
{
    None = 0,
    RanShao = 1,
    BingDong = 2,
    Du = 3
}
[System.Serializable]
public class BuffBase
{
    [XmlAttribute("Id")]
    public int Id { get; set; }
    [XmlAttribute("Name")]
    public string Name { get; set; }
    [XmlAttribute("OutLook")]
    public string OutLook { get; set; }
    [XmlAttribute("Time")]
    public float Time { get; set; }
    [XmlAttribute("BuffType")]
    public BuffEnum BuffType { get; set; }
    [XmlElement("AllList")]
    public List<string> AllList { get; set; }
    [XmlElement("AllBuffList")]
    public List<BuffTestClass> AllBuffList { get; set; }
}
[System.Serializable]
public class BuffTestClass
{
    [XmlAttribute("Id")]
    public int Id { get; set; }
    [XmlAttribute("Name")]
    public string Name { get; set; }
}