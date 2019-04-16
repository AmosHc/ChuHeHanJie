
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using JetBrains.Annotations;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Object = UnityEngine.Object;

public class DataEditor
{
    private static string XMLPATH = ReadConfig.GetRealFream().m_XmlPath;
    private static string BINARYPATH = ReadConfig.GetRealFream().m_BinaryPath;
    private static string SCRIPTPATH = ReadConfig.GetRealFream().m_ScriptsPath;
    private static string EXCELPATH = Application.dataPath+"/../Data/Excel/";
    private static string REGPATH = Application.dataPath + "/../Data/Reg/";
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
    [MenuItem("Tools/Xml/Excel转xml")]
    public static void AssetsAllExcelToXml()
    {
        string[] filePaths = Directory.GetFiles(REGPATH, "*", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; i++)
        {
            if(!filePaths[i].EndsWith(".xml"))
                continue;
            EditorUtility.DisplayCancelableProgressBar("查找文件夹下的类", "正在扫描路径" + filePaths[i] + "...",
                1.0f / filePaths.Length * i);
            string path = filePaths[i].Substring(filePaths[i].LastIndexOf("/") + 1);
            ExcelToXml(path.Replace(".xml",""));
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
    public static void ExcelToXml(string xmlRegName)
    {
        string className = "";
        string excelName = "";
        string xmlName = "";
        //读取reg文件数据
        Dictionary<string, SheetClass> allSheetClassDic = ReadReg(xmlRegName, ref className, ref excelName, ref xmlName);
        //读取excel文件数据
        string excelPath = EXCELPATH+excelName;
        Dictionary<string,SheetData> allSheetDataDic = new Dictionary<string, SheetData>();
        try
        {
            using (FileStream stream = new FileStream(excelPath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
            {
                using (ExcelPackage excelPackage = new ExcelPackage(stream))
                {
                    ExcelWorksheets worksheetArray = excelPackage.Workbook.Worksheets;
                    for (int i = 0; i < worksheetArray.Count; i++)
                    {
                        SheetData sheetData = new SheetData();
                        ExcelWorksheet worksheet = worksheetArray[i + 1];//索引从1开始
                        string key = GetDicKey(allSheetClassDic,worksheet.Name);
                        if (string.IsNullOrEmpty(key))
                        {
                            Debug.Log("配置表在reg文件中不存在!请检查");
                            return;
                        } 
                        SheetClass sheetClass = allSheetClassDic[key];
                        int colCount = worksheet.Dimension.End.Column;
                        int rowCount = worksheet.Dimension.End.Row;
                        for (int j = 0; j < sheetClass.VarList.Count; j++)
                        {
                            sheetData.AllName.Add(sheetClass.VarList[j].Name);
                            sheetData.AllType.Add(sheetClass.VarList[j].Type);
                        }

                        for (int j = 1; j < rowCount; j++)
                        {
                            RowData rowData = new RowData();
                            int k = 0;
                            if (string.IsNullOrEmpty(sheetClass.SplitStr) && sheetClass.ParentVar != null &&
                                !string.IsNullOrEmpty(sheetClass.ParentVar.Foregin))
                            {
                                rowData.parentValue = worksheet.Cells[j + 1, 1].Value.ToString().Trim();
                                k = 1;
                            }
                            for (; k < colCount; k++)
                            {
                                ExcelRange range = worksheet.Cells[j + 1, k + 1];//索引从1开始
                                string value = "";
                                if (range.Value != null)
                                {
                                    value = range.Value.ToString().Trim();
                                }
                                string colValue = worksheet.Cells[1, k + 1].Value.ToString().Trim();
                                rowData.RowDataDic.Add(GetNameFormCol(sheetClass.VarList, colValue), value);
                            }
                            sheetData.AllData.Add(rowData);
                        }
                        allSheetDataDic.Add(worksheet.Name,sheetData);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        //根据类结构创建类，并将execl数据将变量赋值，然后调用xml序列化
        object objClass = CreatClass(className);
        List<SheetClass> outKeyList = new List<SheetClass>();
        foreach (string key in allSheetClassDic.Keys)
        {
            SheetClass sheet = allSheetClassDic[key];
            if (sheet.Depth == 1)
            {
                outKeyList.Add(sheet);
            }
        }

        for (int i = 0; i < outKeyList.Count; i++)
        {
            ReadDataToClass(objClass, allSheetClassDic[ParseDicKey(outKeyList[i])],
                allSheetDataDic[outKeyList[i].SheetName], allSheetClassDic, allSheetDataDic, null);
        }

        BinarySerializeOpt.XmlSerialize(XMLPATH + xmlName, objClass);
        Debug.Log(excelName+"表导入完成！");
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 返回转换后的key
    /// </summary>
    private static string ParseDicKey(SheetClass sheet)
    {
        return sheet.ParentVar.Name + "-" + sheet.SheetName;
    }

    /// <summary>
    /// 寻找结尾是指定sheetName的key值
    /// </summary>
    /// <param name="allSheetClassDic"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    private static string GetDicKey(Dictionary<string,SheetClass> allSheetClassDic,string sheetName)
    {
        foreach (var key in allSheetClassDic.Keys)
        {
            if (key.EndsWith(sheetName))
                return key;
        }
        return null;
    }
    /// <summary>
    /// 将数据转换为类
    /// </summary>
    /// <param name="objClass"></param>
    /// <param name="sheetClass"></param>
    /// <param name="sheetData"></param>
    /// <param name="allSheetClassDic"></param>
    /// <param name="allSheetDataDic"></param>
    private static void ReadDataToClass(object objClass, SheetClass sheetClass, SheetData sheetData,
        Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> allSheetDataDic,object keyValue)
    { 
        object item = CreatClass(sheetClass.Name);//获取列表变量类型
        object list = CreateList(item.GetType());
        for (int i = 0; i < sheetData.AllData.Count; i++)
        {
            if (keyValue != null && !string.IsNullOrEmpty(sheetData.AllData[i].parentValue))
            {
                if (sheetData.AllData[i].parentValue != keyValue.ToString())
                    continue;
            }
            object addItem = CreatClass(sheetClass.Name);
            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                VarClass varClass = sheetClass.VarList[j];
                if (varClass.Type == "list" && string.IsNullOrEmpty(varClass.SplitStr))
                {
                    ReadDataToClass(addItem, allSheetClassDic[varClass.ListSheetName],
                        allSheetDataDic[GetSheetName(varClass.ListSheetName)], allSheetClassDic, allSheetDataDic,
                        GetMemberValue(addItem, sheetClass.MainKey));
                }
                else if (varClass.Type == "list")
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
                    SetSplitClass(addItem, allSheetClassDic[varClass.ListSheetName], value);
                }
                else if (varClass.Type == "listStr" || varClass.Type == "listFloat" ||
                         varClass.Type == "listInt" || varClass.Type == "listBool")
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
                    SetSplitBaseClass(addItem, varClass, value);
                }
                else
                {
                    string value = sheetData.AllData[i].RowDataDic[sheetData.AllName[j]];
                    if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(varClass.DeafultValue))
                    {
                        value = varClass.DeafultValue;
                    }
                    else if (string.IsNullOrEmpty(value))
                    {
                        Debug.Log("表格中有空数据或者该属性的DeafultValue在reg中未配置" + varClass.Name);
                    }
                    SetValue(addItem, sheetData.AllName[j], value,sheetData.AllType[j]);
                }
            }

            list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
                new object[] { addItem });
        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass,list);
    }
    //获得sheet名字
    private static string GetSheetName(string dicSheetName)
    {
        return dicSheetName.Split('-')[1];
    }
    /// <summary>
    /// 设置分隔符class
    /// </summary>
    /// <param name="objClass"></param>
    /// <param name="sheetClass"></param>
    /// <param name="value"></param>
    private static void SetSplitClass(object objClass, SheetClass sheetClass, string value)
    {
        object item = CreatClass(sheetClass.Name);
        object list = CreateList(item.GetType());
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log(sheetClass.Name+"列有空值");
        }
        else
        {
            string[] rowArr = value.Trim().Split(new string[] { sheetClass.ParentVar.SplitStr.Replace("\\n", "\n").Replace("\\r", "\r") }, StringSplitOptions.None);
            for (int i = 0; i < rowArr.Length; i++)
            {
                object addItem = CreatClass(sheetClass.Name);
                string[] valueList = rowArr[i].Trim().Split(new string[] { sheetClass.SplitStr }, StringSplitOptions.None);
                for (int j = 0; j < valueList.Length; j++)
                {
                    SetValue(addItem, sheetClass.VarList[j].Name, valueList[j], sheetClass.VarList[j].Type);
                }
                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
                    new object[] { addItem });
            }
        }
        objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass, list);
    }

    /// <summary>
    /// 设置基础类型列表
    /// </summary>
    /// <param name="addItem"></param>
    /// <param name="varClass"></param>
    /// <param name="value"></param>
    private static void SetSplitBaseClass(object obj, VarClass varClass, string value)
    {
        Type type = null;
        switch (varClass.Type)
        {
            case "listStr":
                type = typeof(string);
                break;
            case "listInt":
                type = typeof(int);
                break;
            case "listFloat":
                type = typeof(float);
                break;
            case "listBool":
                type = typeof(bool);
                break;
        }
        if(type == null) return;
        object list = CreateList(type);
        string[] rowArr = value.Split(new string[]{varClass.SplitStr},StringSplitOptions.None);
        for (int i = 0; i < rowArr.Length; i++)
        {
            object addItem = rowArr[i].Trim();
            try
            {
                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list,
                    new object[] { addItem });
            }
            catch (Exception e)
            {
                Debug.Log(varClass.ListSheetName+"里"+varClass.Name+"列表添加失败，请具体数值是"+addItem);
            }
            
        }
        obj.GetType().GetProperty(varClass.Name).SetValue(obj,list);
    }

