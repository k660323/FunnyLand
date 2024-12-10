using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ToolTip : UI_Base
{
    public enum Images
    {
        ItemImage
    }

    public enum Texts
    {
        ItemNameText,
        ItemRatingTypeText,
        ItemTypeText,
        ToolTipText
    }
    enum CanvasGroups
    {
        UI_ToolTip
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<CanvasGroup>(typeof(CanvasGroups));
    }

    public void ShowItemInfo(bool isOn, int id, Vector3 pos)
    {
        if (isOn)
        {
            if (Managers.Data.ItemDict.TryGetValue(id, out Data.ItemData value))
            {
                Get<CanvasGroup>((int)CanvasGroups.UI_ToolTip).transform.position = pos;
                Get<Image>((int)Images.ItemImage).sprite = Managers.Resource.ItemImageLoad(value.Image);
                Get<Text>((int)Texts.ItemNameText).text = Util.GetRatingColorToString(value.Rating, value.Name);
                Get<Text>((int)Texts.ItemRatingTypeText).text = Util.GetRatingColorToString(value.Rating, Util.GetRatingToString(value.Rating));
                Get<Text>((int)Texts.ItemTypeText).text = Util.GetItemTypeToString(value._ItemType);
                Get<Text>((int)Texts.ToolTipText).text = value.Tooltip;
                Get<CanvasGroup>((int)CanvasGroups.UI_ToolTip).alpha = 1;
            }
        }
        else
            Get<CanvasGroup>((int)CanvasGroups.UI_ToolTip).alpha = 0;
    }
}
