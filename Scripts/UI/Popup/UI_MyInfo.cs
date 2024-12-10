using BackEnd;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MyInfo : UI_Popup
{
    public Text WearText;

    public enum Images
    {
        IconImage,
    }

    public enum Texts
    {
        NickText,
        RaceResultText,
        CoinText,
        DiaText
    }

    enum InputFields
    {
        IntroduceInputField,
    }

    enum Buttons
    {
        CloseButton,
        EditButton,
        ChangeAccountButton
    }

    enum Toggles
    {
        IconToggle,
        StyleToggle,
        EtcToggle
    }

    enum Contents
    {
        Content
    }

    enum UI_ToolTips
    {
        UI_ToolTip
    }

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));
        Bind<Toggle>(typeof(Toggles));
        Bind<Transform>(typeof(Contents));
        Bind<UI_ToolTip>(typeof(UI_ToolTips));

        Get<Text>((int)Texts.RaceResultText).text = $"ÀüÀû\n{Managers.Data.PlayerInfoData.Total}Àü {Managers.Data.PlayerInfoData.win}½Â {Managers.Data.PlayerInfoData.lose}ÆÐ\n ½Â·ü {Managers.Data.PlayerInfoData.WinRate}%";
        Get<InputField>((int)InputFields.IntroduceInputField).text = Managers.Data.PlayerInfoData.introduceText;
        Get<Text>((int)Texts.CoinText).text = Managers.Data.PlayerInfoData.coin.ToString();
        Get<Text>((int)Texts.DiaText).text = Managers.Data.PlayerInfoData.dia.ToString();

        Get<Button>((int)Buttons.EditButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            ShowEdit(); 
        });
        Get<Button>((int)Buttons.ChangeAccountButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            Managers.UI.ShowPopupUI<UI_ChangeAccount>("ChangeAccount"); 
        });

        Get<Toggle>((int)Toggles.IconToggle).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click"); 
            CreateItem(Toggles.IconToggle); 
        });
        Get<Toggle>((int)Toggles.StyleToggle).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            CreateItem(Toggles.StyleToggle); 
        });
        Get<Toggle>((int)Toggles.EtcToggle).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            CreateItem(Toggles.EtcToggle);
        });

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            ClosePopupUI(); 
        });

        CreateItem(Toggles.IconToggle);

        Managers.UI.SceneUI.UpdateSyncUI += SyncImage;
        Managers.UI.SceneUI.UpdateSyncUI += SyncNick;
        Managers.UI.SceneUI.UpdateSyncUI += SyncAsset;
        Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
    }

    void SyncImage()
    {
        Get<Image>((int)Images.IconImage).sprite = Managers.Resource.ItemImageLoad(PhotonNetwork.LocalPlayer.CustomProperties["IconImage"].ToString());
    }

    void SyncNick()
    {
        Get<Text>((int)Texts.NickText).text = Managers.Data.PlayerInfoData.PlayerNick;
    }

    void SyncAsset()
    {
        Get<Text>((int)Texts.CoinText).text = Managers.Data.PlayerInfoData.coin.ToString();
        Get<Text>((int)Texts.DiaText).text = Managers.Data.PlayerInfoData.dia.ToString();
    }

    void ClearItem()
    {
        int length = Get<Transform>((int)Contents.Content).childCount;
        for (int i = 0; i < length; i++)
            Managers.Resource.Destroy(Get<Transform>((int)Contents.Content).GetChild(length - 1 - i).gameObject);
    }

    void CreateItem(Toggles toggle)
    {
        ClearItem();
        switch (toggle)
        {
            case Toggles.IconToggle: // 3000 ~ 3999
                CreateSlot(3000, 3999);
                break;
            case Toggles.StyleToggle: // 4000 ~ 4999
                CreateSlot(4000, 4999);
                break;
            case Toggles.EtcToggle: // 5000 ~ 5999
                CreateSlot(5000, 5999);
                break;
        }
    }

    void CreateSlot(int startIndex, int endIndex)
    {
        Transform parent = Get<Transform>((int)Contents.Content);

        for (int i = 0; i < Managers.Data.PlayerInventoryData.itemId.Count; i++)
        {
            if (startIndex <= Managers.Data.PlayerInventoryData.itemId[i] && Managers.Data.PlayerInventoryData.itemId[i] <= endIndex)
            {
                if (Managers.Data.ItemDict.TryGetValue(Managers.Data.PlayerInventoryData.itemId[i], out Data.ItemData value))
                {
                    UI_InvenItemSlot slot = Managers.Resource.Instantiate("UI/SubItem/UI_ItemSlot", parent).GetOrAddComponent<UI_InvenItemSlot>();
                    slot.SetItemInfo(this, Managers.Data.PlayerInventoryData.itemId[i], value, Get<UI_ToolTip>((int)UI_ToolTips.UI_ToolTip).ShowItemInfo);
                }
            }
        }
    }

    void ShowEdit()
    {
        var editUI = Managers.UI.ShowPopupUI<UI_EditIntroduce>("UI_EditIntroduce");
        editUI.SetAction(EditIntroduce);
    }

    void EditIntroduce(string content)
    {
        Managers.Data.PlayerInfoData.introduceText = content;
        Managers.Data.SaveBackEndData(Managers.Data.PlayerInfoData, "PlayerInfo", "playerInfoToJson");
        Get<InputField>((int)InputFields.IntroduceInputField).text = content;
    }

    private void OnDestroy()
    {
        try
        {
            Managers.UI.SceneUI.UpdateSyncUI -= SyncImage;
            Managers.UI.SceneUI.UpdateSyncUI -= SyncNick;
            Managers.UI.SceneUI.UpdateSyncUI -= SyncAsset;
        }
        catch
        {

        }
    }
}
