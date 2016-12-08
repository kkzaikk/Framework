using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class BuildAB : Editor
{
    /// <summary>
    /// 是否删除manifest文件
    /// </summary>
    private static string _isDeleteManifest = "0";

    /// <summary>
    /// 配置文件路径是否带后缀名
    /// </summary>
    private static string _IsExt = "0";

    /// <summary>
    /// 需要打包的源文件夹
    /// </summary>
    private static string _PackDir = "";

    /// <summary>
    /// 最终生成AB包的路径
    /// </summary>
    private static string _TargDir = "";

    /// <summary>
    /// 打包到服务器地址
    /// </summary>
    private static string _ServerPath = "";

    /// <summary>
    /// 最终生成AB包的文件夹名（为了加载manifeset）
    /// </summary>
    private static string _TargFol = "";

    /// <summary>
    /// 更新文件名
    /// </summary>
    private static string _VersionName = "";

    /// <summary>
    /// 生成更新列表txt
    /// </summary>
    private static string _VersionPath = "";

    /// <summary>
    /// 存放到服务器的版本文件路径
    /// </summary>
    private static string _VersionServerPath = "";

    /// <summary>
    /// 版本号文件路径
    /// </summary>
    private static string _VersionTimePath = "";

    /// <summary>
    /// 版本号文件名
    /// </summary>
    private static string _VersionTimeName = "";

    /// <summary>
    /// 生成AB包的后缀名
    /// </summary>
    private static string _AbExtName = "";

    /// <summary>
    /// 打包的版本号
    /// </summary>
    private static string _Version = "";

    //全部打包
    private static List<string> _APack = new List<string>();
    //文件夹打包
    private static List<string> _FPack = new List<string>();
    //不打包
    private static List<string> _EPack = new List<string>();

    /// <summary>
    /// 图集归并
    /// </summary>
    private static Dictionary<string, string> _mergerDict = new Dictionary<string, string>();

    /// <summary>
    /// 原则上单独打包的后缀名列表
    /// 是否单独打包，如果为真，则是单独打包
    /// 例如A引用了B，B的后缀名满足条件，则A和B分离打包
    /// </summary>
    private static List<string> _DivisionPackExt = new List<string>();

    /// <summary>
    /// 不打包后缀名
    /// </summary>
    private static List<string> _NoPackExt = new List<string>();

    /// <summary>
    /// 配置文件格式
    /// </summary>
    private static List<string> _ConfigRule0 = new List<string>();
    private static string _ConfigRule1 = "";
    private static string _ConfigRule2 = "";

    /// <summary>
    /// resources目录下的所有文件
    /// </summary>
    private static Dictionary<string, ExportFileInfo> _allExportFileDict = new Dictionary<string, ExportFileInfo>();
    private static Dictionary<string, ExportFileInfo> _tmpExportFileDict = new Dictionary<string, ExportFileInfo>();

    //更新文件信息
    private static Dictionary<string, RenewalFileInfoTmp> _renewalFileList = new Dictionary<string, RenewalFileInfoTmp>();

    /// <summary>
    /// 根据配置表初始化一些参数，如路径之类
    /// </summary>
    public static void InitConfig()
    {
        StreamReader sr = new StreamReader(Application.dataPath+ "/Editor/BuildAssetBundleTool/buildSetupInfo.txt", Encoding.UTF8);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            if (line != "" && !line.StartsWith("--"))
            {
                string[] spaces = line.Split(',');
                switch (spaces[0])
                {
                    case "IsDeleteManifest":
                        _isDeleteManifest = spaces[1];
                        break;
                    case "IsExt":
                        _IsExt = spaces[1];
                        break;
                    case "PackDir":
                        _PackDir = Application.dataPath + "/" + spaces[1];
                        break;
                    case "TargDir":
                        _TargDir = Application.dataPath + "/" + spaces[1];
                        break;
                    case "ServerPath":
                        _ServerPath = spaces[1];
                        break;
                    case "VersionServerPath":
                        _VersionServerPath = spaces[1];
                        break;
                    case "VersionName":
                        _VersionName = spaces[1];
                        break;
                    case "TargFol":
                        _TargFol = spaces[1];
                        break;
                    case "VersionPath":
                        _VersionPath = Application.dataPath + "/" + spaces[1];
                        break;
                    case "AbExtName":
                        _AbExtName = spaces[1];
                        break;
                    case "Version":
                        _Version = spaces[1];
                        break;
                    case "APack":
                        _APack.Clear();
                        string[] a_folders = spaces[1].Split('#');
                        for (int i = 0; i < a_folders.Length; ++i)
                        {
                            _APack.Add(a_folders[i].Replace(@"\", @"/"));
                        }  
                        break;
                    case "FPack":
                        _FPack.Clear();
                        string[] f_folders = spaces[1].Split('#');
                        for (int i = 0; i < f_folders.Length; ++i)
                        {
                            _FPack.Add(f_folders[i].Replace(@"\", @"/"));
                        } 
                        break;
                    case "EPack":
                        _EPack.Clear();
                        string[] e_folders = spaces[1].Split('#');
                        for (int i = 0; i < e_folders.Length; ++i)
                        {
                            _EPack.Add(e_folders[i].Replace(@"\", @"/"));
                        } 
                        break;
                    case "Merger":
                        _mergerDict.Clear();
                        string[] m_folders = spaces[1].Split('#');
                        for (int i = 0; i < m_folders.Length; ++i)
                        {
                            if (!string.IsNullOrEmpty(m_folders[i]))
                            {
                                string[] space = m_folders[i].Replace(@"\", @"/").Split(';');
                                _mergerDict[space[0]] = space[1];
                            }
                        }
                        break;
                    case "DivisionPackExt":
                        _DivisionPackExt.Clear();
                        string[] divi_folders = spaces[1].Split('#');
                        _DivisionPackExt.AddRange(divi_folders);
                        break;
                    case "NoPackExt":
                        _NoPackExt.Clear();
                        string[] no_folders = spaces[1].Split('#');
                        _NoPackExt.AddRange(no_folders);
                        break;
                    case "ConfigRule0":
                        _ConfigRule0.Clear();
                        _ConfigRule0.AddRange(spaces[1].Split('#'));
                        break;
                    case "ConfigRule1":
                        _ConfigRule1 = spaces[1];
                        break;
                    case "ConfigRule2":
                        _ConfigRule2 = spaces[1];
                        break;
                    case "VersionTimePath":
                        _VersionTimePath = Application.dataPath + "/" + spaces[1];
                        break;
                    case "VersionTimeName":
                        _VersionTimeName = spaces[1];
                        break;
                    default:
                        break;
                }
            }
        }
        sr.Close();
    }

    /// <summary>
    /// 从配置表判断是否要打包
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool IsEPackPath(string path)
    {
        foreach (var v in _EPack)
        {
            if (ConvertPath(path).Contains("Assets/" + v))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 分步打包
    /// </summary>
    /// <param name="packDir"></param>
    public static void BuildPartCore(string packDir)
    {
        BuildCore(packDir,_TargDir + "/" + _TargFol, _VersionPath);
    }

    /// <summary>
    /// 打包核心代码
    /// </summary>
    public static void BuildCore(string packDir, string outputhPath,string versionPath)
    {
        //先删除文件夹的AB包
        DeleteFileOrFolder(outputhPath);
        _allExportFileDict.Clear();
        _tmpExportFileDict.Clear();
        _renewalFileList.Clear();

        bool isAllPack = false;
        if (packDir == _PackDir) isAllPack = true;

        //判断文件夹路径是否存在，不存在则创建
        if (!Directory.Exists(outputhPath))
        {
            Directory.CreateDirectory(outputhPath);
        }

        //遍历Resource的文件，全部资源都设置AB的文件名
        //找出所有依赖
        //过滤依赖， 分类，A,B,P.E,F
        //存入字典
        //目录下的所有文件和文件夹
        string[] files = Directory.GetFiles(packDir, "*.*", SearchOption.AllDirectories);
        string[] directorys = Directory.GetDirectories(packDir, "*", SearchOption.AllDirectories);
        List<string> directoryList = new List<string>();
        directoryList.Add(packDir);
        directoryList.AddRange(directorys);

        foreach (var path in files)
        {
            string ext = Path.GetExtension(path);
            if (!IsNoPack(ext) && !IsEPackPath(path))
            {
                string conv_path = ConvertPath(path);
                _allExportFileDict[conv_path] = new ExportFileInfo(conv_path, true);
                _tmpExportFileDict[conv_path] = new ExportFileInfo(conv_path, true);
            }
        }

        //查找Resource资源下的所有依赖

        //1.在resources目录下，如果依赖只有一个，且不是IsOutResourceExt的类型，则设置它info.dependUrl为空，表示该资源部打包，则引用它的父体和该资源合成一个ab包
        //例如：A、B都在resources目录下，只有A引用到了B，B的引用次数为1，则会把B的名称设为空，表示不打包。然后打包A的时候，就自动把B打包进A。
        //2.如果一个在resources目录下，一个在其他目录，大体原则上是合包，但是满足IsOutResourceExt条件，则分离包。

        foreach (var d in _tmpExportFileDict)
        {
            string path = d.Key;
            ExportFileInfo fileInfo = d.Value;
            if (!IsNoPack(fileInfo.ext))
            {
                string[] depends = AssetDatabase.GetDependencies(path);

                if (depends != null && depends.Length > 0)
                {
                    foreach (var depend in depends)
                    {
                        if (path != depend)
                        {
                            //如果包含了，即在resources目录下
                            if (_tmpExportFileDict.ContainsKey(depend))
                            {
                                _tmpExportFileDict[depend].AddReadPackTime();
                                _allExportFileDict[depend] = _tmpExportFileDict[depend];
                            }
                            else//不在resources目录下
                            {
                                //原则上是resourceslib里的都要跟resource合成一个包，除了IsOutResourceExt条件，满足IsOutResourceExt条件的则分离
                                if (!_allExportFileDict.ContainsKey(depend) && IsOutResourceExt(Path.GetExtension(depend)))
                                {
                                    ExportFileInfo info = new ExportFileInfo(depend, false);

                                    _allExportFileDict[depend] = info;
                                }
                            }
                        }
                    }
                }
            }
        }

        //过滤依赖
        foreach (var d in _allExportFileDict)
        {
            ExportFileInfo info = d.Value;
            if (info.readPackTime == 1 && !IsOutResourceExt(info.ext))
            {
                info.realPackPath = "";
            }
        }

        //设置AB名字
        //分类，A,B,P.E,F
        foreach (var direPath in directoryList)
        {
            string path = ConvertPath(direPath);
            if (IsTypeF(path))
            {
                SetAllDForF(direPath);
            }
            else if (IsTypeA(path))
            {
                SetAllDForA(direPath);
            }
        }

        //调用API,设置AB名字
        foreach (var d in _allExportFileDict)
        {
            ExportFileInfo info = d.Value;

            if (!IsNoPack(info.ext))//脚本不打包
            {
                if (info.realPackPath != "")
                {
                    string basePathGUID = AssetDatabase.AssetPathToGUID(info.realPackPath) + _AbExtName;

                    AssetImporter importer = AssetImporter.GetAtPath(info.path);
                    if (importer)
                    {
                        if (importer.assetBundleName != "" || basePathGUID != "")
                        {
                            importer.assetBundleName = basePathGUID;
                        }
                    }
                }
            }
        }

        //刷新
        AssetDatabase.Refresh();

        //是否强制重新打包输出
        bool isForceRebuild = false;

        //生成AB包

        BuildTarget target;

#if UNITY_STANDALONE_WIN
        target = BuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
        target = BuildTarget.Android;
#elif UNITY_IPHONE
        target = BuildTarget.iPhone;
#else
        target = BuildTarget.StandaloneWindows;
#endif

        BuildAssetBundleOptions options = BuildAssetBundleOptions.UncompressedAssetBundle;

        //是否强制打包
        if (isForceRebuild)
        {
            BuildPipeline.BuildAssetBundles(outputhPath, options | BuildAssetBundleOptions.ForceRebuildAssetBundle, target);
        }
        else
        {
            BuildPipeline.BuildAssetBundles(outputhPath, options, target);
        }

        //_merger_source = "";
        //_merger_Mapping = "";
        //把散图打包到Resources下对应的图集预设(适用于UGUI)
        foreach (var d in _mergerDict)
        {
            SetSource2AtlasPrefab(Application.dataPath + "/" + d.Key, Application.dataPath + "/" + d.Value);
        }

        //刷新
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log("AB包打包成");

        //更新文件
        //设置路径和需要生成的带所有文件的更新文件
        //从abmanifest中获取hash值
        //更新文件的数据
        foreach (var d in _allExportFileDict)
        {
            ExportFileInfo info = d.Value;
            if (!IsNoPack(info.ext))
            {
                string base_GuidName = AssetDatabase.AssetPathToGUID(info.realPackPath) + _AbExtName;
                if (!_renewalFileList.ContainsKey(base_GuidName))
                {
                    RenewalFileInfoTmp renewal = new RenewalFileInfoTmp();
                    _renewalFileList[base_GuidName] = renewal;
                }
                _renewalFileList[base_GuidName].SetData(info.realPackPath, base_GuidName);
            }
        }

        //加载总的manifest文件
        AssetBundle ab = null;
        if (string.IsNullOrEmpty(_TargFol))
        {
            ab = AssetBundle.LoadFromFile(outputhPath + "StreamingAssets");
        }
        else
        {
            ab = AssetBundle.LoadFromFile(outputhPath + "/" + _TargFol);
        }
            
        if (ab != null)
        {
            AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>("assetBundlemanifest");

            string[] allAssetBundles = manifest.GetAllAssetBundles();

            for (int i = 0; i < allAssetBundles.Length; ++i)
            {
                string assetbundle = allAssetBundles[i];
                if (_renewalFileList.ContainsKey(assetbundle))
                {
                    RenewalFileInfoTmp renewal = _renewalFileList[assetbundle];
                    renewal.abHash = manifest.GetAssetBundleHash(assetbundle);
                }
                else
                {
                    //因为manifest文件是递增的添加信息，所以当分步打包的时候，manifest包含有以前的包信息，而_renewalFileList只包含当前分步的包信息
                    //即会出现不包含的情况，但是是正常的
                    //但是全部打包的时候，出现非法打包路径，就不正常
                    if (isAllPack)//是否是全部打包
                    {
                        Debug.Log(packDir + "非法打包路径" + assetbundle + "，请检查");
                    }
                }
            }

            //没有对应目录，则创建
            if (!Directory.Exists(versionPath))
            {
                Directory.CreateDirectory(versionPath);
            }

            //写入ab信息
            if (isAllPack || !File.Exists(versionPath + "/" + _VersionName))
            {
                string strBulid = "";
                strBulid += BulidRule(_TargFol, "0", _TargFol);
                //生成更新列表txt
                foreach (var d in _renewalFileList)
                {
                    RenewalFileInfoTmp renewal = d.Value;
                    strBulid += BulidRule(renewal.guidName, renewal.abHash.ToString(), renewal.realPackPath);
                }
                File.WriteAllText(versionPath + "/" + _VersionName, strBulid.ToString());

                string verPath = _TargDir;
                if (!string.IsNullOrEmpty(_TargFol))
                {
                    verPath = verPath + "/" + _TargFol;
                }
                File.WriteAllText(verPath + "/" + _VersionName, strBulid.ToString());
            }
            else
            {
                //不是全部打包且不存在文件信息，则追加信息
                Dictionary<string, RenewalFileInfoTmp> tmpDict = new Dictionary<string, RenewalFileInfoTmp>();
                foreach (var d in _renewalFileList)
                {
                    RenewalFileInfoTmp renewal = d.Value;

                    tmpDict[renewal.realPackPath] = renewal;
                }

                //读取原来的信息
                string strBulid = "";
                StreamReader sr = new StreamReader(versionPath + "/" + _VersionName, Encoding.UTF8);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] abInfo = line.Split(_ConfigRule2.ToCharArray());
                    string[] contrasts = abInfo[abInfo.Length - 1].Split(_ConfigRule1.ToCharArray());
                    string contrast = contrasts[contrasts.Length - 1];

                    if (tmpDict.ContainsKey(contrast))
                    {
                        RenewalFileInfoTmp renewal = tmpDict[contrast];
                        strBulid += BulidRule(renewal.guidName, renewal.abHash.ToString(), renewal.realPackPath);
                        tmpDict.Remove(contrast);
                    }
                    else
                    {
                        strBulid += line + Environment.NewLine;
                    }
                }
                sr.Close();

                foreach (var d in tmpDict)
                {
                    RenewalFileInfoTmp renewal = d.Value;
                    strBulid += BulidRule(renewal.guidName, renewal.abHash.ToString(), renewal.realPackPath);
                }

                File.WriteAllText(versionPath + "/" + _VersionName, strBulid.ToString());

                string verPath = _TargDir;
                if (!string.IsNullOrEmpty(_TargFol))
                {
                    verPath = verPath + "/" + _TargFol;
                }
                File.WriteAllText(verPath + "/" + _VersionName, strBulid.ToString());


            }

            //释放内存
            ab.Unload(true);

        }
        else
        {
            Debug.Log("没有assetBundleManifest文件");
        }

        if (_isDeleteManifest == "1")
        {
            //删除AB.Manifest
            string[] ab_files = Directory.GetFiles(outputhPath, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < ab_files.Length; ++i)
            {
                string path = ab_files[i].Replace(@"\", @"/");
                if (File.Exists(path))
                {
                    if (Path.GetExtension(path) == ".manifest" || path.Contains("manifest.meta"))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        Debug.Log("生成更新文件成功");

        //生成版本号文件
        File.WriteAllText(_VersionTimePath + "/" + _VersionTimeName, _Version);

        //压缩
        //......

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 生成对应的格式文件
    /// </summary>
    /// <returns></returns>
    private static string BulidRule(string guiName, string abHash,string path)
    {
        if (_IsExt == "0")
        {
            string ext = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(ext))
            {
                path = path.Replace(ext, "");
            }
        }

        //Debug.Log("vvv "+path);
        path = path.Replace("Assets/Resources/", "");
        //Debug.Log("vvv " + path);
        
        string strBulid = "";
        strBulid += _ConfigRule0[0] + _ConfigRule1 + guiName + _ConfigRule2 +
                    _ConfigRule0[1] + _ConfigRule1 + abHash + _ConfigRule2 +
                    _ConfigRule0[2] + _ConfigRule1 + path + Environment.NewLine;

        return strBulid;
    }

    /// <summary>
    /// 散图映射打包
    /// </summary>
    /// <param name="merger_source"></param>
    /// <param name="merger_Mapping"></param>
    private static void SetSource2AtlasPrefab(string merger_atlasPrefab, string merger_MappingSource)
    {
        string[] atlasPrefabDircs = System.IO.Directory.GetDirectories(merger_atlasPrefab, "*", SearchOption.AllDirectories);

        for (int i = 0; i < atlasPrefabDircs.Length; ++i)
        {
            string[] atlasPrefabs = System.IO.Directory.GetFiles(atlasPrefabDircs[i], "*.*", SearchOption.AllDirectories);

            for (int j = 0; j < atlasPrefabs.Length; ++j)
            {
                string atlasPrefabPath = atlasPrefabs[j];
                if (Path.GetExtension(atlasPrefabPath) == ".prefab")
                {
                    string guid = AssetDatabase.AssetPathToGUID("Assets" + atlasPrefabPath.Replace(Application.dataPath, ""));

                    string[] dirName = atlasPrefabs[j].Replace(@"\", @"/").Split('/');
                    string lastFolderName = dirName[dirName.Length - 2];

                    //设置每个纹理的AB名称
                    string[] textureFiles = System.IO.Directory.GetFiles(merger_MappingSource + "/" + lastFolderName, "*.*", SearchOption.AllDirectories);
                    for (int k = 0; k < textureFiles.Length; ++k)
                    {
                        string textureFile = textureFiles[k];
                        if (!IsNoPack(Path.GetExtension(textureFile)))
                        {
                            textureFile = textureFile.Replace(@"\", @"/");
                            textureFile = "Assets" + textureFile.Replace(Application.dataPath, "");
                            AssetImporter aIporter = AssetImporter.GetAtPath(textureFile);
                            if (aIporter.assetBundleName != guid + _AbExtName)
                            {
                                aIporter.assetBundleName = guid + _AbExtName;
                            }
                        }
                    }

                }
            }
        }
    }

    /// <summary>
    /// 脚本不参与打包
    /// </summary>
    /// <param name="ext"></param>
    /// <returns></returns>
    public static bool IsNoPack(string ext)
    {
        return _NoPackExt.Contains(ext);
    }

    /// <summary>
    /// 是否单独打包，如果为真，则是单独打包
    /// 例如A引用了B，B的后缀名满足条件，则A和B分离打包
    /// </summary>
    /// <param name="ext"></param>
    /// <returns></returns>
    private static bool IsOutResourceExt(string ext)
    {
        foreach (var v in _DivisionPackExt)
        {
            if (ext == "." + v)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否是文件夹打包
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool IsTypeF(string path)
    {
        foreach (var v in _FPack)
        {
            if (v!="" && path.Contains(v))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否是全部打成一个包
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool IsTypeA(string path)
    {
        foreach (var v in _APack)
        {
            if (v!="" && path.Contains(v))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否不打包
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool IsTypeE(string path)
    {
        foreach (var v in _EPack)
        {
            if (v!="" && path.Contains(v))
            {
                return true;
            }
        }
        return false;
    }

    //文件夹打包
    private static void SetAllDForF(string path)
    {
        string[] allFiles = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);

        string folderPath = ConvertPath(path);

        foreach (var filePath in allFiles)
        {
            string tmpFilePath = ConvertPath(filePath);

            if (!_allExportFileDict.ContainsKey(tmpFilePath))
            {
                _allExportFileDict[tmpFilePath] = new ExportFileInfo(tmpFilePath, true);
            }
            _allExportFileDict[tmpFilePath].isURLF = true;
            _allExportFileDict[tmpFilePath].realPackPath = folderPath;
        }
    }

    /// <summary>
    /// 将该文件夹内的所有文件，都打成一个包
    /// </summary>
    /// <param name="path"></param>
    private static void SetAllDForA(string path)
    {
        string[] allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

        foreach (var filePath in allFiles)
        {
            string tmpFilePath = ConvertPath(filePath);

            string reslutPath = GetFileABPath(tmpFilePath);

            if (!_allExportFileDict.ContainsKey(tmpFilePath))
            {
                _allExportFileDict[tmpFilePath] = new ExportFileInfo(tmpFilePath, true);
            }
            _allExportFileDict[tmpFilePath].isURLA = true;
            _allExportFileDict[tmpFilePath].URLA = reslutPath;
            _allExportFileDict[tmpFilePath].realPackPath = reslutPath;
        }
    }

    /// <summary>
    /// 获取整包的ab对应的ab路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string GetFileABPath(string path)
    {
        path = ConvertPath(path);

        string p = path;

        for (int i = 0; i < _APack.Count; ++i)
        {
            string f = _APack[i];
            if (path.Contains(f))
            {
                p = path.Substring(0, path.IndexOf(f) + f.Length);
            }
        }

        for (int i = 0; i < _FPack.Count; ++i)
        {
            string f = _FPack[i];
            if (path.Contains(f))
            {
                p = path.Substring(0, path.LastIndexOf("/"));
            }
        }

        return p;

    }

    /// <summary>
    /// 转换路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ConvertPath(string path)
    {
        string tmpPath = path.Replace(@"\\", "/");
        tmpPath = tmpPath.Replace(@"\", "/");
        int index = tmpPath.LastIndexOf("Assets/");
        if (index >= -1)
        {
            tmpPath = tmpPath.Substring(index);
        }

        return tmpPath;
    }

    /// <summary>
    /// 删除文件夹或者文件
    /// </summary>
    /// <param name="fileOrFolder"></param>
    private static void DeleteFileOrFolder(string fileOrFolder)
    {
        if (Directory.Exists(fileOrFolder))
        {
            System.IO.DirectoryInfo path = new DirectoryInfo(fileOrFolder);

            foreach (System.IO.DirectoryInfo d in path.GetDirectories())
            {
                d.Delete(true);
            }

            foreach (System.IO.FileInfo f in path.GetFiles())
            {
                f.Delete();
            }
        }
    }

    /// <summary>
    /// 清除ab文件名
    /// </summary>
    /// <param name="path"></param>
    private static void ClearAssetBundleName(string path)
    {
        Caching.CleanCache();
        try
        {
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    //后缀名
                    string ext = Path.GetExtension(path);
                    if (!BuildAB.IsNoPack(ext))
                    {
                        string filePath = BuildAB.ConvertPath(file);

                        AssetImporter importer = AssetImporter.GetAtPath(filePath);
                        if (importer)
                        {
                            if (importer.assetBundleName != "")
                            {
                                importer.assetBundleName = "";
                            }
                        }
                    }
                }

            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + ",清除AB名字出错");
        }
    }

    [MenuItem("AB工具/全部打包")]
    public static void BuildQuickAB()
    {
        InitConfig();
        BuildCore(_PackDir, _TargDir + "/" + _TargFol, _VersionPath);
    }

    [MenuItem("AB工具/变化打包")]
    public static void BuildChangeAB()
    {
        Debug.Log("待实现");
    }

    [MenuItem("AB工具/分步打包")]
    public static void BuildQuickPartAB()
    {
        BuildPartABWindow window = EditorWindow.GetWindow<BuildPartABWindow>(true, "分步打包");
        window.minSize = window.maxSize = new Vector2(610f, 310f);
        UnityEngine.Object.DontDestroyOnLoad(window);
    }

    [MenuItem("AB工具/打包到服务器")]
    public static void BuildToServer()
    {
        InitConfig();
        BuildCore(_PackDir,_ServerPath + "/" + _TargFol, _VersionServerPath);
    }

    [MenuItem("AB工具/清理沙盒缓存")]
    public static void ClearCache()
    {
        InitConfig();
        DeleteFileOrFolder(Application.persistentDataPath + "/" + _TargFol);
        Debug.Log("清理缓存完成");

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("AB工具/清理工程AB包")]
    public static void ClearYetBuileAB()
    {
        InitConfig();
        DeleteFileOrFolder(_TargDir);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("AB工具/清理服务器AB包")]
    public static void ClearYetServerBuileAB()
    {
        InitConfig();
        DeleteFileOrFolder(_ServerPath);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("AB工具/清理AB名称")]
    public static void ClearYetABName()
    {
        ClearAssetBundleName(Application.dataPath);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("AB工具/查找AB信息")]
    public static void CheckABInfo()
    {
        CheckABInfoWindow window = EditorWindow.GetWindow<CheckABInfoWindow>(true, "查找信息");
        window.minSize = window.maxSize = new Vector2(610f, 610f);
        UnityEngine.Object.DontDestroyOnLoad(window);
    }


}
