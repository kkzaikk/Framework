using UnityEngine;
using System.Collections;

/// <summary>
/// 文件更新
/// </summary>
public class RefreshFileInfo
{
    /// <summary>
    /// 打包的Resource完整路径
    /// </summary>
    public string path
    {
        private set;
        get;
    }

    /// <summary>
    /// 打包唯一值，加载路径
    /// </summary>
    public string guiName
    {
        private set;
        get;
    }

    //hash值，打包是否发生变化对比值
    public Hash128 abHash
    {
        set;
        get;
    }

    public RefreshFileInfo()
    {

    }

    //构造函数，设置基础数据
    public RefreshFileInfo(string tmpPath, string tmpGuiName, Hash128 tmpHash)
    {
        path = tmpPath;
        guiName = tmpGuiName;
        abHash = tmpHash;
    }

    //设置基础数据
    public void SetData(string tmpPath, string tmpGuiName, Hash128 tmpHash)
    {
        path = tmpPath;
        guiName = tmpGuiName;
        abHash = tmpHash;
    }
}
