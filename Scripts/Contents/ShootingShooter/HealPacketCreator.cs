using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPacketCreator : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    HealPacket healPacket;

    public PhotonViewEx PV;

    public float curSpawnCoolTime;
    public float maxSpawnCoolTime;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(healPacket.isActive);
            stream.SendNext(curSpawnCoolTime);
            stream.SendNext(healPacket.sr.enabled);
        }
        else
        {
            healPacket.isActive = (bool)stream.ReceiveNext();
            curSpawnCoolTime = (float)stream.ReceiveNext();
            healPacket.sr.enabled = (bool)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        PV = GetComponent<PhotonViewEx>();
        healPacket = GetComponentInChildren<HealPacket>();
    }

    [PunRPC]
    public void EatHealPacket()
    {
        var myPlayer = Managers.Game.myPlayer.GetComponent<Controller2D>();
        myPlayer.stat.SetHp(healPacket.healAmount);
    }

    public IEnumerator CollStart()
    {
        curSpawnCoolTime = 0f;
        while (curSpawnCoolTime < maxSpawnCoolTime)
        {
            curSpawnCoolTime += Time.deltaTime;
            yield return null;
        }

        healPacket.isActive = true;
        healPacket.coll.enabled = true;
        healPacket.sr.enabled = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            if (!healPacket.isActive)
                StartCoroutine(CollStart());
        }
    }
}
