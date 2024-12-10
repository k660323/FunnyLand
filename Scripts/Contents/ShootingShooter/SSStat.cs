using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSStat : Stat
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

    protected override void OnDead(int attackerId)
    {
        Managers.Game.ContentsScene.PV.Rpc2DTeamSound("SFX/DieNotice", (int)Define.Sound2D.Effect2D, 1f, "SFX/KillNotice", (int)Define.Sound2D.Effect2D, 1f);
        if (gameObject.TryGetComponent(out BaseController baseController))
        {
            baseController.StopAllCoroutines();
            baseController.SetGetKinematic = true;

            baseController.SetGetColliderActive = false;
            baseController.State = Define.State.Die;

            Player atkPlayer = Managers.Game.GetPlayerObjectPV(attackerId).Owner;
            Managers.Game.gameScene.PV.RPC("PlayerPropertieIntRPC", atkPlayer, "Kill", 1);

            if(!baseController.PV.IsRoomView)
                Managers.Game.ContentsScene.ReSpawn();
        }
    }

    // »£√‚ x
    protected override void OnDead(int attackerId, Stat attacker)
    {
        if (gameObject.TryGetComponent(out BaseController baseController))
        {
            baseController.StopAllCoroutines();
            Managers.Game.ContentsScene.PV.Rpc2DTeamSound("SFX/DieNotice", (int)Define.Sound2D.Effect2D, 1f, "SFX/KillNotice", (int)Define.Sound2D.Effect2D, 1f);

            if (attacker is SSStat)
            {
                string attackName = Managers.Game.GetObjectNickName(attackerId);
                Player atkPlayer = Managers.Game.GetPlayerObjectPV(attackerId).Owner;
                Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{attackName}¥‘¿Ã {PhotonNetwork.NickName}¥‘¿ª √≥∏Æ «œºÃΩ¿¥œ¥Ÿ!", 5f);
                Managers.Game.attackPlayer = attacker.gameObject;
                Debug.Log(atkPlayer.NickName);
                Managers.Game.gameScene.PV.RPC("PlayerPropertieIntRPC", atkPlayer, "Kill", 1);
            }
            else
            {
                Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{PhotonNetwork.NickName}¥‘¿Ã ∫ø¿ª «–¥Î«œø© ªÁ∏¡ «œºÃΩ¿¥œ¥Ÿ!", 5f);
            }
            baseController.SetGetColliderActive = false;
            baseController.State = Define.State.Die;
            Managers.Game.ContentsScene.ReSpawn();
        }
    }

    protected override void OnDead(string killMessage = "")
    {
        if (gameObject.TryGetComponent(out BaseController baseController))
        {
            baseController.StopAllCoroutines();

            Managers.Game.ContentsScene.PV.Rpc2DTeamSound("SFX/DieNotice", (int)Define.Sound2D.Effect2D, 1f, "SFX/KillNotice", (int)Define.Sound2D.Effect2D, 1f);
            Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"{PhotonNetwork.NickName}¥‘¿Ã {killMessage}", 5f);
            baseController.SetGetColliderActive = false;
            baseController.State = Define.State.Die;
        }
    }
}
