using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RoomInfo : UI_Base
{
    public RectTransform rectRoom;

    public enum Buttons
    {
        Button
    }

    public enum Images
    {
        LockImage
    }

    public enum Texts
    {
        RoomNameText,
        CurPeopleText
    }

    public int roomIndex;

    public override void Init()
    {
        rectRoom = GetComponent<RectTransform>();
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
    }
}
