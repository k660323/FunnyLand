using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_Scene : UI_Base
{
    public UnityAction UpdateSyncUI;

    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, false);
    }
}
