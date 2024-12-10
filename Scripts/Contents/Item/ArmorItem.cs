using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorItem : EquipmentItem
{
    public ArmorItemData ArmorData { get; private set; }

    public ArmorItem(ArmorItemData data) : base(data)
    {
        ArmorData = data;
    }
}
