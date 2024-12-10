using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EtcItem : CountableItem
{
    public EtcItemData EtcData { get; private set; }

    public EtcItem(EtcItemData data, int amount = 1) : base(data, amount)
    {
        EtcData = data;
    }

    protected override CountableItem Clone(int amount)
    {
        return new EtcItem(CountableData as EtcItemData, amount);
    }
}