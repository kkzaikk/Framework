#if ASYNC_MODE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class AssetBundleInfo
{
    public AssetBundle m_AssetBundle;
    public int m_ReferencedCount;

    public AssetBundleInfo(AssetBundle assetBundle)
    {
        m_AssetBundle = assetBundle;
        m_ReferencedCount = 0;
    }
}

public class AssetManager : MonoBehaviour
{
    /// <summary>
    /// 根据每个平台不同，返回沙盒路径
    /// </summary>
    string m_BaseDownloadingURL = "";

    /// <summary>
    /// 根据manifest存储的所有AB包
    /// </summary>
    string[] m_AllManifest = null;

    /// <summary>
    /// ab总配置文件
    /// </summary>
    AssetBundleManifest m_AssetBundleManifest = null;

    /// <summary>
    /// ab依赖
    /// </summary>
    Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

    /// <summary>
    /// 已经加载的ab文件字典
    /// </summary>
    Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();

    /// <summary>
    /// 加载请求队列
    /// </summary>
    Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();

    /// <summary>
    /// 从服务器下载到沙盒文件，然后读取都字典，最新的文件列表,整个工程只有一个
    /// 路径名，映射到文本文件
    /// </summary>
    Dictionary<string, RefreshFileInfo> m_refreshFileDict = new Dictionary<string, RefreshFileInfo>();

    class LoadAssetRequest
    {
        public Type assetType;
        public string assetName;
        public Action<UnityEngine.Object> sharpFunc;
    }

