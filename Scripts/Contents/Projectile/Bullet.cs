using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
    protected override void OnMove() // 본인
    {
        transform.Translate(dir * speed * Time.deltaTime); // 초당 해당 방향의 속도로 이동
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent(out BaseController baseController))
        {
            if (baseController.stat.Hp <= 0)
                return;

            if (PV.Owner == baseController.PV.Owner)
                return;

            if (!baseController.PV.IsRoomView)
            {
                if (Managers.Game.isTeamMode && !Managers.Game.isTeamKill)
                {
                    if (baseController.PV.Owner.CustomProperties["Team"].ToString() == PV.Owner.CustomProperties["Team"].ToString())
                        return;
                }
            }

            // 히트 카운트 
            baseController.PV.RPC("OnAttacked", baseController.PV.Owner, masterID, damage);
            PV.RPC("CancelRegisterInsertQueue", RpcTarget.AllViaServer);
        }
    }

    protected override void OnTriggerEnter(Collider collision)
    { 


    }
}
