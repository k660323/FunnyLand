using UnityEngine;

public abstract class PlayerController3D : Controller3D
{
    public Vector3 lookForward;
    public Vector3 lookRight;
    
    public override void Init()
    {
        base.Init();

        Model = transform.Find("Model");

        if (nickNameTextMesh != null)
            nickNameTextMesh.text = PV.Owner.NickName;
        else if  (nickNameText != null)
            nickNameText.text = PV.Owner.NickName;

        if (PV.IsMine)
        {
            Managers.Input.keyAction -= UpdateState;
            Managers.Input.keyAction += UpdateState;
            Managers.Game.ContentsScene.contentFixedAction -= FixedUpdateState;
            Managers.Game.ContentsScene.contentFixedAction += FixedUpdateState;
        }
        else
        {
            Managers.Input.keyAction -= UpdateSyncValue;
            Managers.Input.keyAction += UpdateSyncValue;
        }
    }

    protected override void Input()
    {
        moveInput = new Vector2(Managers.Input.Horizontal, Managers.Input.Vertical).normalized;
        isMove = moveInput.magnitude != 0;
        if (!isMove)
        {
            moveDir = Vector3.zero;
            nextFrameMSpeed = 0f;
        }
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
