using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LedgeBlockerScene : ContentsScene
{
    Ledge ledge;
    [HideInInspector]
    public List<LBPlayerInfo> LBPList = new List<LBPlayerInfo>();

    [Header("Sound")]
    [SerializeField]
    AudioClip[] hitSound = new AudioClip[3];
    [SerializeField]
    AudioClip failSound;
   
    [Header("UI")]
    UI_LedgeBlockerScene ULBS;

    [Header("Effect")]
    public ParticleSystem particle;
    public Animator anim;

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.ContainsKey("Die") || !PhotonNetwork.IsMasterClient)
            return;

        if ((bool)changedProps["Die"])
            endEvent?.Invoke();
    }

    protected override void Init()
    {
        base.Init();  
        Managers.Game.spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();

        ULBS = Managers.UI.ShowCSceneUI<UI_LedgeBlockerScene>();
    }

    #region 게임 씬, 컨텐츠 시작 정의
    public override void GameInit()
    {
        base.GameInit();
        Managers.Sound.Play2D("SFX/Intro Jingle", Define.Sound2D.Effect2D);
        // 플레이어 생성
        SpawnPlayer_L(new Vector3(0, 2.5f, 4.5f), new Vector3(20, 180, 0), true, 0);
        LBPList.Resize((int)Define.PlayerList.Size);
        var forcePlayer = Managers.Game.myPlayer.GetComponent<LBPlayerInfo>();
        LBPList[(int)Define.PlayerList.LocalPlayer] = forcePlayer;
        LBPList[(int)Define.PlayerList.ForcePlayer] = forcePlayer;
        ledge = GameObject.FindObjectOfType<Ledge>();

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnCheck());
    }
    #endregion

    protected override IEnumerator StartCounter()
    {
        LBPList[(int)Define.PlayerList.LocalPlayer].Init();
        ledge.Align();
        Managers.Game.gameScene.Counter(5);
        yield return new WaitForSecondsRealtime(5f);
        ULBS.SetBtnAction(true);
        Managers.Sound.Play2D("BGM/Mercury", Define.Sound2D.Bgm);
        Managers.Game.gameScene.SetFadeOut(1);
        Managers.Game.isGameStart = true;
        LBPList[(int)Define.PlayerList.LocalPlayer].StartGaugeCoroutine();
    }

    public void Success()
    {
        LBPList[(int)Define.PlayerList.LocalPlayer].hit++;
        LBPList[(int)Define.PlayerList.LocalPlayer].combo++;
        LBPList[(int)Define.PlayerList.LocalPlayer].score += 1 + (LBPList[(int)Define.PlayerList.LocalPlayer].combo * 0.1f);
        particle.Play();
        anim.SetTrigger("Hit");

        LBPList[(int)Define.PlayerList.LocalPlayer].lifeGauge = Mathf.Clamp(LBPList[(int)Define.PlayerList.LocalPlayer].lifeGauge + 5, 0, LBPList[(int)Define.PlayerList.LocalPlayer].maxLifeGauge);

        LBPList[(int)Define.PlayerList.LocalPlayer].ComboBreak();

        Managers.Sound.Play2D(hitSound[Random.Range(0, hitSound.Length)], Define.Sound2D.Effect2D);
    }

    public void Fail()
    {
        LBPList[(int)Define.PlayerList.LocalPlayer].lifeGauge -= 10f;
        LBPList[(int)Define.PlayerList.LocalPlayer].combo = 0;
        Managers.Sound.Play2D(failSound, Define.Sound2D.Effect2D);
    }

    public override void GameEnd_M()
    {
        if (endFlag)
            return;

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

    [PunRPC]
    public override void MainQuestReport(int index)
    {
        ULBS.SetBtnAction(false);
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["Die"] == false)
            mainQuestReport[index].Report();

        Managers.Game.gameScene.StateController((int)Define.GameState.Result_0);
    }

    [PunRPC]
    public override void SubQuestReport(int index)
    {
        subQuestReport[index].Report();
    }

    public override void SetUI<T>(T lpbi)
    {
        LBPList[(int)Define.PlayerList.ForcePlayer] = lpbi as LBPlayerInfo;
    }

    public override void ObserverFunc(GameObject go)
    {
        if (go.TryGetComponent(out LBPlayerInfo lbpi))
        {
            SetUI(lbpi);
        }
    }
}
