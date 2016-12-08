using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;

public class CheckABInfoWindow : EditorWindow
{

    private List<string> _checkAbNameList = new List<string>();

    private List<string> _checkPathList = new List<string>();

    private List<string> _abNameResult = new List<string>();

    private List<string> _pathResult = new List<string>();

    private Vector2 m_scrollPos;

    private bool _isCheckOver = false;

	// Use this for initialization
	void Start () 
    {
	
	}

    public void OnGUI()
    {

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("开始查找"))
        {
            _checkAbNameList.Clear();
            _checkPathList.Clear();
            _abNameResult.Clear();
            _pathResult.Clear();

            //读取查找配置
            StreamReader sr = new StreamReader(Application.dataPath + "/Editor/BuildAssetBundleTool/CheckAbInfo.txt", Encoding.UTF8);
            string line;
            int sign = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.StartsWith("--ab"))
                    {
                        sign = 0;
                    }
                    else if (line.StartsWith("--path"))
                    {
                        sign = 1;
                    }

                    if (!line.StartsWith("--ab") && !line.StartsWith("--path"))
                    {
                        if (sign == 0)
                        {
                            _checkAbNameList.Add(line);
                        }
                        else if (sign == 1)
                        {
                            _checkPathList.Add(line);
                        }
                    }
                }
            }
            sr.Close();

            if (_checkAbNameList.Count <= 0 && _checkPathList.Count <= 0) return;


            //根据配置表查找ab名称对应的文件路径
            string[] directorys = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < directorys.Length; ++i)
            {
                string dir = directorys[i];

                dir = dir.Replace(@"\", @"/");
                dir = dir.Replace(Application.dataPath, "");
                dir = "Assets" + dir;
                string guid = AssetDatabase.AssetPathToGUID(dir);
                if (_checkAbNameList.Contains(guid))
                {
                    _abNameResult.Add(guid + "   =>找到资源路径:   " + dir);
                }
            }

            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i)
            {
                string file = files[i];
                if (Path.GetExtension(file) != ".meta")
                {
                    file = file.Replace(@"\", @"/");
                    file = file.Replace(Application.dataPath, "");
                    file = "Assets" + file;
                    string guid = AssetDatabase.AssetPathToGUID(file);
                    if (_checkAbNameList.Contains(guid))
                    {
                        _abNameResult.Add(guid + "   =>找到资源路径:   " + file);
                    }
                }
            }


            //查找根据配置表查找文件路径对应的ab名称
            files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i)
            {
                string file = files[i];
                if (Path.GetExtension(file) != ".meta")
                {
                    file = file.Replace(@"\", @"/");
                    file = file.Replace(Application.dataPath + "/", "");
                    if (_checkPathList.Contains(file))
                    {
                        file = "Assets/" + file;

                        AssetImporter aIporter = AssetImporter.GetAtPath(file);
                        if (aIporter)
                        {
                            _pathResult.Add(file + "   =>找到ab名称:   " + aIporter.assetBundleName);
                        }
                    }
                }
            }

            _isCheckOver = true;
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (_isCheckOver)
        {
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos, false, true, GUILayout.Height(560));

            GUILayout.Space(10);

            EditorGUILayout.LabelField("根据配置表查找ab名称对应的文件路径");

            foreach (var v in _abNameResult)
            {
                GUILayout.Space(10);
                GUILayout.Label(v);
            }

            GUILayout.Space(30);

            EditorGUILayout.LabelField("根据配置表查找文件路径对应的ab名称");

            foreach (var v in _pathResult)
            {
                GUILayout.Space(10);
                GUILayout.Label(v);
            }

            GUILayout.EndScrollView();

        }


    }
	
}
