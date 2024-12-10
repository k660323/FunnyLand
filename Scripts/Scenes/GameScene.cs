using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System;
using System.Linq;

public class GameScene : BaseScene,IPunObservable
{
    public Define.GameState state;
    public PhotonViewEx PV { get; private set; }
    public UI_GameScene UGS { get; private set; }

    public Data.Map curMap = new Data.Map();
    int mapIndex = 0;

    public GameObject CrossHair;

    public int MapIndex
    {
        get
        {
            return mapIndex;
        }
        set
        {
            mapIndex = value;
            if (Managers.Game.isTeamMode)
            {
                if (value < Managers.Data.TeamMapDict.Count)
                    Managers.Data.TeamMapDict.TryGetValue(value, out curMap);
                else
                    Managers.Data.HybridMapDict.TryGetValue(value - Managers.Data.TeamMapDict.Count, out curMap);
            }
            else
            {
                if (value < Managers.Data.SoloMapDict.Count)
                    Managers.Data.SoloMapDict.TryGetValue(value, out curMap);
                else
                    Managers.Data.HybridMapDict.TryGetValue(value - Managers.Data.SoloMapDict.Count, out curMap);
            }
        }
    }

    public int MapLength
    {
        get
        {
            if (Managers.Game.isTeamMode)
                return Managers.Data.TeamMapDict.Count + Managers.Data.HybridMapDict.Count;
            else
                return Managers.Data.SoloMapDict.Count + Managers.Data.HybridMapDict.Count;
        }
    }

    public bool mvpLocalPlayer = false;

    public List<Player> pScoresList = new List<Player>();

    public Dictionary<Define.Team, int> totalScoreDic = new Dictionary<Define.Team, int>();

    object _lock = new object();

    int holdTime = -1;

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        lock (_lock)
        {
            Player item = pScoresList.Find(p => p == otherPlayer);
            if (item != null)
            {
                pScoresList.Remove(item);
                if(Managers.Game.isTeamMode)
                {
                    int score = int.Parse(otherPlayer.CustomProperties["Score"].ToString());
                    if (otherPlayer.CustomProperties["Team"].ToString() == "RedTeam")
                        Managers.Game.gameScene.totalScoreDic[Define.Team.RedTeam] -= score;
                    else
                        Managers.Game.gameScene.totalScoreDic[Define.Team.BlueTeam] -= score;
                }

                if (Managers.Game.isGameStart && Managers.Game.ContentsScene != null)
                    Managers.Game.ContentsScene.endEvent?.Invoke();
                UGS.USB.UpdateLeavePlayer(item);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Managers.Game.isGameStart);
            stream.SendNext(holdTime);
        }
        else
        {
            Managers.Game.isGameStart = (bool)stream.ReceiveNext();
            holdTime = (int)stream.ReceiveNext();
        }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        state = Define.GameState.None;
        PV = GetComponent<PhotonViewEx>();
        UGS = Managers.UI.ShowSceneUI<UI_GameScene>();

        Managers.Game.SetGameSceneSetting(this, (bool)PhotonNetwork.CurrentRoom.CustomProperties["Team"], (bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamKill"]);
        CrossHair = GameObject.FindGameObjectWithTag("CrossHair");
        CrossHair.SetActive(false);

        if (Managers.Game.isTeamMode)
        {
            totalScoreDic.Add(Define.Team.RedTeam, 0);
            totalScoreDic.Add(Define.Team.BlueTeam, 0);
        }

        for (int i = 0; i < Managers.Quest.ActiveQuests.Count; i++)
            Managers.Quest.ActiveQuests[i].Cancel();

        List<Player> p = PhotonNetwork.PlayerList.ToList();
        pScoresList = p;
        //gameObject.GetOrAddComponent<CursorController>();
    }

    private void Start()
    {
        StateController((int)Define.GameState.Choice);
        //if (PhotonNetwork.IsMasterClient)
        //    PV.RPC("StateController", RpcTarget.AllBufferedViaServer, (int)Define.GameState.Choice);
    }

    #region 게임 흐름
    [PunRPC]
    public void StateController(int num)
    {
        if (state == (Define.GameState)num)
            return;

        state = (Define.GameState)num;

        switch (state)
        {
            case Define.GameState.Choice:
                ChoiceState();
                break;
            case Define.GameState.ReChoice:
                ReChoiceState();
                break;
            case Define.GameState.Loading:
                LoadingState();
                break;
            case Define.GameState.Game:
                GameState();
                break;
            case Define.GameState.Result_0: // 결과 대기
                ResultState_0();
                break;
            case Define.GameState.Result_1: // 퀘스트 완료하고 대기 
                ResultState_1();
                break;
            case Define.GameState.Result_2: // 모든 점수 총합 현재 라운드 수 
                ResultState_2();
                break;
            case Define.GameState.Result_3: // 모든 점수 총합 현재 라운드 수 
                ResultState_3();
                break;
            case Define.GameState.End:
                GameEnd();
                break;
        }
    }

    void ChoiceState()
    {
        if (Camera.main != null)
            Camera.main.GetComponent<AudioListener>().enabled = true;

        Managers.Game.gameScene.SetFadeIn(0);
        Managers.Sound.Play2D("BGM/Game", Define.Sound2D.Bgm);
        Managers.Photon.InitPlayerGameProperties();
        CrossHair.SetActive(false);
        UGS.SetActiveUCM(true);
        //if (PhotonNetwork.IsMasterClient)
        //    Managers.Resource.PhotonInstantiate("UI_ChoiceMap", Vector3.zero, Quaternion.identity, Vector3.one, Define.PhotonObjectType.RoomObject, Managers.UI.Root.name);
    }

    void ReChoiceState()
    {
        Managers.Clear();
        Managers.Scene.AsyncUnLoadScene(curMap.sceneName);
        PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Choice);
    }

