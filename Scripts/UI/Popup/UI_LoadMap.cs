using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UI_LoadMap : UI_Photon
{
    GameScene GS;
    AsyncOperation aSync;
    PhotonViewEx PV;

    IEnumerator CorLoadingProgress;

    enum Texts
    {
        MapNameText,
        TypeText,
        CommentText
    }

    enum Images
    {
        MapImage
    }

    public void InitV2()
    {
        GS = Managers.Game.gameScene;
        PV = GetComponent<PhotonViewEx>();
        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        gameObject.SetActive(false);
    }

    public void Loading()
    {
        Get<Text>((int)Texts.MapNameText).text = GS.curMap.name;
        Get<Text>((int)Texts.TypeText).text = GS.curMap.type;
        Get<Text>((int)Texts.CommentText).text = GS.curMap.comment;
        Get<Image>((int)Images.MapImage).sprite = Managers.Resource.Load<Sprite>($"Textures/MapImage/{GS.curMap.image}");
        gameObject.SetActive(true);
        aSync = Managers.Scene.AsyncLoadScene(GS.curMap.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        CorLoadingProgress = LoadSceneProgress();
        StartCoroutine(CorLoadingProgress);
    }

    IEnumerator LoadSceneProgress()
    {
        while (aSync.progress < 0.9f)
        {
            yield return null;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Load", true } });
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CheckReadyPlayer());
    }

    [PunRPC]
    int LoadOkayCount()
    {
        int loadCompletedCounter = 0;
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            if ((bool)PhotonNetwork.PlayerList[i].CustomProperties["Load"] == true)
            {
                loadCompletedCounter++;
            }
        }
        return loadCompletedCounter;
    }

    IEnumerator CheckReadyPlayer()
    {
        int roopCount = 0;
        int maxCount = 60;
        int count;

        yield return new WaitForSeconds(1f);

        while (true)
        {
            count = LoadOkayCount();

            if (count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                PV.RPC("SceneActivation",RpcTarget.AllBufferedViaServer);
                yield break;
            }
            else
            {
                roopCount++;
                if (roopCount == maxCount)
                {
                    PV.RPC("SceneActivation", RpcTarget.AllBufferedViaServer);
                    yield break;
                }       
            }
            yield return new WaitForSeconds(1f);
        }
    }

    [PunRPC]
    void SceneActivation()
    {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Load"] == true)
        {
            GS.StateController((int)Define.GameState.Game);
            aSync.allowSceneActivation = true;
        }
        else
        {
            if (CorLoadingProgress != null)
                StopCoroutine(CorLoadingProgress);
            PhotonNetwork.LeaveRoom();
        }
        gameObject.SetActive(false);
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            if (GS.state == Define.GameState.Loading)
                StartCoroutine(CheckReadyPlayer());
        }
    }

    public override void OnLeftRoom()
    {
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}
