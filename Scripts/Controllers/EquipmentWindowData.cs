using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentWindowData : MonoBehaviour
{
    List<int> initEquipList = new List<int>();
    Stat stat;

    [SerializeField]
    int WeaponItemId = -1;
    [SerializeField]
    int HELMETItemId = -1;
    [SerializeField]
    int TOPItemId = -1;
    [SerializeField]
    int BOTTOMItemId = -1;
    [SerializeField]
    int BOOTSItemId = -1;
    [SerializeField]
    int EARRINGItemId = -1;
    [SerializeField]
    int RINGItemId = -1;
    [SerializeField]
    int CLOAKItemId = -1;
    [SerializeField]
    int ShieldItemId = -1;

    // 무기
    public WeaponItem WEAPONItem { get; private set; }
    // 방어구 
    public ArmorItem HELMETItem { get; private set; }
    public ArmorItem TOPItem { get; private set; }
    public ArmorItem BOTTOMItem { get; private set; }
    public ArmorItem BOOTSItem { get; private set; }
    public ArmorItem EARRINGItem { get; private set; }
    public ArmorItem RINGItem { get; private set; }
    public ArmorItem CLOAKItem { get; private set; }
    public ArmorItem ShieldItem { get; private set; }

    public void Init(Stat _stat)
    {
        stat = _stat;

        EquipInit();
    }

    public void EquipInit()
    {
        if (WeaponItemId != -1)
            initEquipList.Add(WeaponItemId);
        if (HELMETItemId != -1)
            initEquipList.Add(HELMETItemId);
        if (TOPItemId != -1)
            initEquipList.Add(TOPItemId);
        if (BOTTOMItemId != -1)
            initEquipList.Add(BOTTOMItemId);
        if (BOOTSItemId != -1)
            initEquipList.Add(BOOTSItemId);
        if (EARRINGItemId != -1)
            initEquipList.Add(EARRINGItemId);
        if (RINGItemId != -1)
            initEquipList.Add(RINGItemId);
        if (CLOAKItemId != -1)
            initEquipList.Add(CLOAKItemId);
        if (ShieldItemId != -1)
            initEquipList.Add(ShieldItemId);

        for (int i = 0; i < initEquipList.Count; i++)
        {
            if (Managers.Data.ItemDict.TryGetValue(initEquipList[i], out Data.ItemData value))
            {
                Item item = value.CreateItem();
                EquipItem(item);
            }
        }
    }

    public Item EquipItem(Item item)
    {
        Item preItem = null;
        switch(item.Data._ItemType)
        {
            case Define.ItemType.WEAPON:
                preItem = this.WEAPONItem;
                WEAPONItem = item as WeaponItem;
                WearWeaponItem(WEAPONItem);
                break;
            case Define.ItemType.ARMOR:
                ArmorItem armorItem = item as ArmorItem;
                switch(armorItem.ArmorData._ArmorType)
                {
                    case Define.ArmorType.HELMET:
                        preItem = HELMETItem;
                        HELMETItem = armorItem;
                        break;
                    case Define.ArmorType.TOP:
                        preItem = TOPItem;
                        TOPItem = armorItem;
                        break;
                    case Define.ArmorType.BOTTOM:
                        preItem = BOTTOMItem;
                        BOTTOMItem = armorItem;
                        break;
                    case Define.ArmorType.BOOTS:
                        preItem = BOOTSItem;
                        BOOTSItem = armorItem;
                        break;
                    case Define.ArmorType.EARRING:
                        preItem = EARRINGItem;
                        EARRINGItem = armorItem;
                        break;
                    case Define.ArmorType.RIGN:
                        preItem = RINGItem;
                        RINGItem = armorItem;
                        break;
                    case Define.ArmorType.CLOAK:
                        preItem = CLOAKItem;
                        CLOAKItem = armorItem;
                        break;
                    case Define.ArmorType.Shield:
                        preItem = ShieldItem;
                        ShieldItem = armorItem;
                        break;
                }
                WearArmorItem(armorItem);
                break;
        }

        return preItem;
    }
    void WearWeaponItem(WeaponItem item)
    {
        stat.Attack += item.WeaponData.Damage;
        stat.AttackSpeed += item.WeaponData.AttackRate;
        stat.Critical += item.WeaponData.Critical;
    }
    void WearArmorItem(ArmorItem item)
    {
        stat.MaxHp += item.ArmorData.HP;
        stat.MaxMp += item.ArmorData.MP;
        stat.Defense += item.ArmorData.Defense;
        stat.AttackSpeed += item.ArmorData.AttackSpeed;
        stat.MoveSpeed += item.ArmorData.MoveSpeed;
        stat.JumpPower += item.ArmorData.JumpPower;
    }

    public Item DeEquipItem(Item item)
    {
        switch (item.Data._ItemType)
        {
            case Define.ItemType.WEAPON:
                WeaponItem weaponItem = item as WeaponItem;
                return UnWearWeaponItem(weaponItem);
            case Define.ItemType.ARMOR:
                ArmorItem armorItem = item as ArmorItem;
                switch (armorItem.ArmorData._ArmorType)
                {
                    case Define.ArmorType.HELMET:
                        HELMETItem = armorItem;
                        break;
                    case Define.ArmorType.TOP:
                        TOPItem = armorItem;
                        break;
                    case Define.ArmorType.BOTTOM:
                        BOTTOMItem = armorItem;
                        break;
                    case Define.ArmorType.BOOTS:
                        BOOTSItem = armorItem;
                        break;
                    case Define.ArmorType.EARRING:
                        EARRINGItem = armorItem;
                        break;
                    case Define.ArmorType.RIGN:
                        RINGItem = armorItem;
                        break;
                    case Define.ArmorType.CLOAK:
                        CLOAKItem = armorItem;
                        break;
                    case Define.ArmorType.Shield:
                        ShieldItem = armorItem;
                        break;
                }
                return UnWearArmorItem(armorItem);
        }

        return null;
    }
    Item UnWearWeaponItem(WeaponItem item)
    {
        stat.Attack -= item.WeaponData.Damage;
        stat.AttackSpeed -= item.WeaponData.AttackRate;
        stat.Critical -= item.WeaponData.Critical;

        return item;
    }
    Item UnWearArmorItem(ArmorItem item)
    {
        stat.MaxHp -= item.ArmorData.HP;
        stat.MaxMp -= item.ArmorData.MP;
        stat.Defense -= item.ArmorData.Defense;
        stat.AttackSpeed -= item.ArmorData.AttackSpeed;
        stat.MoveSpeed -= item.ArmorData.MoveSpeed;
        stat.JumpPower -= item.ArmorData.JumpPower;

        return item;
    }
}
