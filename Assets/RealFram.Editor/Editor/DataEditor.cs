
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Object = UnityEngine.Object;

public class DataEditor
{
    private static string XMLPATH = "Assets/GameData/Data/Xml/";
    private static string BINARYPATH = "Assets/GameData/Data/Binary/";
    private static string SCRIPTPATH = "Assets/Scripts/Data/";
    [MenuItem("Assets/类转Xml")]
    public static void AssetsClassToXml()
    {
        Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的类转成xml", "正在扫描：" + objects[i].name, 1.0f / objects.Length * i);
            ClassToXml(objects[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Xml转二进制")]
    public static void AssetsXmlToBinary()
    {
        Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转成xml", "正在扫描：" + objects[i].name, 1.0f / objects.Length * i);
            XmlToBinary(objects[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 将文件夹下的xml批量转换为二进制
    /// </summary>
    [MenuItem("Tools/Xml/Xml转二进制")]
    public static void AssetsAllXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + XMLPATH;
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles("*.xml");
        for (int i = 0; i < files.Length; i++ )
        {
            EditorUtility.DisplayProgressBar("文件夹下的xml批量转成二进制", "正在扫描：" + files[i].Name, 1.0f / files.Length * i);
            XmlToBinary(files[i].Name.Replace(".xml", ""));
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 将xml转换为二进制
    /// </summary>
    /// <param name="className"></param>
    public static void XmlToBinary(string className)
    {
        if (string.IsNullOrEmpty(className)) return;
        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(className);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type != null)
            {
                string xmlPath = XMLPATH + className + ".xml";
                string binaryPath = BINARYPATH + className + ".bytes";
                object obj = BinarySerializeOpt.XmlDeserialization(xmlPath,type);
                BinarySerializeOpt.BinarySerialize(binaryPath, obj);
                Debug.Log(className + "xml转二进制成功！");
            }

        }
        catch (Exception e)
        {
            Debug.LogError(className + "xml转二进制失败！" + e);
        }
    }

    /// <summary>
    /// 将运行中的实际类转成Xml
    /// </summary>
    /// <param name="className"></param>
    public static void ClassToXml(string className)
    {
        if (string.IsNullOrEmpty(className)) return;
        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(className);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type != null)
            {
                var obj = Activator.CreateInstance(type);
                if (obj is ExcelBase)
                {
                    (obj as ExcelBase).Construction();
                }

                BinarySerializeOpt.XmlSerialize(XMLPATH + className + ".xml", obj);
                Debug.Log(className + "类转成Xml成功！");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(className + "类转xml失败！"+e);
        }
        
    }
    [MenuItem("Tools/测试读取xml")]
    public static void TestReadXml()
    {
        string xmlPath = Application.dataPath + "/../Data/Reg/MonsterData.xml";
        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlReader xmlReader = XmlReader.Create(xmlPath);
            xmlDocument.Load(xmlReader);
            XmlElement dataElement = (XmlElement)xmlDocument.SelectSingleNode("data");
            string className = dataElement.GetAttribute("name");
            string excelName = dataElement.GetAttribute("from");
            string xmlName = dataElement.GetAttribute("to");
            foreach (XmlNode variableNode in dataElement.ChildNodes)
            {
                XmlElement variableElement = variableNode as XmlElement;
                string attrName = variableElement.GetAttribute("name");
                string attrType = variableElement.GetAttribute("type");
                Debug.Log(attrName + "-" + attrType);
                XmlElement listElement = variableElement.FirstChild as XmlElement;
                foreach (XmlNode itemNode in listElement.ChildNodes)
                {
                    XmlElement itemElement = itemNode as XmlElement;
                    string itemAttrName = itemElement.GetAttribute("name");
                    string itemAttrType = itemElement.GetAttribute("type");
                    string itemColName = itemElement.GetAttribute("col");
                    Debug.Log(itemAttrName + "-" + itemAttrType + "-" + itemColName);
                }
            }
            Debug.Log(className + "-" + excelName + "-" + xmlName);
        }
        catch (Exception e)
        {
            Debug.Log("xml读取异常：" + e);
        }
    }
    /// <summary>
    /// 测试输出excel
    /// </summary>
    [MenuItem("Tools/测试输出excel")]
    public static void TestExportExcel()
    {
        string excelPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
        FileInfo file = new FileInfo(excelPath);
        if (file.Exists)
        {
            file.Delete();
            file = new FileInfo(excelPath);
        }

        using (ExcelPackage excel = new ExcelPackage(file))
        {
            ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("怪物配置表");
            sheet.SetValue(1, 1, "测试");
            //sheet.Cells[1, 1].Value = "测试2";
            excel.Save();
        }
    }

    [MenuItem("Tools/测试已有类对象进行反射")]
    public static void TestReflectByObj()
    {
        TestClass test = new TestClass()
            {Age = 10,Id = 1,Name = "hhhh",AllStrList = new List<string>(){"11","22","33"},AllClassList = new List<TestClass2>()};
        for (int i = 0; i < 3; i++)
        {
            TestClass2 obj = new TestClass2();
            obj.Id = i;
            obj.Name = "Name" + i;
            test.AllClassList.Add(obj);
        }

        //反射列表
        //object list = GetMemberValue(test, "AllStrList");
        //int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count",BindingFlags.Default|BindingFlags.InvokeMethod,null, list, new object[] { }));
        //for (int i = 0; i < listCount; i++)
        //{
        //    string item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i }).ToString();
        //    Debug.Log(item);
        //}

        //反射获得列表里所有数据
        object list = GetMemberValue(test, "AllClassList");
        int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });
            MemberInfo[] members = item.GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
            foreach (var m in members)
            {
                Debug.Log("i=" + i + " name=" + m.Name + " value=" + GetMemberValue(item, m.Name));
            }
        }
    }

