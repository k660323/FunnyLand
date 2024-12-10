using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Realtime;

public class UI_RoomPW : UI_Popup
{
    public RoomInfo roomInfo { get; private set; }
    string pw;
    
    enum Texts
    {
        RoomNameText
    }

    enum InputFields
    {
        InputField_Password
    }

    enum Buttons
    {
        ConfirmButton,
        CloseButton,
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.ConfirmButton).gameObject.BindEvent(CheckPW);
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => ClosePopupUI());
    }

    public void RoomPWInit(RoomInfo _roomInfo)
    {
        if (_roomInfo == null)
        {
            Managers.UI.CloseNewShowPopUp<UI_Notice>(false, "UI_Notice", "������ ����� ���Դϴ�.");
            return;
        }
        roomInfo = _roomInfo;

        Get<Text>((int)Texts.RoomNameText).text = roomInfo.CustomProperties["RoomName"].ToString();
    }

    void CheckPW(PointerEventData data)
    {
        string inputPW = Get<InputField>((int)InputFields.InputField_Password).text;
        bool isJoin = roomInfo.PlayerCount < roomInfo.MaxPlayers;
        pw = roomInfo.CustomProperties["PW"].ToString();
        if (isJoin)
        {
            if (inputPW == pw)
                PhotonNetwork.JoinRoom(roomInfo.Name);
            else
                Util.SimplePopup("��й�ȣ�� ��ġ���� �ʽ��ϴ�.");
        }
        else
        {
            Util.SimplePopup("���� �� á���ϴ�.");
        }
    }
}
