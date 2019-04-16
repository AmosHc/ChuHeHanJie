using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildApp
{
    public static string m_AppName = PlayerSettings.productName;//应用名
    public static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";//Android发布路径
    public static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS/";//IOS
    public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";//Windows

    [MenuItem("Build/标准包")]
    public static void Build()
    {
        //打最新的ab包
        BundleEditor.Build();
        //打包
        string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString()+"/";
        string targetPath = AssetBundleManger.Instance.ABLoadpath;
        Copy(abPath, targetPath);//复制ab包到streamasset/AssetBundle
        string savePath = "";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
                           string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
                break;
            case BuildTarget.iOS:
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
                           string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, m_AppName);
                break;
            default:
                Debug.Log("当前选择平台不支持自动打包" + EditorUserBuildSettings.activeBuildTarget);
                break;
        }
        BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath , EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
        Delete(targetPath);
    }

    private static void Delete(string srcPath)
    {
        try
        {
            DirectoryInfo info = new DirectoryInfo(srcPath);
            FileSystemInfo[] files = info.GetFileSystemInfos();
            foreach (FileSystemInfo fileInfo in files)
            {
                if (fileInfo is DirectoryInfo)
                {
                    DirectoryInfo subInfo = new DirectoryInfo(fileInfo.FullName);
                    subInfo.Delete(true);
                }
                else
                {
                    File.Delete(fileInfo.FullName);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

    }

    /// <summary>
    /// 将ab包从ab包路径复制到streamasset进行打包
    /// </summary>
    /// <param name="abPath"></param>
    /// <param name="streamingAssetsPath"></param>
    private static void Copy(string srcPath, string targetPath)
    {
        try
        {
            if (!Directory.Exists(srcPath))
            {
                Directory.CreateDirectory(srcPath);
            }

            string srcDir = Path.Combine(targetPath, Path.GetFileName(srcPath));
            Debug.Log(srcDir);
            if (Directory.Exists(srcPath))
                srcDir += Path.DirectorySeparatorChar;//如果它是文件夹就加上目录分隔符
            if (!Directory.Exists(srcDir))
                Directory.CreateDirectory(srcDir);//创建文件夹

            string[] filePaths = Directory.GetFileSystemEntries(srcPath);
            foreach (string file in filePaths)
            {
                if (Directory.Exists(file))
                {
                    Copy(file,srcPath);
                }
                else
                {
                    File.Copy(file, srcDir + Path.GetFileName(file),true);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("无法复制" + srcPath + "到" + targetPath);
            throw;
        }
        
    }
    /// <summary>
    /// 找到激活的场景
    /// </summary>
    /// <returns></returns>
    private static string[] FindEnableEditorScenes()
    {
        List<string> editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if(!scene.enabled) continue;
            editorScenes.Add(scene.path);
        }
        return editorScenes.ToArray();
    }
}
