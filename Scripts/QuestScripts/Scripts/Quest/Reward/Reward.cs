using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Reward : ScriptableObject
{
    [SerializeField]
    private Sprite icon;
    [SerializeField]
    private string description;
    [SerializeField]
    private int quantity;
    [SerializeField]
    private int tquantity;

    public Sprite Icon => icon;
    public string Description => description;
    public int Quantity => quantity;
    public int TeamQuantity => tquantity;

    public abstract void Give(Quest quest);
}
