using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_InvenItemSlot : UI_Base
{
    public int id { get; private set; }
    UI_MyInfo myInfo;

    enum Images
    {
        UI_ItemSlot,
        ItemImage
    }

    enum Buttons
    {
        ActionButton
    }

    enum Texts
    {
        ActionText
    }

    enum CanvasGroups
    {
        ToolTip
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
    }

    public void SetItemInfo(UI_MyInfo _myInfo, int _id, Data.ItemData _data, UnityAction<bool, int, Vector3> previewAction)
    {
        id = _id;
        Get<Image>((int)Images.UI_ItemSlot).color = Util.GetRatingColor(_data.Rating);
        Get<Image>((int)Images.ItemImage).sprite = Managers.Resource.ItemImageLoad(_data.Image);

        Get<Image>((int)Images.ItemImage).gameObject.BindEvent((data) => { previewAction?.Invoke(true, id, transform.position); }, Define.UIEvent.Enter);
        Get<Image>((int)Images.ItemImage).gameObject.BindEvent((data) => { previewAction?.Invoke(false, id, transform.position); }, Define.UIEvent.Exit);

        myInfo = _myInfo;

        switch (_data._ItemType)
        {
            case Define.ItemType.ICON:
                if (Managers.Data.PlayerInfoData.PlayerIcon == _data.Id)
                {
                    Get<Text>((int)Texts.ActionText).text = "Âø¿ëÁß";
                    _myInfo.WearText = Get<Text>((int)Texts.ActionText);
                }
                else
                {
                    Get<Text>((int)Texts.ActionText).text = "Âø¿ëÇÏ±â";
                }
                break;
            case Define.ItemType.STYLE:
                if (Managers.Data.PlayerInfoData.Style == _data.Id)
                {
                    Get<Text>((int)Texts.ActionText).text = "Âø¿ëÁß";
                    _myInfo.WearText = Get<Text>((int)Texts.ActionText);
                }
                else
                {
                    Get<Text>((int)Texts.ActionText).text = "Âø¿ëÇÏ±â";
                }
                break;
        }
        Get<Button>((int)Buttons.ActionButton).gameObject.BindEvent((data) => { UseItem(); });
    }

    public void UseItem()
    {
        Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D);

        if (Managers.Data.ItemDict.TryGetValue(id, out Data.ItemData value))
        {
            switch (value._ItemType)
            {
                case Define.ItemType.ICON:
                    if (Managers.Data.PlayerInfoData.PlayerIcon == value.Id)
                        UnwearingAnitem(value);
                    else
                        ItemWear(value);
                    break;
                case Define.ItemType.STYLE:
                    if (Managers.Data.PlayerInfoData.Style == value.Id)
                        UnwearingAnitem(value);
                    else
                        ItemWear(value);
                    break;
                default:
                    break;
            }
        }
    }

    public void ItemWear(Data.ItemData value)
    {
        if (myInfo.WearText != null)
            myInfo.WearText.text = "Âø¿ëÇÏ±â";

        Get<Text>((int)Texts.ActionText).text = "Âø¿ëÁß";
        myInfo.WearText = Get<Text>((int)Texts.ActionText);

        switch (value._ItemType)
        {
            case Define.ItemType.ICON:
                Managers.Data.PlayerInfoData.PlayerIcon = value.Id;
                break;
            case Define.ItemType.STYLE:
                Managers.Data.PlayerInfoData.Style = value.Id;
                break;
        }
        Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
        Managers.Data.SaveBackEndData(Managers.Data.PlayerInfoData, "PlayerInfo", "playerInfoToJson");
    }

    public void UnwearingAnitem(Data.ItemData value)
    {
            Get<Text>((int)Texts.ActionText).text = "Âø¿ëÇÏ±â";
            switch (value._ItemType)
            {
                case Define.ItemType.ICON:
                    Managers.Data.PlayerInfoData.PlayerIcon = -1;
                break;
                case Define.ItemType.STYLE:
                    Managers.Data.PlayerInfoData.Style = -1;
                break;
            }
        Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
        Managers.Data.SaveBackEndData(Managers.Data.PlayerInfoData, "PlayerInfo", "playerInfoToJson");
        }
}
