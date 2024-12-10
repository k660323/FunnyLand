using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChoiceMap : UI_Photon, IPunObservable
{
    [HideInInspector]
    public PhotonViewEx PV;

    GameScene GS;
    
    IEnumerator CorTimer;
    int timer;
    public int Timer
    {
        get
        {
            return timer;
        }
        set
        {
            timer = value;
            Get<Text>((int)Texts.TimerText).text = value.ToString();
        }
    }

    enum Texts
    {
        WhoChoiceText,
        MapText,
        TimerText,
        RoundText
    }

    enum Images
    {
        MapImage
    }

    enum Buttons
    {
        LeftButton,
        RightButton,
        SelectButton
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Timer);
        }
        else
        {
            Timer = (int)stream.ReceiveNext();
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("CurRound"))
            Get<Text>((int)Texts.RoundText).text = $"{propertiesThatChanged["CurRound"]}/{PhotonNetwork.CurrentRoom.CustomProperties["Round"]}";
    }

    public void InitV2()
    {
        GS = Managers.Game.gameScene;

        PV = GetComponent<PhotonViewEx>();
        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
       
        Get<Button>((int)Buttons.LeftButton).onClick.AddListener(() => {
            Managers.Sound.Play2D("SFX/UI_Page");
            Btn_Click(-1); 
        });
        Get<Button>((int)Buttons.RightButton).onClick.AddListener(() => {
            Managers.Sound.Play2D("SFX/UI_Page");
            Btn_Click(1);
        });
        Get<Button>((int)Buttons.SelectButton).onClick.AddListener(() => {
            Managers.Sound.Play2D("SFX/UI_Click");
            PV.RPC("SelectMap", RpcTarget.AllViaServer, GS.MapIndex);
        });
    }

    public void UIInit()
    {
        gameObject.SetActive(true);
        Get<Text>((int)Texts.WhoChoiceText).text = PhotonNetwork.CurrentRoom.CustomProperties["MapSelect"].ToString();
        Get<Text>((int)Texts.RoundText).text = $"{PhotonNetwork.CurrentRoom.CustomProperties["CurRound"]}/{PhotonNetwork.CurrentRoom.CustomProperties["Round"]}";

        if (PhotonNetwork.IsMasterClient)
        {
            Get<Button>((int)Buttons.LeftButton).interactable = (GS.MapIndex <= 0) ? false : true;
            Get<Button>((int)Buttons.RightButton).interactable = (GS.MapIndex >= GS.MapLength) ? false : true;
            Get<Button>((int)Buttons.SelectButton).interactable = true;
        }
        else
        {
            Get<Button>((int)Buttons.LeftButton).interactable = false;
            Get<Button>((int)Buttons.RightButton).interactable = false;
            Get<Button>((int)Buttons.SelectButton).interactable = false;
        }

        UpdateChoiceMapInfo(GS.MapIndex);
        TimerSet(60);
    }

    [PunRPC]
    public void UpdateChoiceMapInfo(int mapIndex)
    {
        GS.MapIndex = mapIndex;
        Get<Text>((int)Texts.MapText).text = GS.curMap.name;
        Get<Image>((int)Images.MapImage).sprite = Managers.Resource.Load<Sprite>($"Textures/MapImage/{GS.curMap.image}");
    }

    void Btn_Click(int num)
    {
        int mapIndex = Mathf.Clamp(GS.MapIndex + num, 0, GS.MapLength - 1);
        Get<Button>((int)Buttons.LeftButton).interactable = (mapIndex <= 0) ? false : true;
        Get<Button>((int)Buttons.RightButton).interactable = (mapIndex >= GS.MapLength - 1) ? false : true;

        PV.RPC("UpdateChoiceMapInfo",RpcTarget.All, mapIndex);
    }

    [PunRPC]
    private void SelectMap(int mapIndex)
    {
        GS.MapIndex = mapIndex;

        if(CorTimer != null)
        {
            StopCoroutine(CorTimer);
            CorTimer = null;
        }

        GS.StateController((int)Define.GameState.Loading);
        gameObject.SetActive(false);
    }

    void TimerSet(int time)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (CorTimer != null)
            StopCoroutine(CorTimer);
        CorTimer = CorTimerSet(time);
        StartCoroutine(CorTimer);
    }

    IEnumerator CorTimerSet(int time)
    {
        Timer = time;
        while (true)
        {
            if (Timer == 0)
                break;

            yield return new WaitForSeconds(1f);
            Timer -= 1;
        }

        if (GS.state == Define.GameState.Choice)
            PV.RPC("SelectMap", RpcTarget.AllViaServer, GS.MapIndex);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(newMasterClient == PhotonNetwork.LocalPlayer)
        {
            if (GS.state == Define.GameState.Choice)
            {
                UIInit();
                TimerSet(Timer);
            }
        }
    }
}
