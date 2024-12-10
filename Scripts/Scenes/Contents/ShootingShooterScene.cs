using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootingShooterScene : ContentsScene, IPunObservable
{
    public List<Player> pKillsList = new List<Player>();
    UI_ShootingShooterScene USSS;

    #region Awake 초기화
    protected override void Init()
    {
        base.Init();
        Managers.Sound.Play2D("BGM/BeachNopeople", Define.Sound2D.Bgm);
        USSS = Managers.UI.ShowCSceneUI<UI_ShootingShooterScene>();

        Managers.Game.spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();

        pKillsList = PhotonNetwork.PlayerList.ToList();

        SetListNumber();
    }
    #endregion

    #region 게임 씬, 컨텐츠 시작 정의
    public override void GameInit()
    {
        base.GameInit();

        PV.RPC("RequestPos_ToM", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }
    #endregion

    #region 게임 로딩후 스폰 요청 후 시작 로직
    protected override void CheckRequestCount()
    {
        if (posDic.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            lock (_lock)
            {
                RegisterSpawn(0);
            }
        }
    }
    protected override IEnumerator StartCounter()
    {
        Managers.Game.gameScene.Counter(3);
        yield return new WaitForSecondsRealtime(3f);
        Managers.Game.isGameStart = true;
        Managers.Game.gameScene.SetFadeOut(1);

        if (PhotonNetwork.IsMasterClient)
        {
            TimerRoutine = TimerStart(GameTime);
            StartCoroutine(TimerRoutine);
        }
    }

    #endregion

    public override void GameEnd_M()
    {
        if (endFlag)
            return;

        if (GameTime != 0f)
        {
            if (PhotonNetwork.PlayerList.Length > 1)
                return;

            if (Managers.Game.isTeamMode)
            {
                int red = 0;
                int blue = 0;
                for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                {
                    string team = PhotonNetwork.PlayerList[i].CustomProperties["Team"].ToString();
                    if (team == "RedTeam")
                        red++;
                    else
                        blue++;
                }

                if (red > 0 && blue > 0)
                    return;
            }
        }

        var pArray = PhotonNetwork.PlayerList.OrderByDescending(p => int.Parse(p.CustomProperties["Kill"].ToString())).ToArray();
        int score = 10;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            PV.RPC("SetReward", pArray[i], score);
            score--;
        }

        endFlag = true;
        Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
    }

    #region 기타 설정
    public override void ReSpawn()
    {
        USSS.StartCoroutine(USSS.ReSpawnRoutine());
    }
    #endregion

    #region 콜백함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        USSS.ukb.UpdateKillList(otherPlayer, UI_KillBoard.State.Leave);

        if (!PhotonNetwork.IsMasterClient)
            return;

        endEvent.Invoke();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            if (GameTime > 0)
            {
                TimerRoutine = TimerStart(GameTime);
                StartCoroutine(TimerRoutine);
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("Kill"))
            return;

        lock (_lock)
        {
            // pKillsList = PhotonNetwork.PlayerList.OrderByDescending(p => int.Parse(p.CustomProperties["Kill"].ToString())).ToList();
            USSS.ukb.UpdateKillList(targetPlayer, UI_KillBoard.State.Update);
        }
    }
    #endregion
}
