using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ZombieSurviveScene : ContentsScene
{
    UI_ZombieSurviveScene UZSS;
    [SerializeField]
    SpawnRootine sRootine;

    protected override void Init()
    {
        base.Init();

        UZSS = Managers.UI.ShowCSceneUI<UI_ZombieSurviveScene>();
        Managers.Sound.Play2D("BGM/BeachNopeople", Define.Sound2D.Bgm);
        Managers.Game.spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
        
        SetListNumber();

    }

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
            sRootine.RootineStart();        

        float sum = 0;
        foreach (var a in sRootine.spawnList)
            sum += a.waitSpawnTime;

        // 소환 다한후 타이머 작동
        yield return new WaitForSecondsRealtime(sum);
        if (PhotonNetwork.IsMasterClient)
        {
            TimerRoutine = TimerStart(300);
            StartCoroutine(TimerRoutine);
        }
    }

    #endregion

    public override void GameEnd_M()
    {
        if (endFlag)
            return;

        if (Managers.Game.isTeamMode)
        {
            int[,] teamCount = Managers.Photon.BoolPropertieCountTeam("Die", true);

            if (teamCount[0,0] == teamCount[0,1] && teamCount[1, 0] == teamCount[1, 1])
            {
                endFlag = true;
                timeStop = true;
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
            }
            else if (teamCount[0, 0] == teamCount[0, 1] || teamCount[1, 0] == teamCount[1, 1])
            {
                // 레드팀 다 사망 // 블루팀 다 사망

                endFlag = true;
                timeStop = true;
                PV.RPC("MainQuestReport", RpcTarget.AllViaServer, 0);
            }
            else if (GameTime == 0)// 시간 초과로 끝난 경우
            {
                endFlag = true;
                timeStop = true;
                // 무승부
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
            }
        }
        else
        {
            // TODO 사망한 플레이어 체크
            int[] deathCount = Managers.Photon.BoolPropertieCount("Die", true);

            // 혼자 남아서 끝난 경우 다죽은 경우 생존 퀘스트 완료 성공 시키기
            // 총 인원수 - 1 // 해당 카운터 수
            if (deathCount[0] - 1 <= deathCount[1])
            {
                endFlag = true;
                timeStop = true;
                PV.RPC("MainQuestReport", RpcTarget.AllViaServer, 0);
            }
            else if (GameTime == 0 || deathCount[0] == deathCount[1])// 시간 초과로 끝난 경우 // 다죽은경우
            {
                endFlag = true;
                timeStop = true;
                // 무승부
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
            }
        }
    }

    #region 기타 설정
    public override void SetUI(GameObject go)
    {
        BaseController bc = go.GetComponent<BaseController>();
        UZSS.SetStatUI(bc.stat);
        UZSS.SetWeaponCoolUI(bc.weapon);
    }

    public override void SetStatUI(Stat stat)
    {
        UZSS.SetStatUI(stat);      
    }

    [PunRPC]
    public override void MainQuestReport(int index)
    {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Die"] == false)
            mainQuestReport[index].Report();

        Managers.Game.gameScene.StateController((int)Define.GameState.Result_0);
    }
    #endregion

    #region 콜백함수
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("Die") || !PhotonNetwork.IsMasterClient)
            return;

        if ((bool)changedProps["Die"])
            endEvent?.Invoke();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            TimerRoutine = TimerStart(GameTime);
            StartCoroutine(TimerRoutine);
        }
    }
    #endregion
}
