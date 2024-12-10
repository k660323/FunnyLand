using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    #region Pool
    class Pool
    {
        object _lock = new object();
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }

        Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
        {
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (int i = 0; i < count; i++)
                Push(Create());
        }

        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            poolable.transform.parent = Root;
            poolable.IsUsing = false;
            poolable.gameObject.SetActive(false);

            _poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.gameObject.SetActive(true);

            if (parent == null)
            {
                if (Managers.Scene.CurrentCScene != null)
                    poolable.transform.SetParent(Managers.Scene.CurrentCScene.transform);
                else
                    poolable.transform.SetParent(Managers.Scene.CurrentScene.transform);
            }
            else
            {
                poolable.transform.SetParent(parent);
            }

            poolable.IsUsing = true;

            return poolable;
        }
    }
    #endregion

    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    Transform _root;

    #region PoolRpc
    class PoolRpc
    {
        public string Path;

        object _lock = new object();
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }

        public Stack<Poolable> _poolStack = new Stack<Poolable>();

        public void Init(string path, GameObject original)
        {
            Path = path;
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";
            Push(Create());
        }

        Poolable Create()
        {
            GameObject go;
            go = PhotonNetwork.Instantiate(Path, Vector3.zero, Quaternion.identity);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            poolable.TryGetComponent(out PhotonViewEx PV);
            if (PV == null)
                return;

            poolable.transform.SetParent(Root);
            PV.RPC("PoolPushRpcInit", RpcTarget.AllViaServer);

            _poolStack.Push(poolable);
        }

        public Poolable Pop(Vector3 pos, Quaternion quter, Vector3 scale, string parent)
        {
            Poolable poolable;
        
            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();
           
            poolable.TryGetComponent(out PhotonViewEx PV);
            if (PV == null)
                return null;

            PV.RPC("RpcTranslate", RpcTarget.AllBuffered, pos, quter, scale);
            PV.RPC("PoolPopRpcInit", RpcTarget.AllBuffered, parent);

            return poolable;
        }
    }
    #endregion

    Dictionary<string, PoolRpc> _poolRpc = new Dictionary<string, PoolRpc>();
    public Transform _rootRpc { get; private set; }

    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            Object.DontDestroyOnLoad(_root);
        }
        if (_rootRpc == null)
        {
            _rootRpc = new GameObject { name = "@Pool_RootRpc" }.transform;
            Object.DontDestroyOnLoad(_rootRpc);
        }
    }

    #region localObjectPool
    public void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.parent = _root;

        _pool.Add(original.name, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }
        _pool[name].Push(poolable);
    }

    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
            CreatePool(original);
        return _pool[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;

        return _pool[name].Original;
    }

    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);
        _pool.Clear();
    }

    #endregion

    #region NetworkObjectPool
    public void CreatePoolRpc(string path, GameObject original)
    {
        PoolRpc poolRpc = new PoolRpc();
        poolRpc.Init(path, original);
        poolRpc.Root.parent = _rootRpc;

        _poolRpc.Add(original.name, poolRpc);
    }

    public void PushRpc(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_poolRpc.ContainsKey(name) == false)
        {
            PhotonNetwork.Destroy(poolable.gameObject);
            return;
        }
        _poolRpc[name].Push(poolable);
    }

    public Poolable PopRpc(string path, Vector3 pos, Quaternion quater,Vector3 scale, string parent, GameObject original)
    {
        if (_poolRpc.ContainsKey(original.name) == false)
            CreatePoolRpc(path, original);
        var temp = _poolRpc[original.name].Pop(pos, quater, scale, parent);
        return temp;
    }

    public GameObject GetOriginalRpc(string name)
    {
        if (_poolRpc.ContainsKey(name) == false)
            return null;

        return _poolRpc[name].Original;
    }

    public void ClearRpc()
    {
        foreach (Transform child in _rootRpc)
        {
            if (!child.GetComponent<PhotonViewEx>())
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        _poolRpc.Clear();
    }
    #endregion
}
