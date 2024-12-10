using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LastPeopleScene : ContentsScene
{
    UI_LastPeopleScene ULPS;
    RedZoneCreator redZoneCreator;
    MagneticField magneticField;

    #region Awake 초기화
    protected override void Init()
    {
        base.Init();
        Managers.Sound.Play2D("BGM/BeachNopeople", Define.Sound2D.Bgm);
        ULPS = Managers.UI.ShowCSceneUI<UI_LastPeopleScene>();
        redZoneCreator = GetComponent<RedZoneCreator>();
        magneticField = GameObject.FindGameObjectWithTag("MagneticField").GetComponent<MagneticField>();

        Managers.Game.spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();

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
            lock(_lock)
            {
                SpawnBot();
                RegisterSpawn(0);
            }
        }
    }

    public void SpawnBot()
    {
        //int maxBotCount = 50 + 10 * (PhotonNetwork.CurrentRoom.PlayerCount / 2); // min 60 max 100
        int maxBotCount = 100;

        Vector3 spawnPos;
        RaycastHit hitInfo;
        for (int i = 0; i < maxBotCount; i++)
        {
            spawnPos = new Vector3(Random.Range(navMeshMinSize.x, navMeshMaxSize.x), 10f, Random.Range(navMeshMinSize.z, navMeshMaxSize.z));
            if (Physics.Raycast(spawnPos, Vector3.down, out hitInfo, 100f))
            {
                if (hitInfo.transform.gameObject.layer == 7)
                {
                    SpawnBot_M(new Vector3(spawnPos.x, 0f, spawnPos.z), Vector3.one, 0);
                }
                else
                {
                    int skip = 0;
                    int maxSkip = 10;
                    while (skip < maxSkip)
                    {
                        spawnPos = new Vector3(Random.Range(navMeshMinSize.x, navMeshMaxSize.x), 10f, Random.Range(navMeshMinSize.z, navMeshMaxSize.z));
                        if (Physics.Raycast(spawnPos, Vector3.down, out hitInfo, 100f))
                        {
                            if (hitInfo.transform.gameObject.layer == 7)
                            {
                                SpawnBot_M(new Vector3(spawnPos.x, 0f, spawnPos.z), Vector3.one, 0);
                                break;
                            }
                        }
                        skip++;
                    }
                }
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
            redZoneCreator.RoutineStart();
            TimerRoutine = TimerStart(magneticField.WaitTime);
            StartCoroutine(TimerRoutine);
        }
    }

    public override IEnumerator TimerStart(int startTime)
    {
        GameTime = startTime;

        while (GameTime > 0 && !timeStop)
        {
            yield return new WaitForSeconds(1f);
            GameTime--;
        }

        if (timeStop)
            yield break;

        // 타이머 끝났을때 다음 이벤트는?
        magneticField.PageStart();
    }

    #endregion

    public override void GameEnd_M()
    {
        if (endFlag)
            return;

        if(Managers.Game.isTeamMode)
        {
            // 총인원, 해당 변수에 해당하는 인원 수
            int[,] teamCount = Managers.Photon.BoolPropertieCountTeam("Die", true);

            if (teamCount[0, 0] == teamCount[0, 1] && teamCount[1, 0] == teamCount[1, 1])
            {
                endFlag = true;
                timeStop = true;
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
            }
            else if (teamCount[0,0] == teamCount[0,1] || teamCount[1, 0] == teamCount[1, 1])
            {
                // 레드팀 다 사망 // 블루팀 다 사망
                endFlag = true;
                timeStop = true;
                PV.RPC("MainQuestReport", RpcTarget.AllViaServer, 0);
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
            /*
            else if (GameTime == 0)// 시간 초과로 끝난 경우 또는 다 죽어서 끝난 경우
            {
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, 4);
            }
            */
        }
    }

    #region 기타 설정
    public override void SetUI(GameObject go)
    {
        ULPS.SetStatUI(go.GetComponentInChildren<Stat>());
    }

    public override void SetStatUI(Stat stat)
    {
        ULPS.SetStatUI(stat);
    }

    [PunRPC]
    public override void MainQuestReport(int index)
    {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Die"] == false)
            mainQuestReport[index].Report();

        Managers.Game.gameScene.StateController((int)Define.GameState.Result_0);
    }

    [PunRPC]
    public override void SubQuestReport(int index)
    {
        subQuestReport[index].Report();
    }

    public override void ObserverFunc(GameObject go)
    {
        if (go.TryGetComponent(out Stat stat))
            SetStatUI(stat);
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
