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

        Get<Text>((int)Texts.RoomNameText).text = $"방 제목 : {_roomInfo.CustomProperties["RoomName"]}";
        Get<Text>((int)Texts.RoomPWText).text = $"비밀번호 : {(_roomInfo.CustomProperties["PW"].ToString() != "" ? "true" : "false")}";
        Get<Text>((int)Texts.RoomCurPeopleText).text = $"최대 인원 : {_roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        Get<Text>((int)Texts.RoomRoundText).text = $"라운드 : {_roomInfo.CustomProperties["Round"]}";
        Get<Text>((int)Texts.RoomMapSelectText).text = $"맵 선택 : {_roomInfo.CustomProperties["MapSelect"]}";
        Get<Text>((int)Texts.RoomTeamModeText).text = $"팀전 여부 : {_roomInfo.CustomProperties["Team"]}";
        Get<Text>((int)Texts.RoomTeamKillModeText).text = $"팀킬 여부 : {_roomInfo.CustomProperties["TeamKill"]}";
        group.alpha = 1;
    }

    public void HideDetail()
    {
        roomInfo = null;
        group.alpha = 0;
    }
}
