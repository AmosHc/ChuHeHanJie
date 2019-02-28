using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ResourcesTest : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        //XmlSerializeTest(XmlCreate(1, "xml序列化"));
        //print(XmlDeserialization());
        //BinarySerializeTest(XmlCreate(2, "二进制序列化"));
        //print(BinaryDeserialization());
        //print(AsssetDeserialization());
        //TestLoadAB();
    }

    //void TestLoadAB()
    //{
    //    string bytesSavePath = "Assets/AssetBundleConfig.bytes";
    //    TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(bytesSavePath);
    //    MemoryStream stream = new MemoryStream(textAsset.bytes);
    //    BinaryFormatter formatter = new BinaryFormatter();
    //    AssetBundleConfig config = (AssetBundleConfig)formatter.Deserialize(stream);
    //    stream.Close();
    //    //加载资源
    //    //校验
    //    string path = "Assets/GameData/Prefabs/Attack.prefab";
    //    foreach (ABBase ab in config.ABList)
    //    {
    //        if (ab.Crc == Crc.GetCRC32(path))
    //        {
    //            //加载关联ab包
    //            for (int i = 0; i < ab.ABDependce.Count; i++)
    //            {
    //                AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + ab.ABDependce[i]);
    //            }
    //            //加载自己ab包
    //            AssetBundle attack = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + ab.ABName);
    //            GameObject obj = GameObject.Instantiate(attack.LoadAsset<GameObject>(ab.AssetName));
    //        }
    //    }
    //}



    //   XmlSerializeTest XmlCreate(int id,string name)
    //   {
    //       XmlSerializeTest xml = new XmlSerializeTest();
    //       xml.id = id;
    //       xml.name = name;
    //       xml.list = new List<string>();
    //       xml.list.Add("h1");
    //       xml.list.Add("h2");
    //       return xml;
    //   }


    //   void XmlSerializeTest(XmlSerializeTest xml) //XML序列化
    //   {
    //       FileStream file = new FileStream(Application.dataPath + "/test.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
    //       StreamWriter sw = new StreamWriter(file, System.Text.Encoding.UTF8);
    //       XmlSerializer serializer = new XmlSerializer(xml.GetType());
    //       serializer.Serialize(sw,xml);
    //       sw.Close();
    //       file.Close();   
    //   }

    //   XmlSerializeTest XmlDeserialization() //XML反序列化
    //   {
    //       FileStream file = new FileStream(Application.dataPath + "/test.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
    //       XmlSerializer serializer = new XmlSerializer(typeof(XmlSerializeTest));
    //       XmlSerializeTest xml = (XmlSerializeTest)serializer.Deserialize(file);
    //       file.Close();
    //       return xml;
    //   }

    //   void BinarySerializeTest(XmlSerializeTest xml) //二进制序列化
    //   {
    //       FileStream file = new FileStream(Application.dataPath + "/binary.bytes", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
    //       BinaryFormatter serializer = new BinaryFormatter();
    //       serializer.Serialize(file, xml);
    //       file.Close();
    //   }

    //   XmlSerializeTest BinaryDeserialization() //二进制反序列化
    //   {
    //       TextAsset text = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/binary.bytes");
    //       MemoryStream stream = new MemoryStream(text.bytes);
    //       BinaryFormatter binary = new BinaryFormatter();
    //       XmlSerializeTest xml = (XmlSerializeTest)binary.Deserialize(stream);
    //       stream.Close();
    //       return xml;
    //   }

    //   string AsssetDeserialization() //Asset反序列化
    //   {
    //       AssetSerialize obj = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetSerialize>("Assets/AssetSerialize.asset");
    //       string liststr = "list:";
    //       for (int i = 0; i < obj.list.Count; i++)
    //       {
    //           liststr += "index=" + i + ":" + obj.list[i] + " ";
    //       }
    //       return "id = " + obj.id + "name = " + obj.name + liststr;
    //   }
}
