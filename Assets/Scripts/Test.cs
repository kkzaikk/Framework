using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Test : MonoBehaviour 
{

    public GameObject testObj;


	// Use this for initialization
	void Start () {

        this.GetComponent<Button>().onClick.AddListener(delegate()
        {
            this.OnClick(this.gameObject);
        });

        //AssetManager mgr = testObj.GetComponent<AssetManager>();
        //mgr.Initialize("StreamingAssets", initOK);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void initOK()
    {
        Debug.Log("开始加载");
    }

    void OnClick(GameObject go)
    {
        Debug.Log("xxxxxxxxxxxxxxxx");

        GameObjectManager mgr = AppFacade.Instance.GetManager<GameObjectManager>(ManagerName.Object);
        mgr.Instantiate("UI/test1");

        //mgr.InstantiateAsync("UI/test1", delegate(GameObject obj)
        //{
        //    obj.name = "啥地方";
        //});




    }

    int k = 0;

    GameObject obj = null;

    void Back(UnityEngine.Object go)
    {
        //TextAsset txt = go as TextAsset;
        //Debug.Log("back : " + txt.text);

        Debug.Log("实例哈");
        obj = GameObject.Instantiate(go) as GameObject;

        if (k == 0)
        {
            AssetManager mgr = testObj.GetComponent<AssetManager>();
            mgr.LoadFromFileAsync("UI/test1", Back);
            k++;
        }
    }



}
