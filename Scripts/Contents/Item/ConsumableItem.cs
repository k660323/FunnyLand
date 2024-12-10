using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItem : CountableItem, IUsableItem
{
    public ConsumableItemData ConsumableData { get; private set; }

    public ConsumableItem(ConsumableItemData data, int amount = 1) : base(data, amount)
    {
        ConsumableData = data;
    }

    public bool Use() // 이 클래스를 상속받아서 구현하는게 좋을듯. 다양한 기능을 구현못함 여기선
    {
        // 임시 : 개수 하나 감소
        Amount--;
        // TODO
        // 1. 플레이어 스텟에 접근하여 풀피인지 죽었는지 확인
        // 2. 살아있으면 hp 증가
        // 3. 그리고 갯수 감소

        return true;
    }

    protected override CountableItem Clone(int amount)
    {
        return new ConsumableItem(CountableData as ConsumableItemData, amount);
    }
}
