using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LPBotController : BotController3D
{
    [Header("Effect")]
    public GameObject dieEffect;

    [SerializeField]
    [Min(0)]
    float randMove;

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);

        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(nextFrameMSpeed);
        }
        else
        {
            DestPos = (Vector3)stream.ReceiveNext();
            DestRot = (Quaternion)stream.ReceiveNext();
            nextFrameMSpeed = (float)stream.ReceiveNext();
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
                    PV.RPC("NoSyncInstantiate", RpcTarget.AllViaServer, "Effects/" + dieEffect.name, "", Vector3.up, Quaternion.identity);
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
                    anim.SetBool("isRun", true);
                    anim.SetBool("isMove", true);
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
            if(Managers.Game.isGameStart && actionroutine == null)//&& !isAction)
            {
                yield return new WaitForSeconds(Random.Range(0f, thinkTime));
                UpdateState();
            }
            yield return null;
        }
    }

    protected override void UpdateState()
    {
        int index = Random.Range(0, 100);
        
        if(index <= 50) // 아이들 50가지 경우의 수
            UpdateIdle();
        else if(index <= 90) // 워크 40가지 경우의 수
            UpdateMoving();
        else // 런 10가지 경우의 수
            UpdateRunning();
    }

    protected override void Input()
    {
        ray.origin = new Vector3(transform.position.x + Random.Range(-randMove, randMove), transform.position.y + Managers.Game.defaultPointHeight, transform.position.z + Random.Range(-randMove, randMove));
        ray.direction = Vector3.down;
    }

    protected override void UpdateIdle()
    {
        NavMoveStop();
        State = Define.State.Idle;
    }

    protected override void UpdateMoving()
    {
        Input();

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, GWMask))
        {
            _destPos = hit.point;

            nav.CalculatePath(_destPos, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                if(NavMoveStart(Define.NavMoveDest.Point, stat.MoveSpeed))
                {
                    State = Define.State.Moving;
                    actionroutine = DistanceCheck();
                    StartCoroutine(actionroutine);
                }
            }
        }
    }

    protected override void UpdateRunning()
    {
        Input();

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, GWMask))
        {
            _destPos = hit.point;
            nav.CalculatePath(_destPos, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                if (NavMoveStart(Define.NavMoveDest.Point, stat.RunSpeed + stat.MoveSpeed))
                {
                    State = Define.State.Run;
                    actionroutine = DistanceCheck();
                    StartCoroutine(actionroutine);
                }  
            }
        }
    }

    IEnumerator DistanceCheck()
    {
        float curCheckCounter = 0;
        while (nav.remainingDistance > 0.1f)
        {
            curCheckCounter += Time.deltaTime;
            if (curCheckCounter >= maxCheckCounter)
                break;
            yield return null;
        }
        State = Define.State.Idle;
        NavMoveStop();
        actionroutine = null;
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
