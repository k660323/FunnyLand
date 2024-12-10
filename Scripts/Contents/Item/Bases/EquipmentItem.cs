using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItem : Item
{
    public EquipmentItemData EquipmentData { get; private set; }
    // 내구성 무한인지
    private bool _infinityDurability;
    public bool InfinityDurability
    {
        get => _infinityDurability;
        set
        {
            _infinityDurability = value;
        }
    }
    // 현재 내구도
    private float _durability;
    public float Durability
    {
        get => _durability;
        set
        {
            if (value < 0) value = 0;
            if (value > EquipmentData.MaxDurability)
                value = EquipmentData.MaxDurability;

            _durability = value;
        }
    }

    public int reinforcementFigures;

    public EquipmentItem(EquipmentItemData data) : base(data)
    {
        EquipmentData = data;
        InfinityDurability = data.InfinityDurability;
        Durability = data.MaxDurability;
        reinforcementFigures = data.ReinforcementFigures;
    }
}
