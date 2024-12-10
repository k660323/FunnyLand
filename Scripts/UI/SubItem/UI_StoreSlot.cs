using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_StoreSlot : UI_Base
{
    public int id { get; private set; }

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

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
    }

    public void SetItemInfo(int _id, Data.ItemData _ItemData, UnityAction<bool, int, Vector3> previewAction, UnityAction<int,Text> purchaseAction)
    {
        id = _id;
        Get<Image>((int)Images.UI_ItemSlot).color = Util.GetRatingColor(_ItemData.Rating);
        Get<Image>((int)Images.ItemImage).sprite = Managers.Resource.ItemImageLoad(_ItemData.Image);

        Get<Image>((int)Images.ItemImage).gameObject.BindEvent((data) => { previewAction?.Invoke(true, id, transform.position); }, Define.UIEvent.Enter);
        Get<Image>((int)Images.ItemImage).gameObject.BindEvent((data) => { previewAction?.Invoke(false, id, transform.position); }, Define.UIEvent.Exit);

        if (Managers.Data.PlayerInventoryData.itemId.Contains(_ItemData.Id))
            Get<Text>((int)Texts.ActionText).text = $"보유중";
        else
        {
            if (Define.GoodsType.NONE == _ItemData.Goods)
            {
                Get<Text>((int)Texts.ActionText).text = "구매 불가";
            }
            else
            {
                Get<Text>((int)Texts.ActionText).text = $"{_ItemData.Price} {Util.GetGoodsTypeToString(_ItemData.Goods)}";
                Get<Button>((int)Buttons.ActionButton).gameObject.BindEvent((data) => {
                    Managers.Sound.Play2D("SFX/UI_Click");
                    purchaseAction?.Invoke(id, Get<Text>((int)Texts.ActionText)); 
                });
            }
        }
    }
}