    void LoadingState()
    {
        UGS.SetActiveULM(true);
        //if(PhotonNetwork.IsMasterClient)
        //{
        //    Managers.Resource.PhotonDestroy(Managers.UI.Root, "UI_ChoiceMap");
        //    Managers.Resource.PhotonInstantiate("UI_LoadMap", Vector3.zero, Quaternion.identity, Vector3.one, Define.PhotonObjectType.RoomObject, Managers.UI.Root.name);
        //}
    }

    void GameState()
    {
        Managers.Sound.PauseBGM();
        if (PhotonNetwork.IsMasterClient)
            Managers.Resource.PhotonDestroy(Managers.UI.Root, "UI_LoadMap");

        SetCommentKeyText("<color=yellow>단축키</color>\n" + curMap.shortcutKeys);
        SetFadeOut(0);
    }

    void ResultState_0()
    {
        if (Managers.Game.isGameStart)
        {
            Managers.Game.isGameStart = false;
            Managers.Game.gameScene.SetFadeOut(3);
        }

        
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Timer(5, () => { PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_1); }));
    }

    void ResultState_1()
    {
        // 점수 등록
        Managers.Quest.CompleteWaitingQuests();
        Managers.Quest.CancelAllQuests();

        if (Managers.Game.rewardScore != 0)
        {
            int totalScore = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"] + Managers.Game.rewardScore;
            Managers.Photon.SetPlayerPropertie("Score", totalScore);
        }

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Timer(3, () => { PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_2); }));
    }

    void ResultState_2()
    {
        // 정렬
        lock (_lock)
        {
            pScoresList = PhotonNetwork.PlayerList.OrderByDescending(p => int.Parse(p.CustomProperties["Score"].ToString())).ToList();

            if (Managers.Game.isTeamMode)
            {
                int scoreR = 0;
                int scoreB = 0;
                for (int i = 0; i < pScoresList.Count(); i++)
                {
                    int score = int.Parse(pScoresList[i].CustomProperties["Score"].ToString());
                    if (pScoresList[i].CustomProperties["Team"].ToString() == "RedTeam")
                        scoreR += score;
                    else
                        scoreB += score;
                }

                Managers.Game.gameScene.totalScoreDic[Define.Team.RedTeam] = scoreR;
                Managers.Game.gameScene.totalScoreDic[Define.Team.BlueTeam] = scoreB;
            }

            UGS.USB.UpdateScore(ref pScoresList);
        }

        // 방장이 점수판 띄운다.
        if (PhotonNetwork.IsMasterClient)
        {
            if (Util.FindChild(Managers.UI.Root, "UI_RoundResult") == null)
                Managers.Resource.PhotonInstantiate("UI_RoundResult", Vector3.zero, Quaternion.identity, Vector3.one, Define.PhotonObjectType.RoomObject, Managers.UI.Root.name);
        }

        Managers.Clear();
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Timer(10, () => { PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_3); }));
    }

    void ResultState_3()
    {
        Managers.Scene.AsyncUnLoadScene(curMap.sceneName);
        if (PhotonNetwork.IsMasterClient)
        {
            Managers.Resource.PhotonDestroy(Managers.UI.Root, "UI_RoundResult");
            RoundCheck();
        }
    }

    void RoundCheck()
    {
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["CurRound"] >= (int)PhotonNetwork.CurrentRoom.CustomProperties["Round"])
        {
            PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.End);
        }
        else
        {
            PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Choice);
        }

        Managers.Photon.SetRoomPropertie("CurRound", (int)PhotonNetwork.CurrentRoom.CustomProperties["CurRound"] + 1);
    }

    void GameEnd()
    {
        Managers.UI.ShowPopupUI<UI_GameOver>("UI_GameOver");
    }
    #endregion

    #region UI 설정
    public string SecondsToTime(int seconds)
    {
        int hour = seconds / 3600;
        int min = seconds % 3600 / 60;
        int sec = seconds % 3600 % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
    }

    public void SetFadeOut(int index = 0)
    {
        UGS.FadeOut(index);
    }

    public void SetFadeIn(int index = 0)
    {
        UGS.FadeOut(index);
    }

    public void SetSpectatingText(string playerName)
    {
        UGS.Get<Text>((int)UI_GameScene.Texts.SpectatingText).text = playerName;
    }

    public void SetCommentKeyText(string shortcutKeys)
    {
        UGS.Get<Text>((int)UI_GameScene.Texts.ShortcutKeysText).text = shortcutKeys;
    }

    public void ClearMessage()
    {
        UGS.Get<MessageQueue>((int)UI_GameScene.MessageQueues.SignQueue).ClearMessage();
    }

    [PunRPC]
    public void RegisterMessage(string message, float showMessageTime)
    {
        UGS.Get<MessageQueue>((int)UI_GameScene.MessageQueues.SignQueue).EnqueueMessage(new Message(message, showMessageTime));
    }

    IEnumerator CorCountrotin;
    [PunRPC]
    public void Counter(int count)
    {
        if(CorCountrotin != null)
        {
            StopCoroutine(CorCountrotin);
            CorCountrotin = null;
        }

        CorCountrotin = CorCounter(count);
        StartCoroutine(CorCountrotin);
    }

    IEnumerator CorCounter(int count)
    {
        while(count > 0)
        {
            if (count == 3)
                Managers.Sound.Play2D("SFX/CountDown");
            UGS.Get<Text>((int)UI_GameScene.Texts.CounterText).text = count.ToString();
            yield return new WaitForSecondsRealtime(1f);
            count--;
        }
        UGS.Get<Text>((int)UI_GameScene.Texts.CounterText).text = "";
    }

    public void CancelTimer()
    {
        if (CorCountrotin != null)
        {
            StopCoroutine(CorCountrotin);
            CorCountrotin = null;
        }
        UGS.Get<Text>((int)UI_GameScene.Texts.CounterText).text = "";
    }

    #endregion

    public override void Clear()
    {
        Managers.Game.isGameStart = false;
        SetCommentKeyText("");
        SetSpectatingText("");
        ClearMessage();
        Managers.Photon.DestoryAllPlayerObjects(true);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(PhotonNetwork.LocalPlayer == newMasterClient)
        {
            switch(state)
            {
                case Define.GameState.None:
                    PV.RPC("StateController", RpcTarget.AllBufferedViaServer, (int)Define.GameState.Choice);
                    break;
                case Define.GameState.Game:  // 게임 시작전에 방장나가면 라운드 카운터 안하고 다시 맵선택할수 있게
                    if (!Managers.Game.isGameStart)
                    {
                        Managers.Game.ContentsScene.StopAllCoroutines();
                        Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.ReChoice);
                    }
                    break;
                case Define.GameState.Result_0:
                    StartCoroutine(Timer(5, () => { PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_1); }));
                    break;
                case Define.GameState.Result_1:
                    StartCoroutine(Timer(3, () => { PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_2); }));
                    break;
                case Define.GameState.Result_2:
                    if (Util.FindChild(Managers.UI.Root, "UI_RoundResult") == null)
                        Managers.Resource.PhotonInstantiate("UI_RoundResult", Vector3.zero, Quaternion.identity, Vector3.one, Define.PhotonObjectType.RoomObject, Managers.UI.Root.name);
                    Managers.Clear();
                    StartCoroutine(Timer(10, () => { PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_3); }));
                    break;
            }
        }
    }

    IEnumerator Timer(int time, Action action)
    {
        if (holdTime == -1)
            holdTime = time;
        while(holdTime > 0)
        {
            yield return new WaitForSeconds(1f);
            holdTime -= 1;
        }
        holdTime = -1;
        action.Invoke();
    }

    [PunRPC]
    public void PlayerPropertieIntRPC(string key, int value)
    {
        int keyValue = int.Parse(PhotonNetwork.LocalPlayer.CustomProperties[key].ToString()) + value;
        Managers.Photon.SetPlayerPropertie(key, keyValue);
    }
}
