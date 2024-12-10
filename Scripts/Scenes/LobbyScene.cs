using BackEnd;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    UI_LobbyScene uls;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;
        Managers.Sound.Play2D("BGM/I Really Want to Stay at Your House", Define.Sound2D.Bgm);

        uls = Managers.UI.ShowSceneUI<UI_LobbyScene>("UI_LobbyScene");

        if (uls.isCreateNick())
        {
            if (!PhotonNetwork.IsConnected)
            {
                Connect();
            }
        }
    }

    public override void Clear()
    {
    
    }

    public override void OnConnectedToMaster()
    {
        uls.UpdateSyncUI?.Invoke();
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        uls.Get<GameObject>((int)UI_LobbyScene.GameObjects.Connecting).SetActive(false);
        uls.SetOfficialNotice();
        Managers.Photon.InitPlayerProperties();
    }
}
