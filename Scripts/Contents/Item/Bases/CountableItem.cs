using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CountableItem : Item
{
    public CountableItemData CountableData { get; private set; }
    
    // 현재 아이템 개수
    public int Amount { get; protected set; }

    // 하나의 슬롯이 가질 수 있는 최대 개수(기본 99)
    public int MaxAmount => CountableData.MaxAmount;

    // 수량이 가득 찼는지 여부
    public bool IsMax => Amount >= CountableData.MaxAmount;

    // 개수가 없는지 여부
    public bool IsEmpty => Amount <= 0;

    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        CountableData = data;
        SetAmount(amount);
    }

    // 개수 지정(범위 제한)
    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, MaxAmount);
    }

    // 개수 추가 및 최대치 초과량 반환(초과량 없을 경우 0 )
    public int AddAmountAndGetExcess(int amount)
    {
        int nextAmount = Amount + amount;
        SetAmount(nextAmount);

        return (nextAmount > MaxAmount) ? (nextAmount - MaxAmount) : 0;
    }

    // 개수를 나누어 복제
    public CountableItem SeperateAndClone(int amount)
    {
        // 수량이 한개 이하일 경우, 복제 불가
        if (Amount <= 1)
            return null;

        if (amount > Amount - 1)
            amount = Amount - 1;

        Amount -= amount;
        return Clone(amount);
    }

    protected abstract CountableItem Clone(int amount);
}
