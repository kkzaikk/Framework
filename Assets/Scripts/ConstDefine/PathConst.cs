using UnityEngine;
using System.Collections;

public class PathConst
{
    /// <summary>
    /// 全部打包
    /// </summary>
    public static string[] aFolders = new string[] { @"LuaScript" };


    /// <summary>
    /// 文件夹打包
    /// </summary>
    public static string[] fFolders = new string[] { @"GameData" };


    /// <summary>
    /// ////////////////////////////////////////////////////////////
    /// </summary>


    public const bool DebugMode = true;//调试模式-用于内部测试

    public const string VersionName = "Version.txt";

    public const string AssetDir = "StreamingAssets";//素材目录

    public const string AppName = "LuaFramework";//应用程序名称

    public const string ExtName = ".assetbundles";//素材扩展名

    /// <summary>
    /// 应用程序内容路径
    /// </summary>
    public static string AppContentPath()
    {
        string path = string.Empty;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = "jar:file://" + Application.dataPath + "!/assets/";
                break;
            case RuntimePlatform.IPhonePlayer:
                path = Application.dataPath + "/Raw/";
                break;
            default:
                path = Application.dataPath + "/" + AssetDir + "/";
                break;
        }
        return path;
    }

    /// <summary>
    /// 取得数据存放目录
    /// </summary>
    public static string DataPath
    {
        get
        {
            string game = AppName.ToLower();
            if (Application.isMobilePlatform)
            {
                return Application.persistentDataPath + "/" + game + "/";
            }

            if (DebugMode)
            {
                return Application.dataPath + "/" + AssetDir + "/";
            }

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                int i = Application.dataPath.LastIndexOf('/');
                return Application.dataPath.Substring(0, i + 1) + game + "/";
            }

            return "c:/" + game + "/";
        }
    }

    public static string GetRelativePath()
    {
        if (Application.isEditor)
            return "file:///" + System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/" + AssetDir + "/";
        else if (Application.isMobilePlatform || Application.isConsolePlatform)
            return "file:///" + DataPath;
        else // For standalone player.
            return "file://" + Application.streamingAssetsPath + "/";
    }
}
