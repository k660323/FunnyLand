using BackEnd;
using LitJson;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UI_LobbyScene : UI_Scene
{
    [HideInInspector]
    public UI_CreateNickName createNickName;
    [HideInInspector]
    public UI_FindRoom findRoom;
    [HideInInspector]
    public UI_RoomPW roomPW;

    List<RoomInfo> myRoomList = new List<RoomInfo>();

    List<string> noticeIndateList = new List<string>();

    bool isSearching = false;

    int niceMoodNum = 0;

    bool isDirectAccess = false;

    public enum GameObjects
    {
        Connecting,
    }

    enum Images
    {
        IconImage
    }

    enum Buttons
    {
        SettingButton,
        MyInfoButton,
        StoreButton,
        QuickMatchButton,
        QuickMatchingButton,
        NickMoodButton,
        CreateRoomButton,
        FindRoomButton,
    }

    enum Texts
    {
        NickNameText,
        NoticeText,
        NiceMoodText,
        VersionText
    }

    enum CanvasGroups
    {
        DirectAccessGroup,
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myRoomList.Contains(roomList[i]))
                    myRoomList.Add(roomList[i]);
                else
                    myRoomList[myRoomList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myRoomList.IndexOf(roomList[i]) != -1)
                myRoomList.RemoveAt(myRoomList.IndexOf(roomList[i]));
        }

        if (findRoom != null)
            findRoom.RoomInit(myRoomList);
        if (roomPW != null)
            roomPW.RoomPWInit(myRoomList.Find(room => room.Name == roomPW.roomInfo.Name));
    }

    public override void Init()
    {
        base.Init();

        createNickName = Util.FindChild<UI_CreateNickName>(gameObject);

        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<CanvasGroup>(typeof(CanvasGroups));

        Get<Button>((int)Buttons.MyInfoButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); StopSearching(); Managers.UI.ShowPopupUI<UI_MyInfo>("UI_MyInfo"); });
        Get<Button>((int)Buttons.StoreButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); StopSearching(); Managers.UI.ShowPopupUI<UI_Store>("UI_Store"); });
        Get<Button>((int)Buttons.SettingButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); StopSearching(); Managers.UI.ShowPopupUI<UI_Setting>("Setting", true); });
        Get<Button>((int)Buttons.QuickMatchButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); StartSearching(); });
        Get<Button>((int)Buttons.QuickMatchingButton).gameObject.BindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click"); StopSearching(); });
        Get<Button>((int)Buttons.QuickMatchingButton).gameObject.SetActive(false);

        Get<CanvasGroup>((int)CanvasGroups.DirectAccessGroup).gameObject.BindEvent((data) => { if (!isDirectAccess) StartCoroutine(HideImage()); }, Define.UIEvent.Enter);
        Get<Button>((int)Buttons.NickMoodButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("SFX/UI_MoodUp");
            Get<Text>((int)Texts.NiceMoodText).text = $"+{++niceMoodNum}";
        });
        Get<Button>((int)Buttons.FindRoomButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            StopSearching();
            findRoom = Managers.UI.ShowPopupUI<UI_FindRoom>("UI_FindRoom");
            findRoom.RoomInit(myRoomList);
        });
        Get<Button>((int)Buttons.CreateRoomButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            StopSearching();
            Managers.UI.ShowPopupUI<UI_CreateRoom>("UI_CreateRoom");
        });

        Get<Text>((int)Texts.VersionText).text = $"Ver {Managers.Photon.Version}";

        Managers.UI.SceneUI.UpdateSyncUI += SyncImage;
        Managers.UI.SceneUI.UpdateSyncUI += SyncNick;
        Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
    }

    void SyncImage()
    {
        Get<Image>((int)Images.IconImage).sprite = Managers.Resource.ItemImageLoad(PhotonNetwork.LocalPlayer.CustomProperties["IconImage"].ToString());
    }

    void SyncNick()
    {
        Managers.Data.PlayerInfoData.PlayerNick = Backend.UserNickName;
        Get<Text>((int)Texts.NickNameText).text = Managers.Data.PlayerInfoData.PlayerNick;
    }

    public bool isCreateNick()
    {
        if (Backend.UserNickName == "" || Backend.UserNickName == null)
        {
            // UI »ý¼º
            Managers.UI.ShowPopupUI<UI_CreateNickName>("CreateNickName");
            return false;
        }

        return true;
    }

    public void SetOfficialNotice()
    {
        var result = Backend.Notice.NoticeList(5);

        if (result.IsSuccess())
        {
            JsonData noticeListJson = result.FlattenRows();
            for (int i = 0; i < noticeListJson.Count; i++)
            {
                noticeIndateList.Add(noticeListJson[i]["inDate"].ToString());
            }

            for (int i = 0; i < noticeIndateList.Count; i++)
            {
                var bro = Backend.Notice.NoticeOne(noticeIndateList[i]);

                JsonData jsonData = bro.GetFlattenJSON()["row"];
                if (bro.IsSuccess())
                {
                    Get<Text>((int)Texts.NoticeText).text = $"{i + 1}. {jsonData["content"]} \n";
                }
            }
        }
    }

    IEnumerator HideImage()
    {
        isDirectAccess = true;

        CanvasGroup group = Get<CanvasGroup>((int)CanvasGroups.DirectAccessGroup);
        float fadeDurtaion = 1.0f;
        float timer = fadeDurtaion;

        while (timer > 0)
        {
            timer -= 2 * Time.deltaTime;
            group.alpha = timer / fadeDurtaion;
            yield return null;
        }
        group.transform.SetSiblingIndex(0);
    }

    void StartSearching()
    {
        StopCoroutine(CoSearching());
        StartCoroutine(CoSearching());
    }

    void StopSearching()
    {
        StopCoroutine(CoSearching());
        isSearching = false;
        Get<Button>((int)Buttons.QuickMatchingButton).gameObject.SetActive(false);
    }

    IEnumerator CoSearching()
    {
        isSearching = true;
        Get<Button>((int)Buttons.QuickMatchingButton).gameObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        Hashtable expectedCustomRoomProperties = new Hashtable { { "PW", "" } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public override void OnJoinedRoom()
    {
        if (isSearching)
        {
            StopSearching();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (isSearching)
            StartSearching();
    }

    private void OnDestroy()
    {
        try
        {
            Managers.UI.SceneUI.UpdateSyncUI -= SyncImage;
            Managers.UI.SceneUI.UpdateSyncUI -= SyncNick;
        }
        catch
        {

        }
    }
}
