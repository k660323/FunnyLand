using Cinemachine;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public bool isGameStart;
    public int rewardScore = 0;
    public bool isTeamMode { get; private set; }
    public bool isTeamKill { get; private set; }

    public GameScene gameScene;
    ContentsScene contentsScene;
    public ContentsScene ContentsScene
    {
        get
        {
            if (contentsScene == null)
            {
                contentsScene = Managers.FindObjectOfType<ContentsScene>();
            }
            return contentsScene;
        }
        set
        {
            if (contentsScene == null)
                contentsScene = value;
        }
    }
    public Vector3 navMeshMinSize;
    public Vector3 navMeshMaxSize;
    public float defaultPointHeight { get; private set; } = 10f;
    public float defaultRayLength { get; private set; } = 15f;

    public Transform photonRoomGroup;
    public Transform photonPlayerGroup;
    public Transform photonPControllGroup;
    public Transform photonSolo;
    public Transform photonRedTeam;
    public Transform photonBlueTeam;

    public Spawner spawner;

    public CameraController camController;
    public CinemachineVirtualCamera vCam;

    #region 게임 오브젝트 관리 함수
    public struct PlayerObject
    {
        public Player player;
        public GameObject gameObject;
        public PhotonViewEx PV;
        public PlayerObject(Player _player, PhotonViewEx _pv, GameObject _go)
        {
            player = _player;
            PV = _pv;
            gameObject = _go;
        }
    }

    public GameObject myPlayer;
    public GameObject attackPlayer;

    public Dictionary<int, PlayerObject> _playersObjects { get; private set; } = new Dictionary<int, PlayerObject>();
    public Dictionary<int, GameObject> _roomObjects { get; private set; } = new Dictionary<int, GameObject>();
    #endregion

    public int viewIndex = 0;

    public void SetGameSceneSetting(GameScene gs, bool _isTeamMode, bool _isTeamKill)
    {
        gameScene = gs;
        isTeamMode = _isTeamMode;
        isTeamKill = _isTeamKill;
    }

    public void SetContentSceneSetting(ContentsScene cs, Vector3 _navMeshMinSize, Vector3 _navMeshMaxSize)
    {
        ContentsScene = cs;
        navMeshMinSize = _navMeshMinSize;
        navMeshMaxSize = _navMeshMaxSize;

        defaultPointHeight += _navMeshMaxSize.y;

        float mapYLength = Mathf.Max(Mathf.Abs(_navMeshMinSize.y), Mathf.Abs(_navMeshMaxSize.y));
        defaultRayLength += mapYLength;
    }

    #region 오브젝트 서치
    public void SetPhotonObject(GameObject go)
    {
        if(go.TryGetComponent(out PhotonViewEx PV))
        {
            if(PV.IsRoomView) // 룸 오브젝트
                SetRoomObj(go);
            else // 플레이어 오브젝트
                SetPlayer(go);
        }
    }

    public GameObject GetPhotonObject(int viewID)
    {
        if (viewID < 1000) // 룸 오브젝트
            return GetRoomObj(viewID);
        else // 플레이어 오브젝트
            return GetPlayerObject(viewID);
    }

    public string GetObjectNickName(int viewID)
    {
        if (viewID < 1000) // 룸 오브젝트
            return GetRoomObj(viewID).name;
        else // 플레이어 오브젝트
            return GetPlayer(viewID).NickName;
    }
    #endregion

    #region 플레이어 오브젝트

    void SetPlayer(GameObject playerObject)
    {
        if (playerObject.TryGetComponent(out PhotonViewEx PV))
        {
            if (_playersObjects.ContainsKey(PV.ViewID))
                _playersObjects.Remove(PV.ViewID);

            _playersObjects.Add(PV.ViewID, new PlayerObject(PV.Owner, PV, playerObject));
        }
    }

    Player GetPlayer(int viewID)
    {
        _playersObjects.TryGetValue(viewID, out PlayerObject playerObject);
        return playerObject.player;
    }

    public PhotonViewEx GetPlayerObjectPV(int viewID)
    {
        _playersObjects.TryGetValue(viewID, out PlayerObject playerObject);
        return playerObject.PV;
    }

    GameObject GetPlayerObject(int viewID)
    {
        _playersObjects.TryGetValue(viewID, out PlayerObject playerObject);
        return playerObject.gameObject;
    }
    #endregion

    #region 봇 오브젝트
    public void SetRoomObj(GameObject roomObject)
    {
        if (roomObject.TryGetComponent(out PhotonViewEx PV))
        {
            if (_roomObjects.ContainsKey(PV.ViewID))
                _roomObjects.Remove(PV.ViewID);
            _roomObjects.Add(PV.ViewID, roomObject);
        }
    }

    public GameObject GetRoomObj(int viewID)
    {
        _roomObjects.TryGetValue(viewID, out GameObject roomObject);
        return roomObject;
    }

    public PhotonViewEx GetRoomObjPV(int viewID)
    {
        GameObject roomObject = GetRoomObj(viewID);
        roomObject.TryGetComponent(out PhotonViewEx PV);
        return PV;
    }
    #endregion

    #region 카메라 관련 옵션
    public void SetCamera(GameObject target)
    {
        camController.SetTarget(target, vCam);
    }

    public IEnumerator ChangeObserveMode(GameObject targetPlayer = null)
    {
        yield return new WaitForSeconds(3f);
        Managers.Game.ObserverMode(targetPlayer);
    }

    public void ObserverMode(GameObject targetPlayer = null)
    {
        if(Managers.Game.isTeamMode)
        {
            OtherPlayerViewIndex(1);
        }
        else
        {
            for(int i = 0; i < Managers.Game.photonSolo.childCount; i++)
            {
                if(Managers.Game.photonSolo.GetChild(i).TryGetComponent(out BaseController baseController))
                {
                    baseController.SetNickNameView(true);
                }
            }
            if (targetPlayer != null)
                OtherPlayerViewObject(targetPlayer);
        }

        Managers.Input.MouseAction -= ObserverViewChange;
        Managers.Input.MouseAction += ObserverViewChange;
    }

    public void OtherPlayerViewObject(GameObject target)
    {
        if (photonRoomGroup.childCount == 0)
            return;
        viewIndex = target.transform.GetSiblingIndex();

        contentsScene.ObserverAction?.Invoke(target);
        
        string nickName = $"<color=yellow>관전 중 :</color> {target.GetComponent<PhotonViewEx>().Owner.NickName}";
        Managers.Game.gameScene.SetSpectatingText(nickName);
        SetCamera(target);
    }

    public void ObserverViewChange(Define.MouseEvent mouseEvent)
    {
        if (mouseEvent == Define.MouseEvent.LClick)
        {
            OtherPlayerViewIndex(-1);
        }
        else if (mouseEvent == Define.MouseEvent.RClick)
        {
            OtherPlayerViewIndex(1);
        }
    }

    public void OtherPlayerViewIndex(int num)
    {
        if (photonPControllGroup.childCount == 0)
            return;

        viewIndex = (viewIndex + num + photonPControllGroup.childCount) % photonPControllGroup.childCount;
        GameObject player = photonPControllGroup.GetChild(viewIndex).gameObject;
        contentsScene.ObserverAction?.Invoke(player);
        string nickName = $"<color=yellow>관전 중 :</color> {player.GetComponent<PhotonViewEx>().Owner.NickName}";
        Managers.Game.gameScene.SetSpectatingText(nickName);
        SetCamera(player);
    }
    #endregion

    public void Clear()
    {
        photonRoomGroup = null;
        photonPlayerGroup = null;
        photonPControllGroup = null;
        photonSolo = null;
        photonRedTeam = null;
        photonBlueTeam = null;
        spawner = null;
        camController = null;
        vCam = null;
        myPlayer = null;
        attackPlayer = null;
        _playersObjects.Clear();
        _roomObjects.Clear();
    }
}
