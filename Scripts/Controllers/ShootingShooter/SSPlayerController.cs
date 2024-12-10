using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSPlayerController : PlayerController2D
{
    Image hpImage;

    public override Define.State State
    {
        get { return base.State; }
        set
        {
            base.State = value;

            switch (_state)
            {
                case Define.State.Die:
                    PV.RPC("TriggerAnim", RpcTarget.AllViaServer, "isDie");
                    if (PV.IsMine)
                        Managers.Photon.SetPlayerPropertie("Die", true);
                    break;
                case Define.State.Idle:
                    anim.SetBool("isMove", false);
                    break;
                case Define.State.Moving:
                    anim.SetBool("isMove", true);
                    break;
                case Define.State.NormalAttack:
                    anim.SetFloat("AtkSpeed", stat.AttackSpeed > 1f ? stat.AttackSpeed : 1);
                    PV.RPC("TriggerAnim", RpcTarget.AllViaServer, "isAtk");
                    break;
            }
        }
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);

        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(sr.flipX);
            stream.SendNext(hpImage.fillAmount);

        }
        else
        {
            DestPos = (Vector3)stream.ReceiveNext();
            DestRot = (Quaternion)stream.ReceiveNext();
            sr.flipX = (bool)stream.ReceiveNext();
            hpImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    public override void Init()
    {
        base.Init();
        hpImage = transform.Find("Canvas").Find("HPImage").GetComponent<Image>();
        SetNickNameViewInit();
        SetTeamNickNameViewInit(true);

        if (PV.IsMine)
        {
            Managers.Input.keyAction -= UpdateState;
            Managers.Input.keyAction += UpdateState;
            Managers.Game.ContentsScene.contentFixedAction -= FixedUpdateState;
            Managers.Game.ContentsScene.contentFixedAction += FixedUpdateState;
            Managers.Game.ContentsScene.contentLateAction -= LateUpdateState;
            Managers.Game.ContentsScene.contentLateAction += LateUpdateState;
            Managers.Game.ContentsScene.SetStatUI(stat);
        }
        else
        {
            SetGetGravtiy = false;
            Managers.Input.keyAction -= UpdateSyncValue;
            Managers.Input.keyAction += UpdateSyncValue;
        }
    }

    protected override void FixedUpdateState()
    {
        FGroundCheck();
        PhysicsCheck();
    }

    protected override void LateUpdateState()
    {
        hpImage.fillAmount = stat.Hp / (float)stat.MaxHp;
    }


    protected override void UpdateState()
    {
        Input();

        switch (State)
        {
            case Define.State.Idle:
                UpdateIdle();
                break;
            case Define.State.Moving:
                UpdateMoving();
                break;
            case Define.State.NormalAttack:
                UpdateMoving();
                break;
            case Define.State.Jumping:
                UpdateJumping(); 
                break;
        }
    }

    protected override void UpdateIdle()
    {
        if (isMove)
        {
            State = Define.State.Moving;
            return;
        }

        if (moveInput.y > 0 && stat.JumpCount > 0)
        {
            State = Define.State.Jumping;
            return;
        }
        UpdateAttack();
    }

    protected override void UpdateMoving()
    {
        if (isMove)
        {
            FlipSprite(moveInput.x);
            // rb.position += new Vector2(moveInput.x * stat.MoveSpeed, moveInput.y < 0f ? moveInput.y * stat.MoveSpeed : 0) * Time.deltaTime;
            // rb.MovePosition(rb.position + new Vector2(moveInput.x * stat.MoveSpeed, moveInput.y < 0f ? moveInput.y * stat.MoveSpeed : 0) * Time.deltaTime);
            rb.velocity = new Vector2(moveInput.x * stat.MoveSpeed, rb.velocity.y);
            // transform.position += new Vector3(moveInput.x * stat.MoveSpeed, moveInput.y < 0f ? moveInput.y * stat.MoveSpeed : 0) * Time.deltaTime;
            // transform.Translate(new Vector3(moveInput.x * stat.MoveSpeed, moveInput.y < 0f ? moveInput.y * stat.MoveSpeed : 0) * Time.deltaTime);
            rb.angularVelocity = 0;
        }
        else
        {
            State = Define.State.Idle;
            return;
        }

        if (moveInput.y > 0 && stat.JumpCount > 0)
        {
            State = Define.State.Jumping;
            return;
        }
        UpdateAttack();
    }

    protected override void UpdateAttack()
    {
        if (weapon != null)
            weapon.attackAction?.Invoke();
    }

    protected override void UpdateJumping()
    {
        if (stat.JumpCount > 0)
        {
            stat.JumpCount--;

            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * stat.JumpPower, ForceMode2D.Impulse);
        }

        State = Define.State.Idle;
    }

    protected override void UpdateSyncValue()
    {
        float distance = (DestPos - transform.position).sqrMagnitude;

        if (distance < 0.005f || distance > 100f)
        {
            transform.position = DestPos;
        }
        else
        {
            transform.position = Vector2.Lerp(transform.position, DestPos, 45f * Time.deltaTime);
        }

        float t = Mathf.Clamp(20f * Time.deltaTime, 0f, 0.99999f);
        transform.rotation = Quaternion.Lerp(transform.rotation, DestRot, t);
    }
}