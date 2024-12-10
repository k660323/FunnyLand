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

    #region ���� ��, ������ ���� ����
    public override void GameInit()
    {
        base.GameInit();

        PV.RPC("RequestPos_ToM", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }
    #endregion

    #region ���� �ε��� ���� ��û �� ���� ����

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

        // ��ȯ ������ Ÿ�̸� �۵�
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
                // ������ �� ��� // ����� �� ���

                endFlag = true;
                timeStop = true;
                PV.RPC("MainQuestReport", RpcTarget.AllViaServer, 0);
            }
            else if (GameTime == 0)// �ð� �ʰ��� ���� ���
            {
                endFlag = true;
                timeStop = true;
                // ���º�
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
            }
        }
        else
        {
            // TODO ����� �÷��̾� üũ
            int[] deathCount = Managers.Photon.BoolPropertieCount("Die", true);

            // ȥ�� ���Ƽ� ���� ��� ������ ��� ���� ����Ʈ �Ϸ� ���� ��Ű��
            // �� �ο��� - 1 // �ش� ī���� ��
            if (deathCount[0] - 1 <= deathCount[1])
            {
                endFlag = true;
                timeStop = true;
                PV.RPC("MainQuestReport", RpcTarget.AllViaServer, 0);
            }
            else if (GameTime == 0 || deathCount[0] == deathCount[1])// �ð� �ʰ��� ���� ��� // ���������
            {
                endFlag = true;
                timeStop = true;
                // ���º�
                Managers.Game.gameScene.PV.RPC("StateController", RpcTarget.AllViaServer, (int)Define.GameState.Result_0);
            }
        }
    }

    #region ��Ÿ ����
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

    #region �ݹ��Լ�
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
