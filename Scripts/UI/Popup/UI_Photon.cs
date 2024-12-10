using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Photon : UI_Base
{
    [SerializeField]
    protected int sort;
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, sort);
    }
}
