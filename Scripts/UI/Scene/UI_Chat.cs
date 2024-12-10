using BackEnd;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Chat : UI_Base
{
    [SerializeField]
    bool Clocking = false;

    float clockingTime = 10f;
    IEnumerator CorSV;

    PhotonViewEx PV;

    bool isSelect = false;
    bool IsSelect
    {
        get
        {
            return isSelect;
        }
        set
        {
            isSelect = value;
            Managers.Input.IsControll = !value;
            InputAndBarClocking(!value);
            SVClocking(!value);

            if (value)
            {
                Get<InputField>((int)InputFields.ChatInputField).text = "";
                Get<InputField>((int)InputFields.ChatInputField).ActivateInputField();
            }
            else
            {
                Get<InputField>((int)InputFields.ChatInputField).DeactivateInputField();
            }

            if (Managers.GameCursor.defaultCursorLock && Managers.UI.PopupCount() == 0)
                Managers.GameCursor.CursorLock = !value;
        }
    }
    List<Text> texts = new List<Text>();

    int lastIndex = 0;
    int chatIndex = 0;

    enum InputFields
    {
        ChatInputField,
    }

    enum ScrollRects
    {
        ScrollView,
    }

    enum Scrollbars
    {
        ScrollbarVertical
    }

    enum CanvasGroups
    {
        ScrollView,
        ScrollbarVertical,
        ChatInputField
    }

    public override void Init()
    {
        Bind<InputField>(typeof(InputFields));
        Bind<ScrollRect>(typeof(ScrollRects));
        Bind<Scrollbar>(typeof(Scrollbars));
        Bind<CanvasGroup>(typeof(CanvasGroups));


        Get<InputField>((int)InputFields.ChatInputField).gameObject.BindEvent(data =>
        {
            if (Managers.UI.PopupCount() == 0 && Managers.UI.ComPopupCount() == 0)
                IsSelect = true;
            else
                IsSelect = false;
        });

        Get<InputField>((int)InputFields.ChatInputField).onEndEdit.AddListener(text =>
        {
            if(!Input.GetKeyDown(KeyCode.Return))
                IsSelect = false;
        });

        PV = GetComponent<PhotonViewEx>();

        var contents = Get<ScrollRect>((int)ScrollRects.ScrollView).content;
        lastIndex = contents.childCount - 1;
        for (int i = 0; i < contents.childCount; i++)
        {
            texts.Add(contents.GetChild(i).GetComponent<Text>());
        }

        if (Clocking)
        {
            Get<CanvasGroup>((int)CanvasGroups.ScrollView).alpha = 0;
            Get<CanvasGroup>((int)CanvasGroups.ScrollbarVertical).alpha = 0;
            Get<CanvasGroup>((int)CanvasGroups.ChatInputField).alpha = 0;

            CorSV = CorSVClocking();
        }

        Managers.Input.uIAction -= ChatCommand;
        Managers.Input.uIAction += ChatCommand;
    }

    private void Start()
    {
        Get<Scrollbar>((int)Scrollbars.ScrollbarVertical).value = 0.01f;
    }

    #region 채팅
    void ChatCommand()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (Managers.UI.PopupCount() != 0 || Managers.UI.ComPopupCount() != 0)
                return;

            if (!IsSelect)
            {
                IsSelect = true;
            }
            else
            {
                ChatSend();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsSelect)
            {
                InputField inputField = Get<InputField>((int)InputFields.ChatInputField);
                inputField.text = "";

                IsSelect = false;
            }
        }
    }

    public void ChatSend()
    {
        InputField inputField = Get<InputField>((int)InputFields.ChatInputField);

        if (inputField.text != "")
        {      
            string chatText = $"{Managers.Data.PlayerInfoData.PlayerNick} : {inputField.text}";
            PV.RPC("ChatReceive", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer, chatText);
            inputField.text = "";
        }

        IsSelect = false;
    }

    //public void ChatSend()
    //{
    //    if (Input.GetKeyDown(KeyCode.Return))
    //    {
    //        if (Managers.UI.ComPopupCount() != 0)
    //            return;

    //        InputField inputField = Get<InputField>((int)InputFields.ChatInputField);
    //        if (inputField.text != "")
    //        {
    //            Managers.Input.IsControll = true;
    //            string chatText = $"{Backend.UserNickName} : {inputField.text}";
    //            PV.RPC("ChatReceive", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer, chatText);

    //            inputField.text = "";
    //            //IsSelect = false;

    //            InputAndBarClocking(true);
    //            if (Managers.GameCursor.defaultCursorLock && Managers.UI.PopupCount() == 0)
    //                Managers.GameCursor.CursorLock = true;
    //        }
    //        else
    //        {
    //            if(IsSelect)
    //            {
    //                //IsSelect = false;
    //                Managers.Input.IsControll = true;

    //                SVClocking(true);
    //                InputAndBarClocking(true);

    //                if (Managers.GameCursor.defaultCursorLock && Managers.UI.PopupCount() == 0)
    //                    Managers.GameCursor.CursorLock = true;
    //            }
    //            else
    //            {
    //                IsSelect = true;
    //                Managers.Input.IsControll = false;
    //                SVClocking(false);
    //                InputAndBarClocking(false);
    //                if (Managers.GameCursor.defaultCursorLock)
    //                    Managers.GameCursor.CursorLock = false;
    //            }
    //        }
    //    }

    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        if (IsSelect)
    //        {
    //            InputField inputField = Get<InputField>((int)InputFields.ChatInputField);
    //            inputField.text = "";

    //            IsSelect = false;

    //            SVClocking(true);
    //            InputAndBarClocking(true);

    //            if (Managers.GameCursor.defaultCursorLock && Managers.UI.PopupCount() == 0)
    //                Managers.GameCursor.CursorLock = true;
    //        }
    //    }
    //}

    [PunRPC]
    void ChatReceive(Player player, string chatText)
    {
        SVClocking(false);
        SVClocking(true);

        // 빈 Text에서 아래 텍스트를 끌어 올려준다.
        for (int i = lastIndex - chatIndex; i < lastIndex; i++)
            texts[i].text = texts[i + 1].text;

        chatIndex = Mathf.Clamp(++chatIndex, 0, lastIndex);
        texts[lastIndex].text = $"<color={SetColor(player)}>{chatText}</color>";
    }

    void NoticeReceive(string chatText)
    {
        SVClocking(false);
        SVClocking(true);

        // 빈 Text에서 아래 텍스트를 끌어 올려준다.
        for (int i = lastIndex - chatIndex; i < lastIndex; i++)
        {
            texts[i].text = texts[i + 1].text;
        }

        chatIndex = Mathf.Clamp(++chatIndex, 0, lastIndex);
        texts[lastIndex].text = chatText;
    }

    string SetColor(Player player)
    {
        if (player == PhotonNetwork.LocalPlayer)
            return "lime";
        else if (player.IsMasterClient)
            return "orange";
        else
            return "white";
    }
    #endregion

    #region 채팅UI 투명화
    void SVClocking(bool isClocking)
    {
        if(Clocking)
        {
            if(isClocking)
            {
                if (CorSV == null)
                    CorSV = CorSVClocking();
                StartCoroutine(CorSV);
            }
            else
            {
                if(CorSV != null)
                {
                    StopCoroutine(CorSV);
                    CorSV = null;
                }
                Get<CanvasGroup>((int)CanvasGroups.ScrollView).alpha = 1;
            }
        }
    }

    IEnumerator CorSVClocking()
    {
        yield return new WaitForSecondsRealtime(clockingTime);
        float fadeDurtaion = 1f;
        float timer = 1f;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            Get<CanvasGroup>((int)CanvasGroups.ScrollView).alpha = timer / fadeDurtaion;
            yield return null;
        }
    }

    void InputAndBarClocking(bool isClocking)
    {
        if (Clocking)
        {
            if (isClocking)
            {
                Get<CanvasGroup>((int)CanvasGroups.ScrollbarVertical).alpha = 0;
                Get<CanvasGroup>((int)CanvasGroups.ChatInputField).alpha = 0;
            }
            else
            {
                Get<CanvasGroup>((int)CanvasGroups.ScrollbarVertical).alpha = 1;
                Get<CanvasGroup>((int)CanvasGroups.ChatInputField).alpha = 1;
            }
        }
    }

    #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        NoticeReceive($"<color=yellow>{newPlayer.NickName}님이 입장 하셨습니다.</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        NoticeReceive($"<color=yellow>{otherPlayer.NickName}님이 퇴장 하셨습니다.</color>");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        NoticeReceive($"<color=sliver>{newMasterClient.NickName}님이 방장이 되셨습니다.</color>");
    }

    private void OnDestroy()
    {
        if (Managers.Input != null)
            Managers.Input.uIAction -= ChatCommand;
        Managers.Input.IsControll = true;
    }
}
