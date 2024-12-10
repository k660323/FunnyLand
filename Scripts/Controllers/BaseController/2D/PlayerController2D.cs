using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController2D : Controller2D
{
    public override void Init()
    {
        base.Init();

        if (nickNameTextMesh != null)
            nickNameTextMesh.text = PV.Owner.NickName;
        else if (nickNameText != null)
            nickNameText.text = PV.Owner.NickName;
    }

    protected override void Input()
    {
        moveInput = new Vector2(Managers.Input.Horizontal, Managers.Input.Vertical).normalized;
        isMove = moveInput.magnitude != 0;
        if (!isMove)
            moveDir = Vector3.zero;
    }

    protected override void OnDestroy()
    {
        if (PV != null)
        {
            if (PV.IsMine)
                Managers.Input.keyAction -= UpdateState;
            else
                Managers.Input.keyAction -= UpdateSyncValue;
        }

        if (Managers.Game.ContentsScene != null)
        {
            Managers.Game.ContentsScene.contentFixedAction -= FixedUpdateState;
            Managers.Game.ContentsScene.contentFixedAction -= LateUpdateState;
        }
    }
}
