using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class MonsterData : ExcelBase
{
    [XmlIgnore]
    private Dictionary<int, MonsterBase> m_AllmosterBase = new Dictionary<int, MonsterBase>();
    public override void Construction()
    {
        AllMonster = new List<MonsterBase>();
        base.Construction();
        for (int i = 0; i < 5; i++)
        {
            MonsterBase monsterBase = new MonsterBase();
            monsterBase.Id = i;
            monsterBase.Name = "monster" + i;
            monsterBase.PrefabPath = ConStr.ALERTPRE;
            AllMonster.Add(monsterBase);
        }
    }

    public override void Init()
    {
        base.Init();
        for (int i = 0; i < AllMonster.Count; i++)
        {
            if (m_AllmosterBase.ContainsKey(AllMonster[i].Id))
                Debug.LogError("有重复id");
            else
                m_AllmosterBase.Add(AllMonster[i].Id,AllMonster[i]);
        }
    }
    /// <summary>
    /// 根据id获得怪物数据
    /// </summary>
    /// <param name="monsterId"></param>
    /// <returns></returns>
    public MonsterBase FindMonsterById(int monsterId)
    {
        return AllMonster[monsterId];
    }
    [XmlElement("AllMonster")]
    public List<MonsterBase> AllMonster { get; set; }
}

[System.Serializable]
public class MonsterBase
{
    [XmlAttribute("Id")]
    public int Id { get; set; }
    [XmlAttribute("Name")]
    public  string Name { get; set; }
    //预制体路径
    [XmlAttribute("PrefabPath")]
    public string PrefabPath { get; set; }
    //攻击范围
    [XmlAttribute("AttackRange")]
    public float AttackRange { get; set; }
    //攻击值
    [XmlAttribute("AttackValue")]
    public int AttackValue { get; set; }
}
