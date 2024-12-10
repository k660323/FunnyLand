using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSPlayerStat : Stat
{
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Hp);
            stream.SendNext(MoveSpeed);
            stream.SendNext(RunSpeed);
            stream.SendNext(JumpPower);
        }
        else
        {
            Hp = (int)stream.ReceiveNext();
            MoveSpeed = (float)stream.ReceiveNext();
            RunSpeed = (float)stream.ReceiveNext();
            JumpPower = (float)stream.ReceiveNext();
        }
    }

    protected override void OnDead(int attackerId, Stat attacker)
    {

        if (gameObject.TryGetComponent(out Controller3D baseController))
        {
            baseController.StopAllCoroutines();
            Managers.Game.ContentsScene.PV.Rpc2DTeamSound("SFX/DieNotice", (int)Define.Sound2D.Effect2D, 1f, "SFX/KillNotice", (int)Define.Sound2D.Effect2D, 1f);

            if (attackerId > 999)
            {
                string attackName = Managers.Game.GetObjectNickName(attackerId);
                Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{attackName}¥‘¿Ã {PhotonNetwork.NickName}¥‘¿ª √≥∏Æ «œºÃΩ¿¥œ¥Ÿ!", 5f);
                Managers.Game.attackPlayer = attacker.gameObject;
            }
            else
            {
                Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{PhotonNetwork.NickName}¥‘¿Ã ªÁ∏¡ «œºÃΩ¿¥œ¥Ÿ!", 5f);
            }
            
            baseController.SetGetKinematic = true;
            baseController.SetGetColliderActive = false;
            baseController.State = Define.State.Die;
        }
    }
}
