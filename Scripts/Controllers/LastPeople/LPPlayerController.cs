using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using Photon.Realtime;
using System.Net.Mime;

public class LPPlayerController : PlayerController3D
{
    [Header("Effect")]
    public GameObject dieEffect;
    public override Define.State State
    {
        get { return base.State; }
        set
        {
            base.State = value;

            switch (_state)
            {
                case Define.State.Die:
                    if (PV.IsMine)
                        Managers.Photon.SetPlayerPropertie("Die", true);        

                    PV.RPC("NoSyncInstantiate",RpcTarget.AllViaServer, "Effects/" + dieEffect.name, "", Vector3.up, Quaternion.identity);
                    PV.RPC("TriggerAnim", RpcTarget.AllViaServer, "isDie");
                    break;
                case Define.State.Idle:
                    anim.SetBool("isMove", false);
                    anim.SetBool("isRun", false);
                    break;
                case Define.State.Moving:
                    anim.SetBool("isMove", true);
                    anim.SetBool("isRun", false);
                    break;
                case Define.State.Run:
                    anim.SetBool("isMove", true);
                    anim.SetBool("isRun", true);
                    break;
                case Define.State.NormalAttack:
                    SetAnimSpeed("AtkSpeed", stat.AttackSpeed);
                    if (PV.IsMine)
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
        }
        else
        {
            DestPos = (Vector3)stream.ReceiveNext();
            DestRot = (Quaternion)stream.ReceiveNext();
        }
    }

    public override void Init()
    {
        base.Init();

        if (PV.IsMine)
        {
            Managers.Game.ContentsScene.SetStatUI(stat);
        }
        else
        {
            SetNickNameViewInit();
            SetTeamNickNameViewInit();
        }
    }

    protected override void FixedUpdateState()
    {
        FGroundCheck();
    }

    protected override Vector3 GetMoveDir()
    {
        if(Managers.Input.LeftAlert)
        {
            moveDir = Managers.Game.camController.prevForward * moveInput.y + Managers.Game.camController.prevRight * moveInput.x;
        }
        else
        {
            lookForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
            lookRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
            moveDir = lookForward * moveInput.y + lookRight * moveInput.x;
        }

        return moveDir;
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
            case Define.State.Run:
                UpdateRunning();
                break;
            case Define.State.NormalAttack:
                UpdateAttack();
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
            if(Managers.Input.LeftShift)
            {
                if(stat.Stamina / stat.MaxStamina > 0.3f)
                {
                    State = Define.State.Run;
                    return;
                }
            }

            State = Define.State.Moving;
            return;
        }


        stat.Stamina = Mathf.Clamp(stat.Stamina + (stat.RunCost * Time.deltaTime), 0f, stat.MaxStamina);

        if (Managers.Input.IsMouseButtonDownLeft)
        {
            State = Define.State.NormalAttack;
            //UpdateAttack();
            return;
        }

        if (Managers.Input.SpaceBar && JumpCheck())
        {
            State = Define.State.Jumping;
            return;
        }
    }

    protected override void UpdateMoving()
    {
        if (Managers.Input.LeftShift)
        {
            if (stat.Stamina / stat.MaxStamina > 0.3f)
            {
                State = Define.State.Run;
                return;
            }
        }

        if (isMove)
        {
            transform.forward = GetMoveDir();
            nextFrameMSpeed = stat.MoveSpeed;

            //rb.MovePosition(rb.position + moveDir * (isRun ? stat.RunSpeed + stat.MoveSpeed : stat.MoveSpeed)  * Time.deltaTime);
            //rb.angularVelocity = Vector3.zero;
        }
        else
        {
            State = Define.State.Idle;
            return;
        }

        stat.Stamina = Mathf.Clamp(stat.Stamina + (stat.RunCost * Time.deltaTime), 0f, stat.MaxStamina);

        if (Managers.Input.IsMouseButtonDownLeft)
        {
            State = Define.State.NormalAttack;
            return;
        }

        if (Managers.Input.SpaceBar && JumpCheck())
        {
            State = Define.State.Jumping;
            return;
        }
    }

    protected override void UpdateRunning()
    {
        if (isMove)
        {
            if (!Managers.Input.LeftShift || stat.Stamina <= 0f)
            {
                State = Define.State.Moving;
                return;
            }

            transform.forward = GetMoveDir();
            nextFrameMSpeed = stat.MoveSpeed + stat.RunSpeed;
            stat.Stamina = Mathf.Clamp(stat.Stamina - (stat.RunCost * Time.deltaTime), 0f, stat.MaxStamina);
        }
        else
        {
            State = Define.State.Idle;
            return;
        }

        if (Managers.Input.IsMouseButtonDownLeft)
        {
            //UpdateAttack();
            State = Define.State.NormalAttack;
            return;
        }

        if (Managers.Input.SpaceBar && JumpCheck())
        {
            State = Define.State.Jumping;
            return;
        }
    }

    protected override void UpdateAttack()
    {
        weapon.attackAction?.Invoke();
    }

    protected override void UpdateJumping()
    {
        crb3D.JumpTo(stat.JumpPower);

        if (isMove)
        {
            if (Managers.Input.LeftShift)
            {
                if (stat.Stamina / stat.MaxStamina > 0.3f)
                {
                    State = Define.State.Run;
                    return;
                }
            }
            State = Define.State.Moving;
        }
        else
        {
            State = Define.State.Idle;
        }
    }

    protected override void UpdateSyncValue()
    {
        //float distance = (DestPos - transform.position).magnitude;

        //float speed = nextFrameMSpeed * Time.deltaTime;

        //if(distance < speed || distance >= 5)
        //{
        //    transform.position = DestPos;
        //}
        //else
        //{
        //    transform.position = Vector3.Lerp(transform.position, DestPos, speed);
        //}

        //float t = Mathf.Clamp(20f * Time.deltaTime, 0f, 0.99999f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, DestRot, t); 

        float distance = (DestPos - transform.position).sqrMagnitude;

        if (distance < 0.000005f || distance > 100f)
        {
            transform.position = DestPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, DestPos, 35f * Time.deltaTime);
        }

        float t = Mathf.Clamp(20f * Time.deltaTime, 0f, 0.99999f);
        transform.rotation = Quaternion.Lerp(transform.rotation, DestRot, t);
    }

    protected override void DestoryObject()
    {
        Managers.Game.ObserverMode(Managers.Game.attackPlayer);
        Managers.Resource.PhotonDestroy(gameObject);
    }
}