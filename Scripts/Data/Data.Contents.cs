using BackEnd;
using LitJson;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

namespace Data
{
    #region Stat

    [Serializable]
    public class StatInfo
    {
        public int level;
        public int maxHp;
        public int attack;
        public int totalExp;
    }

    [Serializable]
    public class StatData : ILoader<int, StatInfo>
    {
        public List<StatInfo> stats = new List<StatInfo>();

        public Dictionary<int, StatInfo> MakeDict()
        {
            Dictionary<int, StatInfo> dict = new Dictionary<int, StatInfo>();
            foreach (StatInfo stat in stats)
                dict.Add(stat.level, stat);
            return dict;
        }
    }

    #endregion

    #region PlayerInfo
    [Serializable]
    public class PlayerInfo
    {
        public int playerIcon = -1;
        public int styleIndex = -1;
        public string styleName;

        public int coin;
        public int dia;
        public int win;
        public int lose;
        public int draw;

        public string introduceText;

        public int Total
        {
            get
            {
                return win + lose + draw;
            }
        }

        public int WinRate
        {
            get
            {
                if (Total == 0)
                    return 0;
                return (int)(((float)win / Total) * 100);
            }
        }

        public int PlayerIcon
        {
            get
            {
                return playerIcon;
            }
            set
            {
                playerIcon = value;
                if (Managers.Data.ItemDict.TryGetValue(value, out ItemData data))
                    Managers.Photon.SetPlayerPropertie("IconImage", data.Image);
                else
                    Managers.Photon.SetPlayerPropertie("IconImage", "Help");
            }
        }

        public int Style
        {
            get
            {
                return styleIndex;
            }
            set
            {
                styleIndex = value;
                if (Managers.Data.ItemDict.TryGetValue(value, out ItemData itemData))
                    styleName = Util.GetRatingColorToString(itemData.Rating, $"{itemData.Name}");
                else
                    styleName = "";
            }
        }

        public string PlayerNick
        {
            get
            {
                return styleName + Backend.UserNickName;
            }
            set
            {
                PhotonNetwork.NickName = value;
            }
        }
    }

    [SerializeField]
    public class PlayerInventory
    {
        public List<int> itemId = new List<int>();
    }
    #endregion

    #region MapData
    [Serializable]
    public class Map
    {
        public int number;
        public string image;
        public string sceneName;
        public string name;
        public string type;
        public string comment;
        public string shortcutKeys;
        public int difficult;
    }

    [Serializable]
    public class MapData : ILoader<int, Map>
    {
        public List<Map> maps = new List<Map>();

        public Dictionary<int, Map> MakeDict()
        {
            Dictionary<int, Map> dict = new Dictionary<int, Map>();
            foreach (Map map in maps)
                dict.Add(map.number, map);
            return dict;
        }
    }
    #endregion

    #region Item
    [Serializable]
    public abstract class ItemData
    {
        public int Id;
        public string Name;
        public string Image;
        public string Tooltip;
        public int Price;

        public ItemRating Rating;
        public GoodsType Goods;
        public ItemType _ItemType;

        public abstract Item CreateItem();
    }

    [Serializable]
    public abstract class EquipmentItemData : ItemData
    {
        public bool InfinityDurability;
        public float MaxDurability { get; private set; } = 100;
        public int ReinforcementFigures;
    }

    [Serializable]
    public abstract class CountableItemData : ItemData
    {
        public int MaxAmount { get; private set; } = 99;
    }

    [Serializable]
    public class WeaponItemData : EquipmentItemData
    {
        public WeaponType _WeaponType;
        public int Damage;
        public float AttackRate;
        public float Critical;

        public override Item CreateItem()
        {
            return new WeaponItem(this);
        }
    }

    [Serializable]
    public class ArmorItemData : EquipmentItemData
    {
        public ArmorType _ArmorType;
        public int HP;
        public int MP;
        public int Defense;
        public int AttackSpeed;
        public int MoveSpeed;
        public int JumpPower;

        public override Item CreateItem()
        {
            return new ArmorItem(this);
        }
    }

    [Serializable]
    public class ConsumableItemData : CountableItemData
    {
        public ConsumableType _ConsumableType;

        public float Duration;
        public float[] Value { get; private set; }

        public override Item CreateItem()
        {
            switch(_ConsumableType)
            {
                case ConsumableType.HPPOTION:
                    return new ConsumableItem(this); // ConsumableItem 상속받는 클래스를 생성 
                default:
                    return new ConsumableItem(this);
            }
        }
    }

    [Serializable]
    public class EtcItemData : CountableItemData
    {
        public override Item CreateItem()
        {
            return new EtcItem(this);
        }
    }

    [Serializable]
    public class IconData : EquipmentItemData
    {
        public override Item CreateItem()
        {
            return null;
        }
    }

    [Serializable]
    public class StyleData : EquipmentItemData
    {
        public override Item CreateItem()
        {
            return null;
        }
    }


    [Serializable]
    public class ItemLoader : ILoader<int,ItemData>
    {
        public List<WeaponItemData> weapons = new List<WeaponItemData>();
        public List<ArmorItemData> armors = new List<ArmorItemData>();
        public List<ConsumableItemData> consumables = new List<ConsumableItemData>();
        public List<EtcItemData> etcs = new List<EtcItemData>();
        public List<IconData> icons = new List<IconData>();
        public List<StyleData> styles = new List<StyleData>();

        public Dictionary<int, ItemData>MakeDict()
        {
            Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
            foreach(ItemData item in weapons)
            {
                item._ItemType = ItemType.WEAPON;
                dict.Add(item.Id, item);
            }
            foreach (ItemData item in armors)
            {
                item._ItemType = ItemType.ARMOR;
                dict.Add(item.Id, item);
            }
            foreach (ItemData item in consumables)
            {
                item._ItemType = ItemType.CONSUMABLE;
                dict.Add(item.Id, item);
            }
            foreach (ItemData item in etcs)
            {
                item._ItemType = ItemType.NONE;
                dict.Add(item.Id, item);
            }
            foreach (ItemData item in icons)
            {
                item._ItemType = ItemType.ICON;
                dict.Add(item.Id, item);
            }
            foreach (ItemData item in styles)
            {
                item._ItemType = ItemType.STYLE;
                dict.Add(item.Id, item);
            }

            return dict;
        }
    }

    #endregion
}