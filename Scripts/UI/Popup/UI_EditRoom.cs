using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UI_EditRoom : UI_Popup
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
        CommitButton,
        CancelButton
    }

    public override void Init()
    {
        base.Init();

        Bind<InputField>(typeof(InputFields));
        Bind<Dropdown>(typeof(Dropdowns));
        Bind<Toggle>(typeof(Toggles));
        Bind<Button>(typeof(Buttons));

        Hashtable roomTable = PhotonNetwork.CurrentRoom.CustomProperties;

        Get<InputField>((int)InputFields.RoomNameInputField).text = roomTable["RoomName"].ToString();
        Get<InputField>((int)InputFields.RoomLockeInputField).text = roomTable["PW"].ToString();
        Get<Dropdown>((int)Dropdowns.RoomPeopleDropdown).value = PhotonNetwork.CurrentRoom.MaxPlayers - 2;

        string roundData = roomTable["Round"].ToString();
        Get<Dropdown>((int)Dropdowns.GameRoundDropdown).value = Get<Dropdown>((int)Dropdowns.GameRoundDropdown).options.FindIndex(option => option.text == roundData);

        string mapSelectData = roomTable["MapSelect"].ToString();
        Get<Dropdown>((int)Dropdowns.RoomMapSelectDropdown).value = Get<Dropdown>((int)Dropdowns.RoomMapSelectDropdown).options.FindIndex(option => option.text == mapSelectData);

        Get<Toggle>((int)Toggles.RoomTeamToggle).isOn = (bool)roomTable["Team"];
        Get<Toggle>((int)Toggles.RoomTeamKillToggle).isOn = (bool)roomTable["TeamKill"];
        Get<Toggle>((int)Toggles.RoomVisibleSelectToggle).isOn = PhotonNetwork.CurrentRoom.IsVisible;

        Get<Button>((int)Buttons.CommitButton).gameObject.BindEvent((data) => CommitRoomOption());
        Get<Button>((int)Buttons.CancelButton).gameObject.BindEvent((data) => ClosePopupUI());
    }

    void CommitRoomOption()
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (Get<InputField>((int)InputFields.RoomNameInputField).text == "")
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            Util.SimplePopup("방 제목은 공백으로 둘 수 없습니다.");
            return;
        }
        if (Get<Dropdown>((int)Dropdowns.RoomPeopleDropdown).value + 2 < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            Util.SimplePopup("현재 유저수 보다 적게 할 수 없습니다.");
            return;
        }

        string _roomName = Get<InputField>((int)InputFields.RoomNameInputField).text;
        string _pw = Get<InputField>((int)InputFields.RoomLockeInputField).text;
        int _round = int.Parse(Get<Dropdown>((int)Dropdowns.GameRoundDropdown).captionText.text);
        string _mapSelect = Get<Dropdown>((int)Dropdowns.RoomMapSelectDropdown).captionText.text;
        bool _team = Get<Toggle>((int)Toggles.RoomTeamToggle).isOn;
        bool _teamKill = Get<Toggle>((int)Toggles.RoomTeamKillToggle).isOn;
        byte _maxPlayers = (byte)(Get<Dropdown>((int)Dropdowns.RoomPeopleDropdown).value + 2);
        bool _roomVisible = Get<Toggle>((int)Toggles.RoomVisibleSelectToggle).isOn;

        PhotonNetwork.CurrentRoom.SetCustomProperties(Managers.Photon.SetRoomProperties(_roomName, _pw, _round, _mapSelect, _team, _teamKill, _maxPlayers, _roomVisible));
    }
}
