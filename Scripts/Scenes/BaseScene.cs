using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public abstract class BaseScene : MonoBehaviourPunCallbacks
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

    void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        Managers.Scene.SetCurrentScene(this);
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
    }

    public virtual void Connect()
    {
        bool result = PhotonNetwork.ConnectUsingSettings();

        if(!result)
        {
            Managers.Scene.LoadScene(Define.Scene.Login);
        }
    }

    public abstract void Clear();
}
