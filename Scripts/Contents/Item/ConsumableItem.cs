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

    public bool Use() // �� Ŭ������ ��ӹ޾Ƽ� �����ϴ°� ������. �پ��� ����� �������� ���⼱
    {
        // �ӽ� : ���� �ϳ� ����
        Amount--;
        // TODO
        // 1. �÷��̾� ���ݿ� �����Ͽ� Ǯ������ �׾����� Ȯ��
        // 2. ��������� hp ����
        // 3. �׸��� ���� ����

        return true;
    }

    protected override CountableItem Clone(int amount)
    {
        return new ConsumableItem(CountableData as ConsumableItemData, amount);
    }
}
