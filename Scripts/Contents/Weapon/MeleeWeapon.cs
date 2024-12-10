using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SerializeField]
    protected Vector2 attackSize;
    [SerializeField]
    protected float attackRange;
    [SerializeField]
    protected Vector3 localAtkPos;

    public override void Attack()
    {
        soundIndex = Random.Range(0, attackClipName.Length);
        bc.PV.RPC("Rpc3DSound", RpcTarget.AllViaServer, attackClipName[soundIndex], transform.position, "", (int)Define.Sound3D.Effect3D, 1f, 1f, attackMaxSound[soundIndex]);
        bc.State = Define.State.NormalAttacking;
    }

    public override void CoolAttack()
    {
        soundIndex = Random.Range(0, attackClipName.Length);
        bc.PV.RPC("Rpc3DSound", RpcTarget.AllViaServer, attackClipName[soundIndex], transform.position, "", (int)Define.Sound3D.Effect3D, 1f, 1f, attackMaxSound[soundIndex]);
        bc.State = Define.State.NormalAttacking;
        StartCoroutine(NormalAtkCool());
    }

    protected override void OnAttack()
    {
        if (!PhotonNetwork.IsMasterClient || bc.stat.Hp <= 0)
            return;

        curCount = 0;
        RaycastHit[] hits = Physics.BoxCastAll(transform.position + localAtkPos, attackSize / 2f, transform.forward, transform.rotation, attackRange);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == this.gameObject)
                continue;

            if (curCount == multiAtkCnt)
                break;

            if (hit.transform.TryGetComponent(out BaseController target))
            {
                if (target.stat.Hp <= 0)
                    continue;

                if (!target.PV.IsRoomView)
                {
                    if (Managers.Game.isTeamMode && !Managers.Game.isTeamKill)
                        if (target.team == bc.team)
                            continue;
                }
                else
                {
                    if (target.team == bc.team)
                        continue;
                }

                OnAddtionPhysicsed(target.PV);
                target.PV.RPC("OnAttacked", target.PV.Owner, bc.PV.ViewID);
                curCount++;
            }
            
        }

        /*else
        {
            if (Physics.BoxCast(transform.position + localAtkPos, attackSize / 2f, transform.forward, out RaycastHit hit, transform.rotation, attackRange))
            {
                if (hit.transform.TryGetComponent(out BaseController target))
                {
                    if (target.stat.Hp <= 0)
                        return;

                    if (!target.PV.IsRoomView)
                    {
                        if (Managers.Game.isTeamMode && !Managers.Game.isTeamKill)
                            if (target.team == bc.team)
                                return;
                    }
                    else
                    {
                        if (target.team == bc.team)
                            return;
                    }

                    OnAddtionPhysicsed(target.PV);
                    target.PV.RPC("OnAttacked", target.PV.Owner, bc.PV.ViewID);
                }
            }
        }*/
    }
}
