using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_CreateRoom : UI_Popup
{
    enum InputFields
    {
        RoomNameInputField,
        RoomLockeInputField,
    }

    enum Dropdowns
    {
        RoomPeopleDropdown,
        GameRoundDropdown,
        RoomMapSelectDropdown
    }

    enum Toggles
    {
        RoomTeamToggle,
        RoomTeamKillToggle,
        RoomVisibleSelectToggle
    }

    enum Buttons
    {
        CreateButton,
        CancelButton
    }

    public override void Init()
    {
        base.Init();

        Bind<InputField>(typeof(InputFields));
        Bind<Dropdown>(typeof(Dropdowns));
        Bind<Toggle>(typeof(Toggles));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.CreateButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            CreateRoom(); 
        });
        Get<Button>((int)Buttons.CancelButton).gameObject.BindEvent((data) => { 
            Managers.Sound.Play2D("SFX/UI_Click"); 
            ClosePopupUI(); 
        });
    }

    void CreateRoom()
    {
        string roomName = Get<InputField>((int)InputFields.RoomNameInputField).text;
        if(roomName == "")
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            Util.SimplePopup("방 이름은 공백으로 둘 수 없습니다.");
            return;
        }
        
        string _roomName = Get<InputField>((int)InputFields.RoomNameInputField).text;
        string _pw = Get<InputField>((int)InputFields.RoomLockeInputField).text;
        int _curRound = 1;
        int _round = int.Parse(Get<Dropdown>((int)Dropdowns.GameRoundDropdown).captionText.text);
        string _mapSelect = Get<Dropdown>((int)Dropdowns.RoomMapSelectDropdown).captionText.text;
        bool _team = Get<Toggle>((int)Toggles.RoomTeamToggle).isOn;
        bool _teamKill = Get<Toggle>((int)Toggles.RoomTeamKillToggle).isOn;
        byte _maxPlayers = (byte)(Get<Dropdown>((int)Dropdowns.RoomPeopleDropdown).value + 2);
        bool _roomVisible = Get<Toggle>((int)Toggles.RoomVisibleSelectToggle).isOn;

        PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.UserId, 
        Managers.Photon.InitRoomProperties(_roomName, _pw, _curRound, _round, _mapSelect, _team, _teamKill, _maxPlayers, _roomVisible));
    }

    public override void OnCreatedRoom()
    {
        Managers.Resource.PhotonInstantiate("UI_Room", Vector3.zero, Quaternion.identity, Vector3.one, Define.PhotonObjectType.RoomObject, Managers.UI.Root.name);
        CloseAllPopupUI();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Managers.Sound.Play2D("SFX/UI_Error");
        Util.SimplePopup("같은 이름의 방이 존재하거나 \n 네트워크 문제입니다.");
    }
}