    [MenuItem("Tools/测试已有数据进行反射")]
    public static void TestReflectByName()
    {
        object obj = CreatClass("TestClass");

        //PropertyInfo agetProperty = obj.GetType().GetProperty("Age");
        //agetProperty.SetValue(obj,System.Convert.ToInt32("20"));
        //PropertyInfo heightProperty = obj.GetType().GetProperty("Height");
        //heightProperty.SetValue(obj, System.Convert.ToSingle("222.2222"));
        //PropertyInfo testEnumProperty = obj.GetType().GetProperty("TestEnum");
        //object enumObj = TypeDescriptor.GetConverter(testEnumProperty.PropertyType).ConvertFromInvariantString("Type1");
        //testEnumProperty.SetValue(obj,enumObj);
        SetValue(obj,"Age","20","int");
        SetValue(obj, "Height", "222.2222", "float");
        SetValue(obj, "TestEnum", "Type1", "enum");
        object list = CreateList<string>();
        for (int i = 0; i < 5; i++)
        {
            object item = "item" + i;
            list.GetType().InvokeMember("Add", BindingFlags.InvokeMethod | BindingFlags.Default,null, list, new object[]{item});
        }
        obj.GetType().GetProperty("AllStrList").SetValue(obj, list);

        object classList = CreateList<TestClass2>();
        for (int i = 0; i < 5; i++)
        {
            object item = CreatClass("TestClass2");
            SetValue(item, "Id",""+i,"int");
            SetValue(item, "Name", "name" + i, "string");
            classList.GetType().InvokeMember("Add", BindingFlags.InvokeMethod | BindingFlags.Default, null, classList,
                new object[] {item});
        }
        obj.GetType().GetProperty("AllClassList").SetValue(obj, classList);
        TestClass test = obj as TestClass;
        foreach (var str in test.AllStrList)
        {
            Debug.Log(str);
        }

        Debug.Log(test.Age + "-" + test.Height + "-" + test.TestEnum);
    }

    public static object CreateList<T>() where T : class
    {
        Type tType = typeof(T);
        Type listType = typeof(List<>);
        Type specType = listType.MakeGenericType(new Type[] { tType });//确定泛型类的type
        return Activator.CreateInstance(specType);
    }

    /// <summary>
    /// 设置对象内部属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public static void SetValue(object obj, string propertyName, string value, string type)
    {
        PropertyInfo property = obj.GetType().GetProperty(propertyName);
        object val = (object) value;
        switch (type)
        {
            case "int":
                val = System.Convert.ToInt32(value);
                break;
            case "float":
                val = System.Convert.ToSingle(value);
                break;
            case "bool":
                val = System.Convert.ToBoolean(value);
                break;
            case "enum":
                val = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString(value);
                break;
        }
        property.SetValue(obj,val);
    }

    /// <summary>
    /// 根据className反射获取类对象
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    private static object CreatClass(string className)
    {
        object obj = null;
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogError("className为空！");
            return null;
        }

        Type type = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type tempType = asm.GetType(className);
            if (tempType != null)
            {
                type = tempType;
                break;
            }
        }

        if (type != null)
        {
            obj = Activator.CreateInstance(type);
        }
        return obj;
    }

    /// <summary>
    /// 获得指定对象中的指定字段
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="memberName"></param>
    /// <param name="bindingFlags"></param>
    /// <returns></returns>
    public static object GetMemberValue(object obj, string memberName,
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
    {
        Type type = obj.GetType();
        MemberInfo[] members = type.GetMember(memberName, bindingFlags);
        if (members == null || members.Length <= 0)
        {
            Debug.LogError("memberName不存在:"+ memberName);
            return null;
        }
        switch (members[0].MemberType)
        {
            case MemberTypes.Field:
                return type.GetField(memberName, bindingFlags)
                    .GetValue(obj);

            case MemberTypes.Property:
                return type.GetProperty(memberName, bindingFlags)
                    .GetValue(obj);
            default:
                Debug.LogError("MemberType未定义:" + members[0].MemberType);
                return null;
        }
    }
}

public enum TestEnum
{
    Type1,
    Type2,
    Type3,
    Type4,
}

public class TestClass
{
    public int Id;
    public string Name;
    public int Age { get; set; }
    public List<string> AllStrList { get; set; }
    public List<TestClass2> AllClassList { get; set; }
    public float Height { get; set; }
    public TestEnum TestEnum { get; set; }
}

public class TestClass2
{
    public int Id;
    public string Name;
}