using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildApp
{
    public const string m_AppName = "ChuHeHanJie";
    public static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
    public static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS/";
    public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";

    public static void Build()
    {
        string savePath = "";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                savePath = m_AndroidPath+m_AppName+"_"+EditorUserBuildSettings.activeBuildTarget+string.Format("_{0:yyyy_MM_dd_HH_mm}",DateTime.Now)+".apk";
                break;
            case BuildTarget.iOS:
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now,m_AppName);
                break;
            default:
                Debug.Log("当前选择平台不支持自动打包" + EditorUserBuildSettings.activeBuildTarget);
                break;
        }
        BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath , EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
    }

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
