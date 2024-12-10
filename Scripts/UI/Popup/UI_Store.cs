using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Store : UI_Popup
{
    enum Toggles
    {
        IconToggle,
        StyleToggle,
        EtcToggle
    }

    enum Buttons
    {
        ExitButton
    }

    public enum Texts
    {
        CoinText,
        DiaText
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
        Bind<Toggle>(typeof(Toggles));
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<Transform>(typeof(Contents));
        Bind<UI_ToolTip>(typeof(UI_ToolTips));

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
        Get<Button>((int)Buttons.ExitButton).onClick.AddListener(() => {
            Managers.Sound.Play2D("SFX/UI_Click");
            ClosePopupUI(); 
        });

        Get<Text>((int)Texts.CoinText).text = Managers.Data.PlayerInfoData.coin.ToString();
        Get<Text>((int)Texts.DiaText).text = Managers.Data.PlayerInfoData.dia.ToString();

        CreateItem(Toggles.IconToggle);

        Managers.UI.SceneUI.UpdateSyncUI += SyncAsset;
        Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
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
            Managers.Resource.Destroy(Get<Transform>((int)Contents.Content).GetChild(length -1 -i).gameObject);
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
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (Managers.Data.ItemDict.TryGetValue(i, out Data.ItemData value))
            {
                UI_StoreSlot slot = Managers.Resource.Instantiate("UI/SubItem/UI_ItemSlot", parent).GetOrAddComponent<UI_StoreSlot>();
                slot.SetItemInfo(i, value, Get<UI_ToolTip>((int)UI_ToolTips.UI_ToolTip).ShowItemInfo, ShowPurchaseWindow);
            }
            else
                break;
        }
    }

    public void ShowPurchaseWindow(int id, Text priceText)
    {
        if (Managers.Data.ItemDict.TryGetValue(id, out Data.ItemData _ItemData))
        {
            UI_Notice notice = Util.SimplePopup($"{_ItemData.Name}을 구매하시겠습니다.\n 구매비용은 {_ItemData.Price} {Util.GetGoodsTypeToString(_ItemData.Goods)} 입니다.");
            notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) =>
            {
                Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D);

                if(Util.PurchaseItemResult(_ItemData.Goods, _ItemData.Price))
                {
                    gameObject.RemoveAllEvent();
                    notice.ClosePopupUI();
                    Managers.Data.PlayerInventoryData.itemId.Add(_ItemData.Id);
                    Managers.Data.SaveBackEndData(Managers.Data.PlayerInventoryData, "PlayerInventory", "playerInventoryToJson");
                    Managers.Sound.Play2D("SFX/UI_PurchaseCompleted");
                    Util.SimplePopup("구입이 완료되었습니다.");
                    priceText.text = $"보유중";
                }
                else
                {
                    Managers.Sound.Play2D("SFX/UI_AlertSound");
                    Util.SimplePopup("비용이 부족합니다.");
                }
            });
            notice.ActiveCancelBtn();
        }
    }

    private void OnDestroy()
    {
        Managers.UI.SceneUI.UpdateSyncUI -= SyncAsset;
    }
}
