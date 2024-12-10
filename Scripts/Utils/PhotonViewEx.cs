using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonViewEx : PhotonView
{
    [HideInInspector]
    public Animator anim;

    [PunRPC]
    public void PoolPushRpcInit()
    {
        if (!IsMine)
            transform.SetParent(Managers.Pool._rootRpc);
        RpcActive(false);
    }
    
    [PunRPC]
    public void PoolPopRpcInit(string parent)
    {
        RpcParent(parent);
        RpcActive(true);
    }

    [PunRPC]
    public void RpcInit(string parent, string name, Vector3 scale)
    {
        RpcParent(parent);
        gameObject.name = name;
        transform.localScale = scale;
    }

    [PunRPC]
    public void RpcTranslate(Vector3 pos, Quaternion quaternion, Vector3 scale)
    {
        gameObject.transform.position = pos;
        gameObject.transform.rotation = quaternion;
        gameObject.transform.localScale = scale;
    }

    [PunRPC]
    public void RpcParent(string parent)
    {
        if (parent != "")
        {
            transform.SetParent(GameObject.Find(parent).transform);
            transform.SetAsLastSibling();
        }
        else
        {
            transform.parent = Managers.Scene.CurrentScene.transform;
        }
    }

    [PunRPC]
    public void RpcActive(bool isUsing)
    {
        gameObject.SetActive(isUsing);
    }

    [PunRPC]
    public void RpcPos(Vector3 pos)
    {
        gameObject.transform.position = pos;
    }

    [PunRPC]
    public void RpcRotaion(Quaternion quaternion)
    {
        gameObject.transform.rotation = quaternion;
    }

    [PunRPC]
    public void RpcScale(Vector3 scale)
    {
        gameObject.transform.localScale = scale;
    }

    [PunRPC]
    public void FloatAnim(string name, float index)
    {
        if (anim == null)
            TryGetComponent(out anim);

        anim.SetFloat(name, index);
    }


    [PunRPC]
    public void TriggerAnim(string name)
    {
        if (anim == null)
            TryGetComponent(out anim);

        anim.SetTrigger(name);
    }

    [PunRPC]
    public void Rpc2DSound(string path, int type, float pitch)
    {
        Managers.Sound.Play2D(path, (Define.Sound2D)type, pitch);
    }

    public void Rpc2DTeamSound(string path, int type, float pitch, string path1, int type1, float pitch1)
    {
        // ∫ª¿Œ or ∆¿ [0]
        // ªÛ¥Î [1]

        if (Managers.Game.isTeamMode)
        {
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties["Team"].ToString() == Owner.CustomProperties["Team"].ToString())
                    RPC("Rpc2DSound", p, path, type, pitch);
                else
                    RPC("Rpc2DSound", p, path1, type1, pitch1);
            }
        }
        else
        {
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p == PhotonNetwork.LocalPlayer)
                    RPC("Rpc2DSound", p, path, type, pitch);
                else
                    RPC("Rpc2DSound", p, path1, type1, pitch1);
            }
        }
    }

    [PunRPC]
    public void Rpc3DSound(string path, Vector3 pos, string parent, int type, float pitch, float minDistance, float maxDistance)
    {
        minDistance = Mathf.Clamp(minDistance, 0f, 1000000f);
        maxDistance = Mathf.Clamp(maxDistance, 0f, 1000000f);
        Managers.Sound.Play3D(path, pos, parent, (Define.Sound3D)type, pitch, minDistance, maxDistance);
    }

    [PunRPC]
    public void NoSyncInstantiate(string path, string parent, Vector3 pos , Quaternion roation)
    {
        GameObject go;
        if (parent == "")
        {
            go = Managers.Resource.Instantiate(path);
        }
        else
        {
            go = Managers.Resource.Instantiate(path, GameObject.Find(parent).transform);
        }
        go.transform.position = transform.position + pos;
        go.transform.rotation = roation;
    }

    [PunRPC]
    public void CrossFadeAnim(string stateName)
    {
        anim.CrossFade(stateName, 0.1f, -1, 0);
    }
}