    // Load AssetBundleManifest.
    public void Initialize(string manifestName, Action initOK)
    {
        m_BaseDownloadingURL = PathConstData.GetRelativePath();
        AddLoadRequestAsync<AssetBundleManifest>(manifestName, "AssetBundleManifest", delegate(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                m_AssetBundleManifest = obj as AssetBundleManifest;
                m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
            }
            else
            {
                Debug.LogError("未加载manifest文件");
            }

            //读取沙盒的更新文件
            ReadRefreshRText();

            if (initOK != null) initOK();
        });
    }

    /// <summary>
    /// 读取沙盒的更新文件
    /// </summary>
    private void ReadRefreshRText()
    {
        //读取最新的更新文件
        Debug.Log("沙盒路径:" + PathConstData.DataPath  + PathConstData.VersionName);
        string text = File.ReadAllText(PathConstData.DataPath + PathConstData.VersionName);

        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                using (StringReader sr = new StringReader(text))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        RefreshFileInfo info = new RefreshFileInfo();
                        string[] contents = line.Trim().Split(';');
                        string[] guidName = contents[0].Split(':');
                        string[] abHash = contents[1].Split(':');
                        string[] path = contents[2].Split(':');
                        info.SetData(path[1], guidName[1],Hash128.Parse(abHash[1]));
                        //Debug.Log("测试" + path[1]+"   "+guidName[1]+"   "+Hash128.Parse(abHash[1]));
                        m_refreshFileDict[path[1]] = info;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    /// <summary>
    /// 判断文件是否在沙盒
    /// </summary>
    private bool IsExistsPersistent(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            return File.Exists(path);
        }

        Debug.LogError("传入路径有错====》" + path);
        return false;
    }

    /// <summary>
    /// 异步加载本地AB资源
    /// </summary>
    /// <param name="manifest"></param>
    /// <param name="pathName"></param>
    /// <param name="assetType"></param>
    /// <param name="action"></param>
    public void LoadFromFileAsync(string path, Action<UnityEngine.Object> action)
    {
        string ext = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(ext))
        {
            path.Replace(ext, "");
        }

        bool isSingle = IsSingleFile(path);//是否是单个文件加载（否：合并加载）

        if (isSingle)
        {
            Debug.Log("单个加载");

            RefreshFileInfo fileInfo;
            if (!IsAtRefreshFileDict(path, out fileInfo))
            {
                if (action != null)
                {
                    action(null);
                }
                return;
            }

            //加载
            AddLoadRequestAsync<UnityEngine.Object>(fileInfo.guiName, fileInfo.path, action);
        }
        else
        {
            //获取整包的ab对应的ab路径(如"Assets/Resources/LuaScript")
            string mainPath = GetFileABPath(path);
            //合体AB包里面的单个欲加载的名称（如"GameLuaMain"）
            string singleName = GetFileABName(path);

            RefreshFileInfo fileInfo;
            if (!IsAtRefreshFileDict(mainPath,out fileInfo))
            {
                if (action != null)
                {
                    action(null);
                }
                return;
            }
            
            //加载
            AddLoadRequestAsync<UnityEngine.Object>(fileInfo.guiName, singleName, action);
        }
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadFromFile(string path)
    {
        string ext = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(ext))
        {
            path.Replace(ext, "");
        }

        bool isSingle = IsSingleFile(path);//是否是单个文件加载（否：合并加载）

        if (isSingle)
        {
            RefreshFileInfo fileInfo;
            if (!IsAtRefreshFileDict(path, out fileInfo))
            {
                return null;
            }

            //加载
            return LoadAsset<UnityEngine.Object>(fileInfo.guiName, fileInfo.path);
            //AddLoadRequest<UnityEngine.Object>(fileInfo.guiName, fileInfo.path, action);
        }
        else
        {
            //获取整包的ab对应的ab路径(如"Assets/Resources/LuaScript")
            string mainPath = GetFileABPath(path);
            //合体AB包里面的单个欲加载的名称（如"GameLuaMain"）
            string singleName = GetFileABName(path);

            RefreshFileInfo fileInfo;
            if (!IsAtRefreshFileDict(mainPath, out fileInfo))
            {
                return null;
            }

            //加载
            //AddLoadRequest<UnityEngine.Object>(fileInfo.guiName, singleName, action);
            return LoadAsset<UnityEngine.Object>(fileInfo.guiName, singleName);
        }
    }



    /// <summary>
    /// 路径path是否在更新文件字典中
    /// </summary>
    /// <param name="path"></param>
    private bool IsAtRefreshFileDict(string path, out RefreshFileInfo fileInfo)
    {
        if (!m_refreshFileDict.TryGetValue(path, out fileInfo))
        {
            Debug.LogError("从路径:  " + path + "  ,没有找到对应的配置文件信息");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 是否是单个文件，还是合成一个AB包里面的某个文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool IsSingleFile(string path)
    {
        path = path.Replace("\\", "/");

        for (int i = 0; i < PathConstData.aFolders.Length; ++i)
        {
            string f = PathConstData.aFolders[i];
            if (path.Contains(f))
            {
                return false;
            }
        }

        for (int i = 0; i < PathConstData.fFolders.Length; ++i)
        {
            string f = PathConstData.fFolders[i];
            if (path.Contains(f))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 获取整包的ab对应的ab路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string GetFileABPath(string path)
    {
        path = ConvertPath(path);

        string p = path;

        for (int i = 0; i < PathConstData.aFolders.Length; ++i)
        {
            string f = PathConstData.aFolders[i];
            if (path.Contains(f))
            {
                p = path.Substring(0, path.IndexOf(f) + f.Length);
            }
        }

        for (int i = 0; i < PathConstData.fFolders.Length; ++i)
        {
            string f = PathConstData.fFolders[i];
            if (path.Contains(f))
            {
                p = path.Substring(0, path.LastIndexOf("/"));
            }
        }

        return p;

    }

    /// <summary>
    /// 获取合体AB包的下单个的资源名称
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFileABName(string path)
    {
        path = path.Replace("\\", "/");

        string p = path;

        for (int i = 0; i < PathConstData.aFolders.Length; ++i)
        {
            string f = PathConstData.aFolders[i];
            if (path.Contains(f))
            {
                string pathName = Path.GetFileName(path);
                string pathExt = Path.GetExtension(path);
                if (!string.IsNullOrEmpty(pathExt))
                {
                    p = pathName.Replace(pathExt, "");
                }
                return p;
            }
        }

        for (int i = 0; i < PathConstData.aFolders.Length; ++i)
        {
            string f = PathConstData.fFolders[i];
            if (path.Contains(f))
            {
                string pathName = Path.GetFileName(path);
                string pathExt = Path.GetExtension(path);
                if (!string.IsNullOrEmpty(pathExt))
                {
                    p = pathName.Replace(pathExt, "");
                }
                return p;
            }
        }

        return p;
    }

    /// <summary>
    /// 转换路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string ConvertPath(string path)
    {
        string tmpPath = path.Replace(@"\\", "/");
        tmpPath = tmpPath.Replace(@"\", "/");
        return tmpPath;
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 载入素材
    /// </summary>
    public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object
    {
        abname = abname.ToLower();
        AssetBundleInfo bundleInfo = LoadAssetBundle(abname);
        return bundleInfo.m_AssetBundle.LoadAsset<T>(Path.GetFileName(assetname));
    }


    /// <summary>
    /// 载入AssetBundle
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public AssetBundleInfo LoadAssetBundle(string abname)
    {
        if (!abname.EndsWith(PathConstData.ExtName))
        {
            abname += PathConstData.ExtName;
        }
        AssetBundleInfo bundle = null;
        if (!m_LoadedAssetBundles.ContainsKey(abname))
        {
            byte[] stream = null;
            string uri = PathConstData.DataPath + abname;
            Debug.LogWarning("LoadFile::>> " + uri);
            LoadDependencies(abname);

            //stream = File.ReadAllBytes(uri);

            Debug.Log(stream);

            bundle = new AssetBundleInfo(AssetBundle.LoadFromFile(uri)); //关联数据的素材绑定   //AssetBundle.LoadFromMemory(stream)
            m_LoadedAssetBundles.Add(abname, bundle);
        }
        else
        {
            m_LoadedAssetBundles.TryGetValue(abname, out bundle);
        }
        return bundle;
    }

    /// <summary>
    /// 载入依赖
    /// </summary>
    /// <param name="name"></param>
    void LoadDependencies(string name)
    {
        if (m_AssetBundleManifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            return;
        }
        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = m_AssetBundleManifest.GetAllDependencies(name);
        if (dependencies.Length == 0) return;

        // Record and load all dependencies.
        for (int i = 0; i < dependencies.Length; i++)
        {
            LoadAssetBundle(dependencies[i]);
        }
    }


    string GetRealAssetPath(string abName)
    {
        if (abName.Equals(PathConstData.AssetDir))
        {
            return abName;
        }
        abName = abName.ToLower();

        if (!abName.EndsWith(PathConstData.ExtName))
        {
            abName += PathConstData.ExtName;
        }
        if (abName.Contains("/"))
        {
            return abName;
        }

        //string[] paths = m_AssetBundleManifest.GetAllAssetBundles();  产生GC，需要缓存结果
        for (int i = 0; i < m_AllManifest.Length; i++)
        {
            int index = m_AllManifest[i].LastIndexOf('/');
            string path = m_AllManifest[i].Remove(0, index + 1);    //字符串操作函数都会产生GC
            if (path.Equals(abName))
            {
                return m_AllManifest[i];
            }
        }
        Debug.LogError("GetRealAssetPath Error:>>" + abName);
        return null;
    }

    /// <summary>
    /// 载入素材请求
    /// </summary>
    void AddLoadRequestAsync<T>(string abName, string assetName, Action<UnityEngine.Object> action = null) where T : UnityEngine.Object
    {
        abName = GetRealAssetPath(abName);

        LoadAssetRequest request = new LoadAssetRequest();
        request.assetType = typeof(T);
        request.assetName = assetName;
        request.sharpFunc = action;

        List<LoadAssetRequest> requests = null;
        if (!m_LoadRequests.TryGetValue(abName, out requests))
        {
            requests = new List<LoadAssetRequest>();
            requests.Add(request);
            m_LoadRequests.Add(abName, requests);
            StartCoroutine(OnLoadAssetBundleAsync<T>(abName));
        }
        else
        {
            requests.Add(request);
        }
    }

    IEnumerator OnLoadAssetBundleAsync<T>(string abName) where T : UnityEngine.Object
    {
        AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
        if (bundleInfo == null)
        {
            yield return StartCoroutine(LoadDependenciesAsync(abName, typeof(T)));

            bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null)
            {
                m_LoadRequests.Remove(abName);
                Debug.LogError("OnLoadAsset--->>>" + abName);
                yield break;
            }
        }

        List<LoadAssetRequest> list = null;
        if (!m_LoadRequests.TryGetValue(abName, out list))
        {
            m_LoadRequests.Remove(abName);
            yield break;
        }

        for (int i = 0; i < list.Count; i++)
        {
            string assetNames = list[i].assetName;
            UnityEngine.Object result = null;

            AssetBundle ab = bundleInfo.m_AssetBundle;

            string assetPath = assetNames;

            assetPath = assetPath.Replace(@"\",@"/");
            if (assetPath.Contains(@"/"))
            {
                int index = assetPath.LastIndexOf("/");
                assetPath = assetPath.Substring(index + 1, assetPath.Length - index - 1);
            }

            AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i].assetType);
            yield return request;
            result = request.asset;

            if (list[i].sharpFunc != null)
            {
                list[i].sharpFunc(result);
                list[i].sharpFunc = null;
            }

            bundleInfo.m_ReferencedCount++;
            Debug.Log(abName + "主包引用" + bundleInfo.m_ReferencedCount);

        }

        Debug.Log("移除请求队列");
        m_LoadRequests.Remove(abName);
    }

    /// <summary>
    /// 加载所有依赖后，再加载主包
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    IEnumerator LoadDependenciesAsync(string abName, Type type)
    {
        string url = m_BaseDownloadingURL + abName;

        WWW download = null;
        if (type == typeof(AssetBundleManifest))
        {
            download = new WWW(url);
        }
        else
        {
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
            if (dependencies.Length > 0)
            {
                m_Dependencies.Add(abName, dependencies);

                Debug.Log("开始加载依赖");
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string depName = dependencies[i];

                    AssetBundleInfo bundleInfo = null;

                    if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                    {
                        Debug.Log(abName + "依赖+1");
                        bundleInfo.m_ReferencedCount++;
                    }
                    else if (!m_LoadRequests.ContainsKey(depName))
                    {
                        yield return StartCoroutine(LoadDependenciesAsync(depName, type));
                    }

                }
            }

            download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
        }
        yield return download;

        AssetBundle assetObj = download.assetBundle;
        if (assetObj != null)
        {
            m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
        }
    }

    AssetBundleInfo GetLoadedAssetBundle(string abName)
    {
        AssetBundleInfo bundle = null;
        m_LoadedAssetBundles.TryGetValue(abName, out bundle);
        if (bundle == null) return null;

        // No dependencies are recorded, only the bundle itself is required.
        string[] dependencies = null;
        if (!m_Dependencies.TryGetValue(abName, out dependencies))
            return bundle;

        // Make sure all dependencies are loaded
        foreach (var dependency in dependencies)
        {
            AssetBundleInfo dependentBundle;
            m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
            if (dependentBundle == null) return null;
        }
        return bundle;
    }

    /// <summary>
    /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="isThorough"></param>
    public void UnloadAssetBundle(string path, bool isThorough = false)
    {
        if(!m_refreshFileDict.ContainsKey(path))
        {
            return;
        }

        string abName = m_refreshFileDict[path].guiName;


        abName = GetRealAssetPath(abName);
        Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);

        Debug.Log("释放主包");
        UnloadAssetBundleInternal(abName, isThorough);

        Debug.Log("释放依赖");
        UnloadDependencies(abName, isThorough);
        Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
    }

    void UnloadDependencies(string abName, bool isThorough)
    {
        string[] dependencies = null;
        if (!m_Dependencies.TryGetValue(abName, out dependencies))
            return;

        // Loop dependencies.
        foreach (var dependency in dependencies)
        {
            UnloadAssetBundleInternal(dependency, isThorough);
        }
        m_Dependencies.Remove(abName);
    }

    void UnloadAssetBundleInternal(string abName, bool isThorough)
    {
        AssetBundleInfo bundle = GetLoadedAssetBundle(abName);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
        if (bundle == null) return;

        Debug.Log(abName+"原来" + bundle.m_ReferencedCount);
        if (--bundle.m_ReferencedCount <= 0)
        {
            if (m_LoadRequests.ContainsKey(abName))
            {
                Debug.Log("如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可");
                return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
            }
            bundle.m_AssetBundle.Unload(isThorough);
            m_LoadedAssetBundles.Remove(abName);
            Debug.Log(abName + " has been unloaded successfully");
        }
        Debug.Log(abName+"后来" + bundle.m_ReferencedCount);

    }
}
#else

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ResourceManager : MonoBehaviour
{
    private string[] m_Variants = { };
    private AssetBundleManifest manifest;
    private AssetBundle shared, assetbundle;
    private Dictionary<string, AssetBundle> bundles;

    void Awake()
    {
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        byte[] stream = null;
        string uri = string.Empty;
        bundles = new Dictionary<string, AssetBundle>();
        uri = PathConstData.DataPath + PathConstData.AssetDir;
        if (!File.Exists(uri)) return;
        stream = File.ReadAllBytes(uri);
        assetbundle = AssetBundle.LoadFromMemory(stream);
        manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    public void LoadFromFile(string path, Action<UnityEngine.Object> action)
    {
 
    }

    /// <summary>
    /// 载入素材
    /// </summary>
    public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object
    {
        abname = abname.ToLower();
        AssetBundle bundle = LoadAssetBundle(abname);
        return bundle.LoadAsset<T>(assetname);
    }

    /// <summary>
    /// 载入AssetBundle
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public AssetBundle LoadAssetBundle(string abname)
    {
        if (!abname.EndsWith(PathConstData.ExtName))
        {
            abname += PathConstData.ExtName;
        }
        AssetBundle bundle = null;
        if (!bundles.ContainsKey(abname))
        {
            byte[] stream = null;
            string uri = PathConstData.DataPath + abname;
            Debug.LogWarning("LoadFile::>> " + uri);
            LoadDependencies(abname);

            stream = File.ReadAllBytes(uri);
            bundle = AssetBundle.LoadFromMemory(stream); //关联数据的素材绑定
            bundles.Add(abname, bundle);
        }
        else
        {
            bundles.TryGetValue(abname, out bundle);
        }
        return bundle;
    }

    /// <summary>
    /// 载入依赖
    /// </summary>
    /// <param name="name"></param>
    void LoadDependencies(string name)
    {
        if (manifest == null)
        {
            Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
            return;
        }
        // Get dependecies from the AssetBundleManifest object..
        string[] dependencies = manifest.GetAllDependencies(name);
        if (dependencies.Length == 0) return;

        for (int i = 0; i < dependencies.Length; i++)
        {
            dependencies[i] = RemapVariantName(dependencies[i]);
        }

        // Record and load all dependencies.
        for (int i = 0; i < dependencies.Length; i++)
        {
            LoadAssetBundle(dependencies[i]);
        }
    }

    // Remaps the asset bundle name to the best fitting asset bundle variant.
    string RemapVariantName(string assetBundleName)
    {
        string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

        // If the asset bundle doesn't have variant, simply return.
        if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
            return assetBundleName;

        string[] split = assetBundleName.Split('.');

        int bestFit = int.MaxValue;
        int bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for (int i = 0; i < bundlesWithVariant.Length; i++)
        {
            string[] curSplit = bundlesWithVariant[i].Split('.');
            if (curSplit[0] != split[0])
                continue;

            int found = System.Array.IndexOf(m_Variants, curSplit[1]);
            if (found != -1 && found < bestFit)
            {
                bestFit = found;
                bestFitIndex = i;
            }
        }
        if (bestFitIndex != -1)
            return bundlesWithVariant[bestFitIndex];
        else
            return assetBundleName;
    }

    /// <summary>
    /// 销毁资源
    /// </summary>
    void OnDestroy()
    {
        if (shared != null) shared.Unload(true);
        if (manifest != null) manifest = null;
        Debug.Log("~ResourceManager was destroy!");
    }
}
#endif