    /// <summary>
    /// 根据列名返回变量名
    /// </summary>
    /// <param name="varlist"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string GetNameFormCol(List<VarClass> varlist, string col)
    {
        foreach (VarClass varClass in varlist)
        {
            if (varClass.Col == col)
            {
                return varClass.Name;
            }
        }

        return null;
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
    [MenuItem("Assets/选中XML导出Execl")]
    public static void ExportExcel()
    {
        Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转成xml", "正在扫描：" + objects[i].name, 1.0f / objects.Length * i);
            XmlToExcel(objects[i].name.Replace(".xml",""));
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// xml反序列化
    /// </summary>
    /// <param name="className"></param>
    /// <param name="dataObj"></param>
    /// <returns></returns>
    private static object GetObjFromXml(string className)
    {
        object dataObj = null;
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
            dataObj = BinarySerializeOpt.XmlDeserialization(xmlPath, type);
        }
        return dataObj;
    }
    /// <summary>
    /// 反射解析列表返回Data
    /// </summary>
    /// <param name="dataObj"></param>
    /// <param name="sheet"></param>
    /// <param name="allSheetClassDic"></param>
    /// <param name="sheetData"></param>
    private static void ReflectParseList(object dataObj, SheetClass sheet, Dictionary<string, SheetClass> allSheetClassDic, 
        ref Dictionary<string, SheetData> sheetDataDic, string foreginKey = null, string foreginValue = null)
    {
        SheetData sheetData = null;
        if (sheetDataDic.ContainsKey(ParseDicKey(sheet)))
        {
            sheetData = sheetDataDic[ParseDicKey(sheet)];
        }
        else
        {
            sheetData = new SheetData();
        }
        if (sheetData.AllName.Count == 0)
        {
            if (foreginKey != null)
                sheetData.AllName.Add(foreginKey);
            for (int j = 0; j < sheet.VarList.Count; j++)
            {
                sheetData.AllName.Add(sheet.VarList[j].Col);
            }
        }
        
        object list = GetMemberValue(dataObj, sheet.ParentVar.Name);
        int count = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null,
            list, new object[] { }));
        object itemObj = CreatClass(sheet.Name);
        for (int i = 0; i < count; i++)
        {
            object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                list, new object[] { i });
            RowData data = new RowData();
            if ( foreginValue != null)
            {
                data.RowDataDic.Add(foreginKey, foreginValue);
            }
            for (int j = 0; j < sheet.VarList.Count; j++)
            {
                string value = null;
                if (sheet.VarList[j].Type == "listStr" || sheet.VarList[j].Type == "listFloat" ||
                    sheet.VarList[j].Type == "listInt" || sheet.VarList[j].Type == "listBool")
                {
                    value = GetSpliteBaseList(GetMemberValue(item, sheet.VarList[j].Name), sheet.VarList[j]);
                }
                else if (sheet.VarList[j].Type == "list" && !string.IsNullOrEmpty(sheet.VarList[j].SplitStr))
                {
                    value = GetSpliteClassList(GetMemberValue(item, sheet.VarList[j].Name), allSheetClassDic[sheet.VarList[j].ListSheetName], sheet.VarList[j]);
                }
                else if (sheet.VarList[j].Type == "list" && !string.IsNullOrEmpty(sheet.VarList[j].Foregin))
                {
                    string tempForValue = data.RowDataDic[sheet.VarList[j].Foregin];
                    ReflectParseList(item, allSheetClassDic[sheet.VarList[j].ListSheetName], allSheetClassDic,ref sheetDataDic, sheet.VarList[j].Foregin,tempForValue);
                    if (sheetData.AllName.Contains(sheet.VarList[j].Col))
                        sheetData.AllName.Remove(sheet.VarList[j].Col);
                }
                else
                {
                    value = GetMemberValue(item, sheet.VarList[j].Name).ToString();
                }
                data.RowDataDic.Add(sheet.VarList[j].Col, value);
            }
            sheetData.AllData.Add(data);
        }
        if (!sheetDataDic.ContainsKey(ParseDicKey(sheet)))
        {
            sheetDataDic.Add(ParseDicKey(sheet), sheetData);
        }
    }

    /// <summary>
    /// 解析xml
    /// </summary>
    /// <param name="xmlName"></param>
    /// <param name="className"></param>
    /// <param name="allSheetClassDic"></param>
    /// <param name="sheetDataDic"></param>
    private static void ParserXml(string xmlName, string className, ref Dictionary<string, SheetClass> allSheetClassDic, ref Dictionary<string, SheetData> sheetDataDic)
    {
        if (string.IsNullOrEmpty(xmlName))
        {
            Debug.LogError("xmlName配置为空");
            return;
        }
        try
        {
            object dataObj = null;
            dataObj = GetObjFromXml(className);
            foreach (SheetClass sheet in allSheetClassDic.Values)
            {
                if (!string.IsNullOrEmpty(sheet.ParentVar.SplitStr)) continue;
                if (sheet.ParentVar.ParentSheet != null) continue;
                ReflectParseList(dataObj, sheet, allSheetClassDic,ref sheetDataDic);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(className + "xml读取失败！" + e);
            return;
        }
    }
    /// <summary>
    /// 将列表中的特殊类转换为字符串返回
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sheet"></param>
    /// <param name="varClass"></param>
    /// <returns></returns>
    private static string GetSpliteClassList(object obj, SheetClass sheet, VarClass varClass)
    {
        string str = "";
        if (string.IsNullOrEmpty(varClass.SplitStr) || string.IsNullOrEmpty(varClass.Col))
        {
            Debug.LogError("字符分隔符或列名为空，请检查:" + varClass.Name);
            return str;
        }
        int listCount = System.Convert.ToInt32(obj.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null,
            obj, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = obj.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                obj, new object[] { i });
            string tempStr = "";
            for (int j = 0; j < sheet.VarList.Count; j++)
            {
                if (j != sheet.VarList.Count - 1)
                    tempStr += GetMemberValue(item, sheet.VarList[j].Name) + sheet.SplitStr;
                else
                    tempStr += GetMemberValue(item, sheet.VarList[j].Name);
            }

            tempStr = tempStr.Replace("\\n", "\n").Replace("\\r","\r");
            if (i != listCount - 1)
                str += tempStr + varClass.SplitStr;
            else
                str += tempStr;
        }
        str = str.Replace("\\n", "\n");
        return str;
    }
    /// <summary>
    /// 解析基本类型列表，结合成字符串返回
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="varClass"></param>
    /// <returns></returns>
    private static string GetSpliteBaseList(object obj, VarClass varClass)
    {
        string str = "";
        if (string.IsNullOrEmpty(varClass.SplitStr))
        {
            Debug.LogError("字符分隔符为空，请检查:"+varClass.Name);
            return str;
        }
        int listCount = System.Convert.ToInt32(obj.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null,
            obj, new object[] { }));
        for (int i = 0; i < listCount; i++)
        {
            object item = obj.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                obj, new object[] { i });
            if (i != listCount - 1)
                str += item.ToString() + varClass.SplitStr;
            else
                str += item.ToString();
        }

        return str;
    }

    /// <summary>
    /// 将数据类导出为excel
    /// </summary>
    /// <param name="excelName"></param>
    /// <param name="allSheetClassDic"></param>
    /// <param name="sheetDataDic"></param>
    private static void ExportExcel(string excelName, ref Dictionary<string, SheetClass> allSheetClassDic, Dictionary<string, SheetData> sheetDataDic)
    {
        string exprotExcelPath = Application.dataPath + "/../Data/Excel/" + excelName;
        if (!CheckFileIsUse(exprotExcelPath))
        {
            FileInfo file = new FileInfo(exprotExcelPath);
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(exprotExcelPath);
            }

            using (ExcelPackage excel = new ExcelPackage(file))
            {
                foreach (string sheetName in allSheetClassDic.Keys)
                {
                    if (!sheetDataDic.ContainsKey(sheetName)) continue;
                        ExcelWorksheet sheet = excel.Workbook.Worksheets.Add(allSheetClassDic[sheetName].SheetName);
                    //sheet.Cells.AutoFitColumns();//自动宽度
                    //設置第一行
                    for (int i = 0; i < sheetDataDic[sheetName].AllName.Count; i++)
                    {
                        sheet.SetValue(1, i + 1, sheetDataDic[sheetName].AllName[i]);
                    }
                    //设置n+1行
                    for (int i = 1; i <= sheetDataDic[sheetName].AllData.Count; i++)
                    {
                        for (int j = 0; j < sheetDataDic[sheetName].AllName.Count; j++)
                        {
                            string data = sheetDataDic[sheetName].AllData[i - 1].RowDataDic[sheetDataDic[sheetName].AllName[j]];

                            if (data.Contains("\n") || data.Contains("\r"))
                                sheet.Cells[i + 1, j + 1].Style.WrapText = true;
                            sheet.SetValue(i + 1, j + 1, data);
                            sheet.Cells[i + 1, j + 1].AutoFitColumns();
                        }
                    }
                }
                excel.Save();
                Debug.Log(excelName + "导出成功！");
            }
        }
        else
        {
            Debug.LogError(excelName + "文件已被打开，请检查");
        }
    }

    /// <summary>
    /// xml转excel
    /// </summary>
    public static void XmlToExcel(string xmlRegName)
    {
        string className = "";
        string excelName = "";
        string xmlName = "";
        Dictionary<string, SheetClass> allSheetClassDic = ReadReg(xmlRegName,ref className,ref excelName,ref xmlName);
        //根据xml配置生成类
        //反序列化xml
        Dictionary<string, SheetData> sheetDataDic = new Dictionary<string, SheetData>();
        ParserXml(xmlName, className, ref allSheetClassDic, ref sheetDataDic);

        //类导出Excel
        ExportExcel(excelName, ref allSheetClassDic, sheetDataDic);
    }

    private static Dictionary<string, SheetClass> ReadReg(string xmlRegName, ref string className, ref string excelName, ref string xmlName)
    {
        string xmlRegPath = REGPATH + xmlRegName + ".xml";
        if (!File.Exists(xmlRegPath))
        {
            Debug.LogError(xmlRegName + "不正确，请检查：" + xmlRegPath);
            return null;
        }
        XmlDocument xmlDocument = new XmlDocument();
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;//忽略xml文档注释
        XmlReader xmlReader = XmlReader.Create(xmlRegPath,readerSettings);
        xmlDocument.Load(xmlReader);
        //解析reg文档配置
        Dictionary<string, SheetClass> allSheetClassDic = new Dictionary<string, SheetClass>();
        //拿data
        XmlElement dataElement = xmlDocument.SelectSingleNode("data") as XmlElement;
        className = dataElement.GetAttribute("name");
        excelName = dataElement.GetAttribute("from");
        xmlName = dataElement.GetAttribute("to");
        foreach (XmlElement attrElement in dataElement.ChildNodes)
        {
            ReadXmlNode(1,attrElement, allSheetClassDic, null);
        }
        xmlReader.Close();
        return allSheetClassDic;
    }

    /// <summary>
    /// 检查文件是否正在被打开
    /// </summary>
    public static bool CheckFileIsUse(string filePath)
    {
        FileInfo file = new FileInfo(filePath);
        FileStream fs = null;
        if (!file.Exists)
            return false;
        try
        {
            fs = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return true;
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
        }
    }

    /// <summary>
    /// 解析xml配置
    /// </summary>
    /// <param name="attrElement"></param>
    public static void ReadXmlNode(int depth,XmlElement attrElement,Dictionary<string, SheetClass> allSheetClassDic,SheetClass parentSheet)
    {
        VarClass attrVar = new VarClass()
        {
            Name = attrElement.GetAttribute("name"),
            Col = attrElement.GetAttribute("col"),
            Type = attrElement.GetAttribute("type"),
            DeafultValue = attrElement.GetAttribute("defaultValue"),
            Foregin = attrElement.GetAttribute("foregin"),
            SplitStr = attrElement.GetAttribute("split"),
        };

        if (parentSheet != null)
        {
            parentSheet.VarList.Add(attrVar);
            attrVar.ParentSheet = parentSheet;
        }
        if (attrElement.GetAttribute("type") == "list")
        {
            XmlElement listElement = attrElement.FirstChild as XmlElement;
            SheetClass sheet = new SheetClass();
            sheet.Name = listElement.GetAttribute("name");
            sheet.SheetName = listElement.GetAttribute("sheetname");
            sheet.MainKey = listElement.GetAttribute("mainKey");
            sheet.SplitStr = listElement.GetAttribute("split");
            sheet.ParentVar = attrVar;
            sheet.Depth = depth;
            if (!string.IsNullOrEmpty(sheet.SheetName))
            {
                if (!allSheetClassDic.ContainsKey(ParseDicKey(sheet)))
                {
                    foreach (XmlElement itemElement in listElement.ChildNodes)
                    {
                        ReadXmlNode(++depth,itemElement, allSheetClassDic, sheet);
                    }
                    allSheetClassDic.Add(ParseDicKey(sheet), sheet);
                    attrVar.ListSheetName = ParseDicKey(sheet);
                }
            }
        }
    }

    #region 测试用方法
    //[MenuItem("Tools/测试读取xml")]
    //public static void TestReadXml()
    //{
    //    string xmlPath = Application.dataPath + "/../Data/Reg/MonsterData.xml";
    //    try
    //    {
    //        XmlDocument xmlDocument = new XmlDocument();
    //        XmlReader xmlReader = XmlReader.Create(xmlPath);
    //        xmlDocument.Load(xmlReader);
    //        XmlElement dataElement = (XmlElement)xmlDocument.SelectSingleNode("data");
    //        string className = dataElement.GetAttribute("name");
    //        string excelName = dataElement.GetAttribute("from");
    //        string xmlName = dataElement.GetAttribute("to");
    //        foreach (XmlNode variableNode in dataElement.ChildNodes)
    //        {
    //            XmlElement variableElement = variableNode as XmlElement;
    //            string attrName = variableElement.GetAttribute("name");
    //            string attrType = variableElement.GetAttribute("type");
    //            Debug.Log(attrName + "-" + attrType);
    //            XmlElement listElement = variableElement.FirstChild as XmlElement;
    //            foreach (XmlNode itemNode in listElement.ChildNodes)
    //            {
    //                XmlElement itemElement = itemNode as XmlElement;
    //                string itemAttrName = itemElement.GetAttribute("name");
    //                string itemAttrType = itemElement.GetAttribute("type");
    //                string itemColName = itemElement.GetAttribute("col");
    //                Debug.Log(itemAttrName + "-" + itemAttrType + "-" + itemColName);
    //            }
    //        }
    //        Debug.Log(className + "-" + excelName + "-" + xmlName);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("xml读取异常：" + e);
    //    }
    //}
    ///// <summary>
    ///// 测试输出excel
    ///// </summary>
    //[MenuItem("Tools/测试输出excel")]
    //public static void TestExportExcel()
    //{
    //    string excelPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
    //    FileInfo file = new FileInfo(excelPath);
    //    if (file.Exists)
    //    {
    //        file.Delete();
    //        file = new FileInfo(excelPath);
    //    }

    //    using (ExcelPackage excel = new ExcelPackage(file))
    //    {
    //        ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("怪物配置表");
    //        sheet.SetValue(1, 1, "测试");
    //        //sheet.Cells[1, 1].Value = "测试2";
    //        excel.Save();
    //    }
    //}

    //[MenuItem("Tools/测试已有类对象进行反射")]
    //public static void TestReflectByObj()
    //{
    //    TestClass test = new TestClass()
    //    { Age = 10, Id = 1, Name = "hhhh", AllStrList = new List<string>() { "11", "22", "33" }, AllClassList = new List<TestClass2>() };
    //    for (int i = 0; i < 3; i++)
    //    {
    //        TestClass2 obj = new TestClass2();
    //        obj.Id = i;
    //        obj.Name = "Name" + i;
    //        test.AllClassList.Add(obj);
    //    }

    //    //反射列表
    //    //object list = GetMemberValue(test, "AllStrList");
    //    //int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count",BindingFlags.Default|BindingFlags.InvokeMethod,null, list, new object[] { }));
    //    //for (int i = 0; i < listCount; i++)
    //    //{
    //    //    string item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i }).ToString();
    //    //    Debug.Log(item);
    //    //}

    //    //反射获得列表里所有数据
    //    object list = GetMemberValue(test, "AllClassList");
    //    int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { }));
    //    for (int i = 0; i < listCount; i++)
    //    {
    //        object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[] { i });
    //        MemberInfo[] members = item.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
    //        foreach (var m in members)
    //        {
    //            Debug.Log("i=" + i + " name=" + m.Name + " value=" + GetMemberValue(item, m.Name));
    //        }
    //    }
    //}

    //[MenuItem("Tools/测试已有数据进行反射")]
    //public static void TestReflectByName()
    //{
    //    object obj = CreatClass("TestClass");

    //    //PropertyInfo agetProperty = obj.GetType().GetProperty("Age");
    //    //agetProperty.SetValue(obj,System.Convert.ToInt32("20"));
    //    //PropertyInfo heightProperty = obj.GetType().GetProperty("Height");
    //    //heightProperty.SetValue(obj, System.Convert.ToSingle("222.2222"));
    //    //PropertyInfo testEnumProperty = obj.GetType().GetProperty("TestEnum");
    //    //object enumObj = TypeDescriptor.GetConverter(testEnumProperty.PropertyType).ConvertFromInvariantString("Type1");
    //    //testEnumProperty.SetValue(obj,enumObj);
    //    SetValue(obj, "Age", "20", "int");
    //    SetValue(obj, "Height", "222.2222", "float");
    //    SetValue(obj, "TestEnum", "Type1", "enum");
    //    object list = CreateList<string>();
    //    for (int i = 0; i < 5; i++)
    //    {
    //        object item = "item" + i;
    //        list.GetType().InvokeMember("Add", BindingFlags.InvokeMethod | BindingFlags.Default, null, list, new object[] { item });
    //    }
    //    obj.GetType().GetProperty("AllStrList").SetValue(obj, list);

    //    object classList = CreateList<TestClass2>();
    //    for (int i = 0; i < 5; i++)
    //    {
    //        object item = CreatClass("TestClass2");
    //        SetValue(item, "Id", "" + i, "int");
    //        SetValue(item, "Name", "name" + i, "string");
    //        classList.GetType().InvokeMember("Add", BindingFlags.InvokeMethod | BindingFlags.Default, null, classList,
    //            new object[] { item });
    //    }
    //    obj.GetType().GetProperty("AllClassList").SetValue(obj, classList);
    //    TestClass test = obj as TestClass;
    //    foreach (var str in test.AllStrList)
    //    {
    //        Debug.Log(str);
    //    }

    //    Debug.Log(test.Age + "-" + test.Height + "-" + test.TestEnum);
    //}

    #endregion


    private static object CreateList<T>() where T : class
    {
        Type tType = typeof(T);
        Type listType = typeof(List<>);
        Type specType = listType.MakeGenericType(new Type[] { tType });//确定泛型类的type
        return Activator.CreateInstance(specType);
    }
    private static object CreateList(Type tType)
    {
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
            case "string":
                val = System.Convert.ToString(value);
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

public class SheetClass
{
    //类名
    public string Name { get; set; }
    //类对应的sheet名
    public string SheetName { get; set; }
    //主键
    public string MainKey { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    public VarClass ParentVar { get; set; }
    //子节点列表
    public List<VarClass> VarList = new List<VarClass>();
    public int Depth { get; set; }
}

/// <summary>
/// 属性中间类
/// </summary>
public class VarClass
{
    //原类中变量名称
    public string Name { get; set; }
    //原类变量类型
    public string Type { get; set; }
    //excel列名
    public string Col { get; set; }
    //若不填，列的默认值
    public string DeafultValue { get; set; }
    //如果变量为list，外联部分列
    public string Foregin { get; set; }
    //分隔符
    public string SplitStr { get; set; }
    //包含的sheet列表的名字
    public string ListSheetName { get; set; }
    //自己所包在的父列表
    public SheetClass ParentSheet { get; set; }
}

public class SheetData
{
    public List<string> AllName = new List<string>();
    public List<string> AllType = new List<string>();
    public List<RowData> AllData = new List<RowData>();
}

public class RowData
{
    public string parentValue { get; set; }
    public Dictionary<string,string> RowDataDic = new Dictionary<string, string>();
}

#region 测试用类型

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
    public int Id { get; set; }
    public string Name { get; set; }
}
#endregion
