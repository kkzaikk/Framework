using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 更新文件数据,打包AB专用，为了独立打包工具，和正式工程的区分，其实数据格式一样
/// </summary>
public class RenewalFileInfoTmp
{
    /// <summary>
    /// 打包的Resource完整路径
    /// </summary>
    public string realPackPath
    {
        private set;
        get;
    }

    /// <summary>
    /// 打包唯一值，加载路径
    /// </summary>
    public string guidName
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

    public RenewalFileInfoTmp()
    {

    }

    //设置基础数据
    public void SetData(string pathTmp, string gn)
    {
        realPackPath = pathTmp;
        guidName = gn;
    }


}
