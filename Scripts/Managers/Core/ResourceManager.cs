using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResourceManager
{
    Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();

    public Sprite ItemImageLoad(string path)
    {
        return Load<Sprite>($"Textures/ItemImage/{path}");
    }

    public T Load<T>(string path) where T : Object // T�� ������ �Ǵ�.
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }

        if (_resources.TryGetValue(path, out Object resource))
            return resource as T;

        T obj = Resources.Load<T>(path);
        _resources.Add(path, obj);

        return obj;
    }
    /*
    #region ��巹����
    public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        // ĳ�� Ȯ��.
        if (_resources.TryGetValue(key, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;
        if (key.Contains(".sprite"))
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";

        // ���ҽ� �񵿱� �ε� ����.
        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOperation.Completed += (op) =>
        {
            _resources.Add(key, op.Result);
            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                LoadAsync<T>(result.PrimaryKey, (obj) =>
                {
                    loadCount++;
                    callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                });
            }
        };
    }

    #endregion
    */
    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }
    

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        // ���࿡ Ǯ���� �ʿ��� ���̶�� -> Ǯ�� �Ŵ������� ��Ź
        Poolable poolable = go.GetComponent<Poolable>();
        if(poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }
        Object.Destroy(go);
    }

    public GameObject PhotonInstantiate(string path, Vector3 pos, Quaternion quater, Vector3 scale, Define.PhotonObjectType type = Define.PhotonObjectType.PlayerObject, string parent = "")
    {
        GameObject original = Managers.Resource.Load<GameObject>($"Prefabs/Photon/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }
        if (original.GetComponent<PhotonViewEx>() == null)
        {
            Debug.Log($"Failed to load PhotonViewEx : {path}");
            return null;
        }

        // Ǯ�� �̸� Ǯ���� ������
        if (original.GetComponent<Poolable>() != null && type == Define.PhotonObjectType.PlayerObject)
            return Managers.Pool.PopRpc($"Prefabs/Photon/{path}", pos, quater, scale, parent, original).gameObject;

        GameObject go;
        // Ǯ�� �ƴϸ� ���ϰ� �����ϰ� ��
        if (type == Define.PhotonObjectType.PlayerObject)
        {
            go = PhotonNetwork.Instantiate($"Prefabs/Photon/{path}", pos, quater);
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
                return null;
            go = PhotonNetwork.InstantiateRoomObject($"Prefabs/Photon/{path}", pos, quater);
        }

        go.GetComponent<PhotonViewEx>().RPC("RpcInit", RpcTarget.AllBuffered, parent, original.name, scale);

        return go;
    }

    public void PhotonDestroy(GameObject go, string name)
    {
        if (go == null)
            return;
        GameObject photonObject = Util.FindChild(go, name);
        if (photonObject != null)
            PhotonDestroy(photonObject);
    }

    public void PhotonDestroy(GameObject go)
    {
        if (go == null)
            return;
        if (!go.TryGetComponent(out PhotonViewEx pv))
            return;
        
        if(pv.ViewID < 1000)//�� ������Ʈ
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
        }
        else // �÷��̾� ������Ʈ
        {
            if (!pv.IsMine && !PhotonNetwork.IsMasterClient)
                return;

            // ���࿡ Ǯ���� �ʿ��� ���̶�� -> Ǯ�� �Ŵ������� ��Ź
            Poolable poolable = go.GetComponent<Poolable>();
            if (poolable != null)
            {
                Managers.Pool.PushRpc(poolable);
                return;
            }
        }

        PhotonNetwork.Destroy(go);
    }

}
