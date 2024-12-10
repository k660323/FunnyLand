using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UI_Room : UI_Photon
{
    UI_Player myPlayer;
    PhotonViewEx PV;

    string UI = "UI_Player";
    bool isConditionCheck = false;
    PointerEventData _ped;
    GraphicRaycaster _gr;
    List<RaycastResult> _rcList;

    object _lock = new object();
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UI_Init();
        if (PhotonNetwork.IsMasterClient)
            Ready(false);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged != null)
            RoomInfoInit(PhotonNetwork.CurrentRoom.CustomProperties);
    }

    enum Texts
    {
        RoomNameText,
        RoomPWText,
        MaxPlayerText,
        RoundText,
        MapSelectionText,
        TeamModeText,
        TeamKillText,
        RoomVisibleText,
        RoomIDText,
        ReadyText,
        StartText,
    }

    enum Buttons
    {
        RoomEditButton,
        GameStartButton,
        GameReadyButton,
        RedButton,
        BlueButton,
        SettingButton,
        ExitButton,
    }

    enum Transforms
    {
        RedTeam,
        BlueTeam
    }

    public override void Init()
    {
        base.Init();

        _ped = new PointerEventData(EventSystem.current);
        _gr = GetComponent<GraphicRaycaster>();
        _rcList = new List<RaycastResult>();

        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<Transform>(typeof(Transforms));

        Get<Button>((int)Buttons.GameStartButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            GameStart();
        });
        Get<Button>((int)Buttons.GameReadyButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            if (myPlayer != null) 
                Ready(!myPlayer.Ready); 
        });
        Get<Button>((int)Buttons.RoomEditButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            EditRoomOption();
        });
        Get<Button>((int)Buttons.RedButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            if (myPlayer != null && !myPlayer.Ready) 
                PV.RPC("MoveTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, "RedTeam");
        });
        Get<Button>((int)Buttons.BlueButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            if (myPlayer != null && !myPlayer.Ready) 
                PV.RPC("MoveTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, "BlueTeam");
        });
        Get<Button>((int)Buttons.SettingButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            Managers.UI.ShowPopupUI<UI_Setting>("Setting");
        });
        Get<Button>((int)Buttons.ExitButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            PhotonNetwork.LeaveRoom();
        });

        Get<Text>((int)Texts.RoomIDText).text = $"Room ID : {PhotonNetwork.CurrentRoom.Name}";

        UI_Init();
        PV = GetComponent<PhotonViewEx>();
    }

    private void Start()
    {
        PV.RPC("RequestUIPos", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        StartCoroutine(CorRoomInfoInit());
    }

    IEnumerator CorRoomInfoInit()
    {
        yield return new WaitForSeconds(1f);
        RoomInfoInit(PhotonNetwork.CurrentRoom.CustomProperties);
    }

    void UI_Init()
    {
        if (PhotonNetwork.IsMasterClient)
            MasterInit();
        else
            UserInit();
    }

    void UserInit()
    {
        Get<Button>((int)Buttons.RoomEditButton).gameObject.SetActive(false);
        Get<Button>((int)Buttons.GameStartButton).gameObject.SetActive(false);
    }

    void MasterInit()
    {
        Get<Button>((int)Buttons.RoomEditButton).gameObject.SetActive(true);
        Get<Button>((int)Buttons.GameStartButton).gameObject.SetActive(true);
    }

    void RoomInfoInit(Hashtable roomProperties)
    {
        Get<Text>((int)Texts.RoomNameText).text = $"방 제목 : {roomProperties["RoomName"]}";
        Get<Text>((int)Texts.RoomPWText).text = $"비밀번호 : {roomProperties["PW"]}";
        Get<Text>((int)Texts.MaxPlayerText).text = $"최대 인원 : {PhotonNetwork.CurrentRoom.MaxPlayers}";
        Get<Text>((int)Texts.RoundText).text = $"라운드 : {roomProperties["Round"]}";
        Get<Text>((int)Texts.MapSelectionText).text = $"맵 선택 : {roomProperties["MapSelect"]}";
        Get<Text>((int)Texts.TeamModeText).text = $"팀전 여부 : {roomProperties["Team"]}";
        Get<Text>((int)Texts.TeamKillText).text = $"팀킬 여부 : {roomProperties["TeamKill"]}";
        Get<Text>((int)Texts.RoomVisibleText).text = $"공개 여부 : {PhotonNetwork.CurrentRoom.IsVisible}";
    }

    [PunRPC]
    void RequestUIPos(Player requestPlayer)
    {
        int redCount = Get<Transform>((int)Transforms.RedTeam).childCount;
        int blueCount = Get<Transform>((int)Transforms.BlueTeam).childCount;

        if (redCount <= blueCount && Get<Transform>((int)Transforms.RedTeam).childCount < 5)
        {
             PV.RPC("SetUIPos", requestPlayer, true, "RedTeam");
        }
        else
        {
             PV.RPC("SetUIPos", requestPlayer, true, "BlueTeam");
        }
    }

    [PunRPC]
    void SetUIPos(bool isInit, string parent)
    {
        Managers.Photon.SetPlayerPropertie("Team", parent);

        if (isInit)
        {
            GameObject go = Managers.Resource.PhotonInstantiate(UI, Vector3.zero, Quaternion.identity, Vector3.one, Define.PhotonObjectType.PlayerObject, parent);
            myPlayer = go.GetComponent<UI_Player>();
        }
        else
        {
            myPlayer.GetComponent<PhotonViewEx>().RPC("RpcParent", RpcTarget.AllBuffered, parent);
        }
    }

    void EditRoomOption()
    {
        if (PhotonNetwork.IsMasterClient)
            Managers.UI.ShowPopupUI<UI_EditRoom>("UI_EditRoom");
    }
    void Ready(bool isReady)
    {
        myPlayer.Ready = isReady;
        if (isReady)
            Get<Text>((int)Texts.ReadyText).text = "취소";
        else
            Get<Text>((int)Texts.ReadyText).text = "준비";
    }

    void GameStart()
    {
        if (!PhotonNetwork.IsMasterClient && isConditionCheck)
            return;

        isConditionCheck = true;

        bool isTeamMode = (bool)PhotonNetwork.CurrentRoom.CustomProperties["Team"];

        if (isTeamMode)
        {
            int redCount = Get<Transform>((int)Transforms.RedTeam).childCount;
            int blueCount = Get<Transform>((int)Transforms.BlueTeam).childCount;

            if (redCount == blueCount)
            {
                if(ReadyCheck())
                {
                    //TODO : 게임 시작
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PV.RPC("GameSceneLoad",RpcTarget.AllBufferedViaServer);
                }
                else
                {
                    Util.SimplePopup("레디하지 않은 플레이어가 있어\n 게임을 시작 할 수 없습니다.");
                    isConditionCheck = false;
                }
            }
            else
            {
                Util.SimplePopup("각 팀원 수가 맞지 않아 게임을 시작 할 수 없습니다.");
                isConditionCheck = false;
            }
        }
        else
        {
            if(ReadyCheck())
            {
                // TODO : 게임 시작
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PV.RPC("GameSceneLoad", RpcTarget.AllBufferedViaServer);
            }
            else
            {
                Util.SimplePopup("레디하지 않은 플레이어가 있어\n 게임을 시작 할 수 없습니다.");
                isConditionCheck = false;
            }
        }
    }

    bool ReadyCheck()
    {
        int count = 0;
        for (int i = 0; i < Get<Transform>((int)Transforms.RedTeam).childCount; i++)
        {
            Get<Transform>((int)Transforms.RedTeam).GetChild(i).TryGetComponent<UI_Player>(out UI_Player playerComponent);
            if(playerComponent != null)
            {
                if (playerComponent.Ready)
                    count++;
            }
        }

        for (int i = 0; i < Get<Transform>((int)Transforms.BlueTeam).childCount; i++)
        {
            Get<Transform>((int)Transforms.BlueTeam).GetChild(i).TryGetComponent<UI_Player>(out UI_Player playerComponent);
            if (playerComponent != null)
            {
                if (playerComponent.Ready)
                    count++;
            }
        }

        if (count == PhotonNetwork.CurrentRoom.PlayerCount - 1)
        {
            return true;
        }

        return false;
    }

    [PunRPC]
    void GameSceneLoad()
    {
        Managers.Scene.PhotonLoadScene(Define.Scene.Loading);
    }

    public void PointerUpPos(UI_Player uI_Player)
    {
        if (uI_Player == null)
            return;

        _ped.position = Input.mousePosition;
        _rcList.Clear();

        _gr.Raycast(_ped, _rcList);

        if (_rcList.Count == 0)
            return;

        foreach (var raycast in _rcList)
        {
            var team = raycast.gameObject.GetComponent<UI_TeamRange>();
            if (team)
            {
                MoveTeam(uI_Player.PV.Owner, team.gameObject.name);
                break;
            }
        }
    }

    [PunRPC]
    void MoveTeam(Player requestPlayer, string team)
    {
        int Count = 5;
        if (team == "RedTeam")
            Count = Get<Transform>((int)Transforms.RedTeam).childCount;
        else if (team == "BlueTeam")
            Count = Get<Transform>((int)Transforms.BlueTeam).childCount;

        if (Count < 5)
            PV.RPC("SetUIPos", requestPlayer, false, team);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Managers.Sound.Play2D("SFX/EnterPlayer");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Managers.Sound.Play2D("SFX/LeavePlayer");
    }
}
