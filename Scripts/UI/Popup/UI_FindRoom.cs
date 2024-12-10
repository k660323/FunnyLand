using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UI_FindRoom : UI_Popup
{
    enum UI_DetailInfos
    {
        RoomDetailInfo,
    }

    enum Rooms
    {
        RoomList
    }
    enum UI_Rooms
    {
        RoomObject0,
        RoomObject1,
        RoomObject2,
        RoomObject3,
        RoomObject4,
        RoomObject5,
    }
    enum Buttons
    {
        CloseButton,
        LeftButton,
        RightButton,
        RefreshButton
    }

    enum Texts
    {
        PageText,
    }

    int RoomCount = 0;

    public List<RoomInfo> myRoomList = new List<RoomInfo>();

    int curPage = 1, maxPage, multiple;

    public override void Init()
    {
        base.Init();

        Bind<UI_DetailInfo>(typeof(UI_DetailInfos));
        Bind<GameObject>(typeof(Rooms));
        Bind<UI_RoomInfo>(typeof(UI_Rooms));
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject0).gameObject.BindEvent(data =>
         { ShowRoomDetailInfo(0, Get<UI_RoomInfo>((int)UI_Rooms.RoomObject0).rectRoom);},Define.UIEvent.Enter);
         
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject1).gameObject.BindEvent(data =>
         { ShowRoomDetailInfo(1, Get<UI_RoomInfo>((int)UI_Rooms.RoomObject1).rectRoom); }, Define.UIEvent.Enter);
         
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject2).gameObject.BindEvent(data =>
         { ShowRoomDetailInfo(2, Get<UI_RoomInfo>((int)UI_Rooms.RoomObject2).rectRoom); }, Define.UIEvent.Enter);
         
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject3).gameObject.BindEvent(data =>
         { ShowRoomDetailInfo(3, Get<UI_RoomInfo>((int)UI_Rooms.RoomObject3).rectRoom); }, Define.UIEvent.Enter);
         
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject4).gameObject.BindEvent(data =>
         { ShowRoomDetailInfo(4, Get<UI_RoomInfo>((int)UI_Rooms.RoomObject4).rectRoom); }, Define.UIEvent.Enter);
         
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject5).gameObject.BindEvent(data =>
         { ShowRoomDetailInfo(5, Get<UI_RoomInfo>((int)UI_Rooms.RoomObject5).rectRoom); }, Define.UIEvent.Enter);

        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject0).gameObject.BindEvent(data => { HideRoomDetailInfo(); }, Define.UIEvent.Exit);
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject1).gameObject.BindEvent(data => { HideRoomDetailInfo(); }, Define.UIEvent.Exit);
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject2).gameObject.BindEvent(data => { HideRoomDetailInfo(); }, Define.UIEvent.Exit);
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject3).gameObject.BindEvent(data => { HideRoomDetailInfo(); }, Define.UIEvent.Exit);
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject4).gameObject.BindEvent(data => { HideRoomDetailInfo(); }, Define.UIEvent.Exit);
        Get<UI_RoomInfo>((int)UI_Rooms.RoomObject5).gameObject.BindEvent(data => { HideRoomDetailInfo(); }, Define.UIEvent.Exit);


        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            ClosePopupUI();
        });
        Get<Button>((int)Buttons.RefreshButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            RoomInit(myRoomList); 
        });

        Get<Button>((int)Buttons.LeftButton).onClick.AddListener(() =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            BtnClick(-2); 
        });
        Get<Button>((int)Buttons.RightButton).onClick.AddListener(() =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            BtnClick(-1);
        });

        RoomCount = Get<GameObject>((int)Rooms.RoomList).transform.childCount;
        myRoomList.Clear();
    }

    private void Start()
    {
        for (int i = 0; i < RoomCount; i++)
        {
            int roomNum = i;
            Get<UI_RoomInfo>(i).Get<Button>((int)UI_RoomInfo.Buttons.Button).onClick.AddListener(() => JoinRoom(roomNum));
        }
    }

    #region 방정보 가져오기
    void SetRoomInfo()
    {
        if (Get<UI_DetailInfo>((int)UI_DetailInfos.RoomDetailInfo).roomInfo != null)
            Get<UI_DetailInfo>((int)UI_DetailInfos.RoomDetailInfo).
            ShowDetail(myRoomList.Find(room => room.Name == Get<UI_DetailInfo>((int)UI_DetailInfos.RoomDetailInfo).roomInfo.Name));
    }

    void ShowRoomDetailInfo(int index,RectTransform rectRoom)
    {
        if (index + multiple >= myRoomList.Count)
            return;
        Managers.Sound.Play2D("SFX/UI_Pop2");
        Get<UI_DetailInfo>((int)UI_DetailInfos.RoomDetailInfo).ShowDetail(myRoomList[index + multiple], rectRoom);
    }

    void HideRoomDetailInfo()
    {
        Get<UI_DetailInfo>((int)UI_DetailInfos.RoomDetailInfo).HideDetail();
    }
    #endregion

    #region 방리스트 정리
    // 왼쪽(-2) 오른쪽(-1) 셀숫자(그외)
    void BtnClick(int num)
    {
        if (num == -2) --curPage;
        else if (num == -1) ++curPage;
        else print(myRoomList[multiple + num]);

        RoomInit(myRoomList);
    }

    public void RoomInit(List<RoomInfo> roomList)
    {
        myRoomList = roomList;

        maxPage = (roomList.Count % RoomCount == 0 ? roomList.Count / RoomCount : roomList.Count / RoomCount + 1);
        
        multiple = (curPage - 1) * RoomCount;

        Get<Button>((int)Buttons.LeftButton).interactable = (curPage <= 1) ? false : true;
        Get<Button>((int)Buttons.RightButton).interactable = (curPage >= maxPage) ? false : true;
        Get<Text>((int)Texts.PageText).text = $"{curPage}/{maxPage}";

        for (int i = 0; i < RoomCount; i++)
        {
            string roomName = "";
            string pw = "";
            string playerCount = "";
            bool isActive = false;

            if (i + multiple < roomList.Count)
            {
                Hashtable hs = roomList[i + multiple].CustomProperties;
                roomName = hs["RoomName"].ToString();
                pw = hs["PW"].ToString();
                playerCount = $"{roomList[i + multiple].PlayerCount}/{roomList[i + multiple].MaxPlayers}";
                isActive = true;
            }

            Get<UI_RoomInfo>(i).Get<Text>((int)UI_RoomInfo.Texts.RoomNameText).text = roomName;
            Get<UI_RoomInfo>(i).Get<Image>((int)UI_RoomInfo.Images.LockImage).enabled = (pw != "" ? true : false);
            Get<UI_RoomInfo>(i).Get<Text>((int)UI_RoomInfo.Texts.CurPeopleText).text = playerCount;
            Get<UI_RoomInfo>(i).Get<Button>((int)UI_RoomInfo.Buttons.Button).interactable = isActive;
        }

        SetRoomInfo();
    }
    #endregion

    #region 방 입장
    void JoinRoom(int i)
    {
        if (myRoomList[i + multiple] == null)
            return;
        Hashtable hs = myRoomList[i + multiple].CustomProperties;
        string pw = hs["PW"].ToString();
        bool isJoin = myRoomList[i + multiple].PlayerCount < myRoomList[i + multiple].MaxPlayers;

        if(isJoin)
        {
            if (!string.IsNullOrWhiteSpace(pw))
            {
                UI_LobbyScene uls = Managers.UI.SceneUI as UI_LobbyScene;
                uls.roomPW = Managers.UI.ShowPopupUI<UI_RoomPW>("RoomPW");
                uls.roomPW.RoomPWInit(myRoomList[i + multiple]);
            }
            else
            {
                PhotonNetwork.JoinRoom(myRoomList[i + multiple].Name);
            }
        }
        else
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            Util.SimplePopup("방이 꽉 찼습니다.");
        }
    }

    public override void OnJoinedRoom()
    {
        CloseAllPopupUI();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Managers.Sound.Play2D("SFX/UI_Error");
        Util.SimplePopup("방이 존재하지않거나 \n 인원이 꽉찬 방 입니다.");
    }
    #endregion
}
