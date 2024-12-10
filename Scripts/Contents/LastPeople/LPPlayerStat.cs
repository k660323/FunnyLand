using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPPlayerStat : Stat, IPunObservable
{
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Hp);
            stream.SendNext(MoveSpeed);
            stream.SendNext(RunSpeed);
            stream.SendNext(Stamina);
        }
        else
        {
            Hp = (int)stream.ReceiveNext();
            MoveSpeed = (float)stream.ReceiveNext();
            RunSpeed = (float)stream.ReceiveNext();
            Stamina = (float)stream.ReceiveNext();
        }
    }

    protected override void OnDead(int attackerId, Stat attacker)
    {
        if (gameObject.TryGetComponent(out Controller3D baseController))
        {
            baseController.StopAllCoroutines();
            Managers.Game.ContentsScene.PV.Rpc2DTeamSound("SFX/DieNotice", (int)Define.Sound2D.Effect2D, 1f, "SFX/KillNotice", (int)Define.Sound2D.Effect2D, 1f);

            if (attacker is LPPlayerStat)
            {
                Managers.Game.ContentsScene.PV.RPC("SubQuestReport", Managers.Game.GetPlayerObjectPV(attackerId).Owner, 0);
                string attackName = Managers.Game.GetObjectNickName(attackerId);
                Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{attackName}¥‘¿Ã {PhotonNetwork.NickName}¥‘¿ª √≥∏Æ «œºÃΩ¿¥œ¥Ÿ!", 5f);
                Managers.Game.attackPlayer = attacker.gameObject;
            }
            else
            {
                Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{PhotonNetwork.NickName}¥‘¿Ã ∫ø¿ª «–¥Î«œø© ªÁ∏¡ «œºÃΩ¿¥œ¥Ÿ!", 5f);
            }
            baseController.SetGetColliderActive = false;
            baseController.State = Define.State.Die;
        }
    }

    protected override void OnDead(string killMessage = "")
    {
        if (gameObject.TryGetComponent(out Controller3D baseController))
        {
            baseController.StopAllCoroutines();

            Managers.Game.ContentsScene.PV.Rpc2DTeamSound("SFX/DieNotice", (int)Define.Sound2D.Effect2D, 1f, "SFX/KillNotice", (int)Define.Sound2D.Effect2D, 1f);
            Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{PhotonNetwork.NickName}¥‘¿Ã {killMessage}", 5f);
            baseController.SetGetColliderActive = false;
            baseController.State = Define.State.Die;
        }
    }
}
