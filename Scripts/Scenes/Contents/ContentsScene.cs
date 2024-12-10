using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ContentsScene : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    bool ShowCrossHair;

    public Vector3 navMeshMaxSize;
    public Vector3 navMeshMinSize;

    public UnityAction<int> gameTimeUIEvent;
    [SerializeField]
    int gameTime;
    public int GameTime
    {
        get
        {
            return gameTime;
        }
        set
        {
            gameTime = value;
            gameTimeUIEvent?.Invoke(value);
        }
    }
    public bool timeStop = false;
    public IEnumerator TimerRoutine;

    public PhotonViewEx PV;
    
    public GameObject[] playerPrefabs;
    public GameObject[] roomObjectPrefabs;

    [SerializeField]
    protected Quest[] RegisterMainQuests;
    public QuestReporter[] mainQuestReport;

    [SerializeField]
    protected Quest[] RegisterSubQuests;
    public QuestReporter[] subQuestReport;

    public UnityAction contentUpdateAction;
    public UnityAction contentFixedAction;
    public UnityAction contentLateAction;
    public UnityAction endEvent;
    public UnityAction<GameObject> ObserverAction;
    public bool endFlag { get; protected set; } = false;

    protected Object _lock = new Object();
    protected List<int> posList;
    protected Dictionary<int, Player> posDic;

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GameTime);
            stream.SendNext(timeStop);
            stream.SendNext(endFlag);
        }
        else
        {
            GameTime = (int)stream.ReceiveNext();
            timeStop = (bool)stream.ReceiveNext();
            endFlag = (bool)stream.ReceiveNext();
        }
    }

    #region Awake 초기화
    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        PV = GetComponent<PhotonViewEx>();
        
        Managers.Game.camController = GameObject.FindGameObjectWithTag("CameraController").GetComponent<CameraController>();
        Managers.Game.vCam = GameObject.FindGameObjectWithTag("MVC").GetComponent<CinemachineVirtualCamera>();
        Managers.Game.photonRoomGroup = GameObject.FindGameObjectWithTag("PhotonRoomGroup").transform;
        Managers.Game.photonPlayerGroup = GameObject.FindGameObjectWithTag("PhotonPlayerGroup").transform;

        if (Managers.Game.isTeamMode)
        {
            Managers.Game.photonRedTeam = GameObject.FindGameObjectWithTag("RedTeam").transform;
            Managers.Game.photonBlueTeam = GameObject.FindGameObjectWithTag("BlueTeam").transform;
            Managers.Game.photonPControllGroup = PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == "RedTeam" ? GameObject.FindGameObjectWithTag("RedTeam").transform : GameObject.FindGameObjectWithTag("BlueTeam").transform;
        }
        else
        {
            Managers.Game.photonSolo = GameObject.FindGameObjectWithTag("Solo").transform;
            Managers.Game.photonPControllGroup = GameObject.FindGameObjectWithTag("Solo").transform;
        }

        Managers.Game.ContentsScene = this;
        Managers.Game.SetContentSceneSetting(this, navMeshMinSize, navMeshMaxSize);
        endEvent -= GameEnd_M;
        endEvent += GameEnd_M;
        ObserverAction -= ObserverFunc;
        ObserverAction += ObserverFunc;

        if (ShowCrossHair)
            Managers.Game.gameScene.CrossHair.SetActive(true);
    }

    public void SetListNumber() // 예약할 자리 생성 Awake단에서 넣고 호출
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        posList = new List<int>();
        posDic = new Dictionary<int, Player>();

        for (int i = 0; i < 10; i++)
        {
            posList.Add(i);
        }
    }
    #endregion

    #region #region 게임 씬, 컨텐츠 시작 정의
    private void Start()
    {
        SetScene();
        GameInit();
    }

    void SetScene()
    {
        Managers.Scene.SetCurrentCScene(Managers.Game.gameScene.curMap.sceneName, this);
    }

    public virtual void GameInit()
    {
        foreach (var quest in RegisterMainQuests)
        {
            if (quest.IsAcceptable && !Managers.Quest.ContainsInCompletedQuests(quest))
            {
                Managers.Quest.Register(quest);
            }
        }
        foreach (var quest in RegisterSubQuests)
        {
            if (quest.IsAcceptable && !Managers.Quest.ContainsInCompletedQuests(quest))
                Managers.Quest.Register(quest);
        }
    }
    #endregion

    #region 랜덤자리요청 후 -> 스폰하고 스폰체크요청

    [PunRPC]
    protected void RequestPos_ToM(Player requestPlayer) // GameInit단에 넣고 호출
    {
        lock (_lock)
        {
            int rand = Random.Range(0, posList.Count);
            if (posDic.ContainsKey(posList[rand]))
            {
                RequestPos_ToM(requestPlayer);
                return;
            }
            posDic.Add(posList[rand], requestPlayer);
            posList.Remove(rand);

            CheckRequestCount();
        }
    }

    protected virtual void CheckRequestCount() { } // 자리 요청한 갯수 체크

    protected void RegisterSpawn(int index, bool spawnCheck = true) // 갯수 맞으면 스폰
    {
        for (int i = 0; i < 10; i++)
        {
            posDic.TryGetValue(i, out Player player);
            if (player != null)
            {
                PV.RPC("SpawnPlayer_L", player, i, true, index);
            }
        }
        posList = null;
        posDic = null;

        if (spawnCheck)
            StartCoroutine(SpawnCheck());
    }
    #endregion

    #region 스폰 카운팅후 카운팅 후 게임 시작
    public virtual IEnumerator SpawnCheck()
    {
        while (true)
        {
            int count = 0;
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if ((bool)PhotonNetwork.PlayerList[i].CustomProperties["SpawnOK"])
                    count++;
            }

            if (count == PhotonNetwork.CurrentRoom.PlayerCount)
                break;
            yield return new WaitForSecondsRealtime(1f);
        }

        PV.RPC("GameStartCount_A", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void GameStartCount_A()
    {
        StartCoroutine(StartCounter());
    }

    protected virtual IEnumerator StartCounter()
    {
        Managers.Game.gameScene.Counter(3);
        yield return new WaitForSecondsRealtime(3f);
        Managers.Game.isGameStart = true;
        Managers.Game.gameScene.SetFadeOut(1);
    }

    public virtual IEnumerator TimerStart(int startTime)
    {
        GameTime = startTime;

        while (GameTime > 0 && !timeStop)
        {
            yield return new WaitForSeconds(1f);
            GameTime--;
        }

        endEvent?.Invoke();
    }

    #endregion

    private void Update()
    {
        contentUpdateAction?.Invoke();
    }

    private void FixedUpdate()
    {
        contentFixedAction?.Invoke();
    }

    private void LateUpdate()
    {
        contentLateAction?.Invoke();
    }

    public abstract void GameEnd_M();

    #region 기타 설정
    [PunRPC]
    public virtual void SpawnPlayer_L(int posIndex, bool focurs = true, int prefabIndex = 0)
    {
        if (Managers.Game.myPlayer)
            return;
        string spawnObjectName = Managers.Game.ContentsScene.playerPrefabs[prefabIndex].name;
        Vector3 spawnPos = Managers.Game.spawner.spawnList[posIndex].transform.position;
        Managers.Game.myPlayer = Managers.Resource.PhotonInstantiate(spawnObjectName, spawnPos, Quaternion.identity, Vector3.one, Define.PhotonObjectType.PlayerObject, Managers.Game.photonPControllGroup.name);
        if (focurs)
            Managers.Game.SetCamera(Managers.Game.myPlayer);
        Managers.Photon.SetPlayerPropertie("SpawnOK", true);
    }

    [PunRPC]
    public virtual void SpawnPlayer_L(Vector3 pos,Vector3 rotation, bool focurs = true, int prefabIndex = 0)
    {
        if (Managers.Game.myPlayer)
            return;
        string spawnObjectName = Managers.Game.ContentsScene.playerPrefabs[prefabIndex].name;
        Managers.Game.myPlayer = Managers.Resource.PhotonInstantiate(spawnObjectName, pos, Quaternion.Euler(rotation), Vector3.one, Define.PhotonObjectType.PlayerObject, Managers.Game.photonPControllGroup.name);
        if (focurs)
            Managers.Game.SetCamera(Managers.Game.myPlayer);
        Managers.Photon.SetPlayerPropertie("SpawnOK", true);
    }

    [PunRPC]
    public virtual void SpawnBot_M(Vector3 spawnPos, Vector3 scale,int prefabIndex = 0)
    {
        if (!PhotonNetwork.IsMasterClient && !Managers.Game.isGameStart)
            return;
        string spawnObjectName = Managers.Game.ContentsScene.roomObjectPrefabs[prefabIndex].name;
        Managers.Resource.PhotonInstantiate(spawnObjectName, spawnPos, Quaternion.identity, scale, Define.PhotonObjectType.RoomObject, Managers.Game.photonRoomGroup.name);
    }

    public virtual void ReSpawn() { }

    public virtual void SetUI<T>(T value) where T : Component { }

    public virtual void SetUI(GameObject go){ }

    public virtual void SetStatUI(Stat stat) { }

    [PunRPC]
    protected void SetReward(int value)
    {
        Managers.Game.rewardScore = value;
    }

    [PunRPC]
    public virtual void MainQuestReport(int index)
    {
        mainQuestReport[index].Report();
    }

    [PunRPC]
    public virtual void SubQuestReport(int index)
    {
        subQuestReport[index].Report();
    }

    public virtual void ObserverFunc(GameObject go)
    {
        Debug.Log("호출");
    }
    #endregion

    #region 콜백함수
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
       
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
      
    }
    #endregion
}
