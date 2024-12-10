using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();

        return component;
    }
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        return null;
    }

    public static Color GetRatingColor(Define.ItemRating itemRating)
    {
        switch(itemRating)
        {
            case Define.ItemRating.Common:
                return Color.grey;
            case Define.ItemRating.Normal:
                return Color.white;
            case Define.ItemRating.Rare:
                return Color.blue;
            case Define.ItemRating.Unique:
                return Color.magenta;
            case Define.ItemRating.Legend:
                return Color.red;
           case Define.ItemRating.Epic:
                return Color.yellow;
            default:
                return Color.grey;
        }
    }

    public static string GetRatingColorToString(Define.ItemRating itemRating,string name)
    {
        switch (itemRating)
        {
            case Define.ItemRating.Common:
                return $"<color=grey>{name}</color>";
            case Define.ItemRating.Normal:
                return $"<color=white>{name}</color>";
            case Define.ItemRating.Rare:
                return $"<color=blue>{name}</color>";
            case Define.ItemRating.Unique:
                return $"<color=magenta>{name}</color>";
            case Define.ItemRating.Legend:
                return $"<color=red>{name}</color>";
            case Define.ItemRating.Epic:
                return $"<color=yellow>{name}</color>";
            default:
                return $"<color=grey>{name}</color>";
        }
    }

    public static string GetRatingToString(Define.ItemRating itemRating)
    {
        switch (itemRating)
        {
            case Define.ItemRating.Common:
                return $"Ŀ��";
            case Define.ItemRating.Normal:
                return $"�븻";
            case Define.ItemRating.Rare:
                return $"����";
            case Define.ItemRating.Unique:
                return $"����ũ";
            case Define.ItemRating.Legend:
                return $"������";
            case Define.ItemRating.Epic:
                return $"����";
            default:
                return $"Ŀ��";
        }
    }

    public static string GetItemTypeToString(Define.ItemType itemType)
    {
        switch(itemType)
        {
            case Define.ItemType.WEAPON:
                return "����";
            case Define.ItemType.ARMOR:
                return "Ȱ";
            case Define.ItemType.CONSUMABLE:
                return "�Ҹ�ǰ";
            case Define.ItemType.INSTALLABLE:
                return "��ġ��";
            case Define.ItemType.ICON:
                return "������";
            case Define.ItemType.STYLE:
                return "Īȣ";
            default:
                return "��Ÿ";
        }
    }

    public static string GetGoodsTypeToString(Define.GoodsType goodsType)
    {
        switch(goodsType)
        {
            case Define.GoodsType.COIN:
                return "����";
            case Define.GoodsType.DIA:
                return "���̾�";
            default:
                return "���� �Ұ�";
        }
    }

    public static bool PurchaseItemResult(Define.GoodsType goosType,int itemPrice)
    {
        switch(goosType)
        {
            case Define.GoodsType.COIN:
                if (itemPrice <= Managers.Data.PlayerInfoData.coin)
                {
                    Managers.Data.PlayerInfoData.coin -= itemPrice;
                    Managers.Data.SaveBackEndData(Managers.Data.PlayerInfoData, "PlayerInfo", "playerInfoToJson");
                    Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
                    return true;
                }
                break;
            case Define.GoodsType.DIA:
                if (itemPrice <= Managers.Data.PlayerInfoData.dia)
                {
                    Managers.Data.PlayerInfoData.dia -= itemPrice;
                    Managers.Data.SaveBackEndData(Managers.Data.PlayerInfoData, "PlayerInfo", "playerInfoToJson");
                    Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
                    return true;
                }
                break;
        }

        return false;
    }

    public static bool PurchaseItemCheck(Define.GoodsType goosType, int itemPrice)
    {
        switch (goosType)
        {
            case Define.GoodsType.COIN:
                if (itemPrice <= Managers.Data.PlayerInfoData.coin)
                    return true;
                break;
            case Define.GoodsType.DIA:
                if (itemPrice <= Managers.Data.PlayerInfoData.dia)
                    return true;
                break;
        }

        return false;
    }

    public static UI_Notice SimplePopup(string _content)
    {
        UI_Notice notice = Managers.UI.ShowPopupUI<UI_Notice>("UI_Notice");
        notice.SetContent(_content);

        return notice;
    }
}
