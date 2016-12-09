using UnityEngine;
using System.Collections;
using System;

public class GameObjectManager : Manager
{
    public GameObject InstantiateAsync(string path)
    {
        GameObject go = null;

        assetManager.LoadFromFileAsync(path, delegate(UnityEngine.Object obj)
        {
            go = Instantiate(obj) as GameObject;
        });

        return go;

    }

    public GameObject Instantiate(string path)
    {
        UnityEngine.Object obj = assetManager.LoadFromFile(path);

        GameObject go = Instantiate(obj) as GameObject;

        return go;
    }


}
