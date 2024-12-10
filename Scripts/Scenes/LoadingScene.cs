using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LoadingScene : BaseScene
{
    UI_LoadingScene uls;

    PhotonViewEx PV;

    AsyncOperation aSync;

    IEnumerator CorLoadingProgress;

    protected override void Init()
    {
        Managers.Input.IsUIActive = false;

        SceneType = Define.Scene.Loading;
        Managers.Sound.Play2D("BGM/Loading", Define.Sound2D.Bgm);

        uls = Managers.UI.ShowSceneUI<UI_LoadingScene>("UI_LoadingScene");
        PV = Util.GetOrAddComponent<PhotonViewEx>(gameObject);

        aSync = Managers.Scene.AsyncLoadScene(Define.Scene.Game);
        CorLoadingProgress = LoadSceneProgress();
        StartCoroutine(CorLoadingProgress);
    }

    IEnumerator LoadSceneProgress()
    {
        float timer = 0;
        while (true)
        {
            yield return null;

            if (aSync.progress < 0.9f)
            {
                uls.Get<Text>((int)UI_LoadingScene.Texts.LoadingPercentText).text = $"{(int)(aSync.progress * 100)}%";
            }
            else
            {
                timer += (Time.unscaledDeltaTime / 2);
                uls.Get<Text>((int)UI_LoadingScene.Texts.LoadingPercentText).text = $"{(int)(Mathf.Lerp(0.9f, 1f, timer) * 100)}%";
                if (timer >= 1f)
                    break;
            }
        }

        Managers.Sound.Play2D("SFX/LoadCompleted", Define.Sound2D.Effect2D);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Load", true } });

        PV.RPC("LoadOkayCount", RpcTarget.AllBuffered);
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
        uls.Get<Text>((int)UI_LoadingScene.Texts.LoadingCompletePlayerText).text = $"로딩 완료된 플레이어 <color=yellow>{loadCompletedCounter}</color>/{PhotonNetwork.CurrentRoom.PlayerCount}";
        return loadCompletedCounter;
    }

    IEnumerator CheckReadyPlayer()
    {
        int roopCount = 0;
        int maxCount = 20;
        int count;
        while (true)
        {
            yield return new WaitForSeconds(3f);
            count = LoadOkayCount();
            if (count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                yield return new WaitForSeconds(3f);
                PV.RPC("SceneActivation", RpcTarget.AllViaServer);
                yield break;
            }
            else
            {
                roopCount++;
                if(roopCount == maxCount)
                {
                    yield return new WaitForSeconds(3f);
                    PV.RPC("SceneActivation", RpcTarget.AllViaServer);
                    yield break;
                }
            }
        }
    }

    [PunRPC]
    void SceneActivation()
    {
        Managers.UI.CloseAllPopupUI();
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Load"] == true)
        {
            Managers.Clear();
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Load", false } });
            aSync.allowSceneActivation = true;
        }
        else
        {
            if (CorLoadingProgress != null)
                StopCoroutine(CorLoadingProgress);
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
            StartCoroutine(CheckReadyPlayer());
      
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LoadOkayCount();
    }

    public override void OnLeftRoom()
    {
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }

    public override void Clear()
    {
        
    }
}
