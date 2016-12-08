using UnityEngine;
using System.Collections;
using System.IO;

public class ExportFileInfo
{
    /// <summary>
    /// 真实打包的路径,
    /// 例如Assets/Resources/LuaScript/K3.txt   的真实打包路径   Assets/Resources/LuaScript
    /// </summary>
    public string realPackPath
    {
        get;
        set;
    }

    /// <summary>
    /// 是否全部打包
    /// </summary>
    public bool isURLA
    {
        get;
        set;
    }

    /// <summary>
    /// 是否根据文件夹打包
    /// </summary>
    public bool isURLF
    {
        get;
        set;
    }

    /// <summary>
    /// 全部打包的基础文件路径
    /// </summary>
    public string URLA
    {
        get;
        set;
    }

    /// <summary>
    /// 是否分离打包
    /// </summary>
    public bool isURLB
    {
        get;
        set;
    }

    /// <summary>
    /// 是否根据预设打包
    /// </summary>
    public bool isURLP
    {
        get;
        set;
    }

    /// <summary>
    /// 是否不打包
    /// </summary>
    public bool isURLE
    {
        get;
        set;
    }

    /// <summary>
    /// 是否在Resource目录下
    /// </summary>
    public bool isInResource
    {
        get;
        set;
    }

    /// <summary>
    /// 依赖次数
    /// </summary>
    public int readPackTime
    {
        get;
        private set;
    }

    /// <summary>
    /// 当前文件路径,包含文件后缀名
    /// </summary>
    public string path
    {
        get;
        private set;
    }

    public string ext
    {
        get
        {
            return Path.GetExtension(path);
        }
    }

    /// <summary>
    /// 构造一个导出文件信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isIn"></param>
    public ExportFileInfo(string p,bool isIn)
    {
        path = p;
        realPackPath = p;
        isInResource = isIn;
        readPackTime = 0;
        isURLA = false;
        isURLB = false;
        isURLP = false;

    }

    /// <summary>
    /// 依赖次数加1
    /// </summary>
    public void AddReadPackTime()
    {
        ++readPackTime;
    }



}
