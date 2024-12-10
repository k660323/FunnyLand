using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DetailInfo : UI_Base
{
    public RoomInfo roomInfo;
    RectTransform rectTransform;
    CanvasGroup group;

    enum Texts
    {
        RoomNameText,
        RoomPWText,
        RoomCurPeopleText,
        RoomRoundText,
        RoomMapSelectText,
        RoomTeamModeText,
        RoomTeamKillModeText
    }

    public override void Init()
    {
        group = Util.GetOrAddComponent<CanvasGroup>(gameObject);
        rectTransform = Util.GetOrAddComponent<RectTransform>(gameObject);
        Bind<Text>(typeof(Texts));
    }

    public void ShowDetail(RoomInfo _roomInfo,RectTransform _rectRoom = null)
    {
        if (_roomInfo == null)
            return;
        roomInfo = _roomInfo;

        if(_rectRoom != null)
        {
            rectTransform.position = _rectRoom.position;

            if(rectTransform.position.y > Managers.Setting.gOption.height)
                rectTransform.pivot = new Vector2(0, 1);
            else
                rectTransform.pivot = new Vector2(0, 0);
        }

        Get<Text>((int)Texts.RoomNameText).text = $"�� ���� : {_roomInfo.CustomProperties["RoomName"]}";
        Get<Text>((int)Texts.RoomPWText).text = $"��й�ȣ : {(_roomInfo.CustomProperties["PW"].ToString() != "" ? "true" : "false")}";
        Get<Text>((int)Texts.RoomCurPeopleText).text = $"�ִ� �ο� : {_roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        Get<Text>((int)Texts.RoomRoundText).text = $"���� : {_roomInfo.CustomProperties["Round"]}";
        Get<Text>((int)Texts.RoomMapSelectText).text = $"�� ���� : {_roomInfo.CustomProperties["MapSelect"]}";
        Get<Text>((int)Texts.RoomTeamModeText).text = $"���� ���� : {_roomInfo.CustomProperties["Team"]}";
        Get<Text>((int)Texts.RoomTeamKillModeText).text = $"��ų ���� : {_roomInfo.CustomProperties["TeamKill"]}";
        group.alpha = 1;
    }

    public void HideDetail()
    {
        roomInfo = null;
        group.alpha = 0;
    }
}
