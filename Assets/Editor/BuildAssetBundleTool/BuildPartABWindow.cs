using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BuildPartABWindow : EditorWindow
{
    //选择需要打包的路径
    private string _defaultPath = Application.dataPath + "/Resources";

    ////需要打包的文件
    //private static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

	// Use this for initialization
	void Start () {
	
	}

    public void OnGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("选择路径"))
        {
            _defaultPath = EditorUtility.OpenFolderPanel("选择路径", "", "");
            _defaultPath = _defaultPath.Replace(Application.dataPath + "/", "");
        }

        //输入框控件
        GUILayout.TextField(_defaultPath);

        GUILayout.EndHorizontal();

        for (int i = 0; i < 4; ++i)
        {
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("开始打包"))
        {
            BuildAB.InitConfig();

            BuildAB.BuildPartCore("Assets/"+ _defaultPath);
        }


        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

}
