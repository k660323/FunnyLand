using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPBotStat : Stat
{
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Hp);
        }
        else
        {
            Hp = (int)stream.ReceiveNext();
        }
    }

    protected override void OnDead(int attackerId, Stat attacker)
    {
        if (!gameObject.TryGetComponent(out PhotonViewEx PV))
            return;

        if (gameObject.TryGetComponent(out Controller3D baseController))
        {
            baseController.StopAllCoroutines();
            baseController.nav.isStopped = true;
            baseController.SetGetKinematic = true;
            baseController.SetGetColliderActive = false;

            if (attacker is LPPlayerStat)
            {
                if (attacker.TryGetComponent(out PhotonViewEx attackerPV))
                {
                    attackerPV.RPC("OnAttacked", attackerPV.Owner, PV.ViewID);
                }
            }

            baseController.State = Define.State.Die;
        }
    }

    protected override void OnDead(string killMessage = "")
    {
        if (gameObject.TryGetComponent(out Controller3D baseController))
        {
            baseController.StopAllCoroutines();
            baseController.nav.isStopped = true;
            baseController.SetGetKinematic = true;
            baseController.SetGetColliderActive = false;

            baseController.State = Define.State.Die;
        }
    }
}
