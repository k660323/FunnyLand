using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZSZombieController : BotController3D
{
    [Header("Effect")]
    public GameObject dieEffect;

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

    public override Define.State State
    {
        get { return base.State; }
        set
        {
            base.State = value;
            switch (_state)
            {
                case Define.State.Die:
                    if(PV.IsMine)
                    {
                        Managers.Photon.SetPlayerPropertie("Die", true);
                    }

                    //PV.RPC("NoSyncInstantiate", RpcTarget.AllViaServer, "Effects/" + dieEffect.name, "", Vector3.up, Quaternion.identity);
                    anim.SetFloat("DieMotion", Random.Range(0, 2));
                    PV.RPC("TriggerAnim", RpcTarget.AllViaServer, "isDie");
                    break;
                case Define.State.Idle:
                    anim.SetBool("isMove", false);
                    break;
                case Define.State.Moving:
                    anim.SetBool("isMove", true);
                    break;
                case Define.State.NormalAttack:
                    SetAnimSpeed("AtkSpeed", stat.AttackSpeed);
                    anim.SetFloat("AtkMotion", Random.Range(0, 2));
                    if (PV.IsMine)
                    {
                        PV.RPC("TriggerAnim", RpcTarget.AllViaServer, "isAtk");
                    }
                    break;
            }
        }
    }

    public override void Init()
    {
        base.Init();
    }

    protected override IEnumerator AIActive()
    {
        while (true)
        {
            if (Managers.Game.isGameStart)
            {
                UpdateState();
            }
            yield return null;
        }
    }

    protected override void UpdateState()
    {
        switch (State)
        {
            case Define.State.Idle:
                UpdateIdle();
                break;
            case Define.State.Moving:
                UpdateMoving();
                break;
            case Define.State.NormalAttack:
                UpdateAttack();
                break;
        }
    }

    bool PlayerIsValid()
    {
        return Managers.Game.photonPlayerGroup.GetComponentsInChildren<PlayerController3D>().Length > 0;
    }

    bool FindTarget()
    {
        PlayerController3D[] players = Managers.Game.photonPlayerGroup.GetComponentsInChildren<PlayerController3D>();
        float minDist = float.MaxValue;
        foreach (PlayerController3D player in players)
        {
            float dist = (player.transform.position - transform.position).sqrMagnitude;
            if (minDist > dist)
            {
                target = player.transform.gameObject;
                minDist = dist;
            }
        }

        return target != null;
    }

    protected override void UpdateIdle()
    {
        if (PlayerIsValid())
        {
            State = Define.State.Moving;
        }
        else
        {
            NavMoveStop();
        }
    }

    protected override void UpdateMoving()
    {
        if(FindTarget())
        {
            if ((target.transform.position - transform.position).sqrMagnitude < 5f)
            {
                transform.LookAt(target.transform);
                State = Define.State.NormalAttack;
            }
            else
            {
                NavMoveStart(Define.NavMoveDest.Target, stat.MoveSpeed);
            }
        }
        else
        {
            State = Define.State.Idle;
        }
    }

    protected override void UpdateAttack()
    {
        NavMoveStop();

        weapon.attackAction?.Invoke();
    }

    protected override void FixedUpdateState()
    {
        FGroundCheck();
    }

    protected override void UpdateSyncValue()
    {
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
}
