using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : EquipmentItem
{
    public WeaponItemData WeaponData { get; private set; }

    public WeaponItem(WeaponItemData data) : base(data)
    {
        WeaponData = data;
    }
}
