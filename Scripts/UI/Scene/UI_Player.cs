using BackEnd;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Player : UI_Base, IPunObservable
{
    public PhotonViewEx PV { get; private set; }
    bool ready = false;
    public bool Ready
    {
        get
        {
            return ready;
        }
        set
        {
            ready = value;
            Get<Image>((int)Images.ReadyImage).enabled = value;
        }
    }

    Transform _beginParent;
    int _beginIndex;
    Vector3 _beginPosition;
    Vector3 _beginMousePosition;

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        SetUI();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Ready);
            stream.SendNext(Get<Image>((int)Images.MasterImage).enabled);
        }
        else
        {
            Ready = (bool)stream.ReceiveNext();
            Get<Image>((int)Images.MasterImage).enabled = (bool)stream.ReceiveNext();
        }
    }

    public enum Images
    {
        PlayerImage,
        ReadyImage,
        MasterImage
    }

    enum Texts
    {
        NickNameText,
        WinRateText
    }

    enum Buttons
    {
        KickButton
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        PV = GetComponent<PhotonViewEx>();
        SetUI();
        if (PV.IsMine)
        {
            PV.RPC("SetPlayerInfo", RpcTarget.AllBuffered, Managers.Data.PlayerInfoData.PlayerIcon, Managers.Data.PlayerInfoData.PlayerNick, Managers.Data.PlayerInfoData.WinRate.ToString());
        }

        Get<Button>((int)Buttons.KickButton).onClick.AddListener(() =>{
            Managers.Sound.Play2D("SFX/UI_Click");
            KickPlayerSign(); 
        });

        Get<Image>((int)Images.PlayerImage).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            RequestPlayerInfo();
        }, Define.UIEvent.Click);

        gameObject.BindEvent((data) =>
        {
            OnPointerDown();
        }, Define.UIEvent.Down);

        gameObject.BindEvent((data) =>
        {
            OnPointerDrag();
        }, Define.UIEvent.Drag);

        gameObject.BindEvent((data) =>
        {
            OnPointerUp();
        }, Define.UIEvent.Up);
    }

    void SetUI()
    {
        if (PV.IsMine)
        {
            if (PhotonNetwork.IsMasterClient)
                Get<Image>((int)Images.MasterImage).enabled = true;
            else
                Get<Image>((int)Images.MasterImage).enabled = false;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!PV.IsMine)
                    Get<Button>((int)Buttons.KickButton).interactable = true;
            }
            else
            {
                Get<Button>((int)Buttons.KickButton).interactable = false;
            }
        }
    }

    [PunRPC]
    void SetPlayerInfo(int imageIndex, string nickName, string rate)
    {
        if (Managers.Data.ItemDict.TryGetValue(imageIndex, out Data.ItemData value))
            Get<Image>((int)Images.PlayerImage).sprite = Managers.Resource.ItemImageLoad(value.Image);
        Get<Text>((int)Texts.NickNameText).color = PV.IsMine ? Color.green : Color.white;
        Get<Text>((int)Texts.NickNameText).text = nickName;
        Get<Text>((int)Texts.WinRateText).text = $"승률 {rate}%";
    }

    void KickPlayerSign()
    {
        string nickName = Get<Text>((int)Texts.NickNameText).text;
        UI_Notice notice = Util.SimplePopup($"{nickName}님을 강퇴 하시겠습니까?");
        notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent(data => { PV.RPC("KickPlayer",PV.Owner); notice.ClosePopupUI(); });
        notice.ActiveCancelBtn();
    }

    [PunRPC]
    void KickPlayer()
    {
        PhotonNetwork.LeaveRoom();
    }

    void OnPointerDown()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        Managers.Sound.Play2D("SFX/UI_Click");
        _beginParent = transform.parent;
        _beginIndex = transform.GetSiblingIndex();
        _beginPosition = transform.position;
        _beginMousePosition = Input.mousePosition;

        transform.SetParent(transform.parent.parent);
        transform.SetAsLastSibling();
    }

    void OnPointerDrag()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        transform.position = _beginPosition + 
                          (Input.mousePosition - _beginMousePosition);
    }

    void OnPointerUp()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        // 밖으로 빼낸 상태가 아니면
        if (transform.parent != _beginParent.parent)
            return;

        Managers.Sound.Play2D("SFX/UI_Dropdown");
        transform.SetParent(_beginParent);
        transform.SetSiblingIndex(_beginIndex);
        UI_Room room = FindObjectOfType<UI_Room>();
        room.PointerUpPos(this);
    }

    void RequestPlayerInfo()
    {
        PV.RPC("SendPlayerInfo", PV.Owner, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    void SendPlayerInfo(Player requestPlayer)
    {
        int imageIndex = Managers.Data.PlayerInfoData.PlayerIcon;
        string nickName = Managers.Data.PlayerInfoData.PlayerNick;
        int win = Managers.Data.PlayerInfoData.win;
        int lose = Managers.Data.PlayerInfoData.lose;
        int draw = Managers.Data.PlayerInfoData.draw;
        string introduce = Managers.Data.PlayerInfoData.introduceText;

        PV.RPC("ReceviePlayerInfo", requestPlayer, imageIndex, nickName, win, lose, draw, introduce);
    }

    [PunRPC]
    void ReceviePlayerInfo(int imageIndex, string nickName, int win, int lose, int draw, string introduce)
    {
        var info = Managers.UI.ShowPopupUI<UI_AccountInfo>("UI_AccountInfo");

        int total = win + lose + draw;
        int winRate = total != 0 ? (int)(((float)win / total) * 100) : 0;

        info.SetAccountInfo(imageIndex, nickName, total, win, lose, draw, winRate, introduce);
    }
}
