using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;
    static Managers Instance { get { Init(); return _instance; } }

    #region Contents
    ObjectManager _obj = new ObjectManager();
    NetworkManager _network = new NetworkManager();

    public static ObjectManager Object { get { return Instance._obj; } }
    public static NetworkManager Network { get { return Instance._network; } }
	#endregion

	#region Core
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();

    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
	#endregion

	void Start()
    {
        Init();
	}

    void Update()
    {
        _network.Update();
    }

    static void Init()
    {
        if (_instance == null)
        {
			GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();

            _instance._network.Init();
            _instance._pool.Init();
            _instance._obj.Init();
        }		
	}

    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }
}
