using Photon.Pun;
using Photon.Pun.Demo.Cockpit.Forms;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class HealPacket : MonoBehaviourPunCallbacks
{
    [SerializeField]
    HealPacketCreator healPacketCreator;

    public bool isActive = true;
    public SpriteRenderer sr;
    public Collider2D coll;
    public int healAmount;

    private void Awake()
    {
        healPacketCreator = GetComponentInParent<HealPacketCreator>();
        coll = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        coll.enabled = PhotonNetwork.IsMasterClient ? true : false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Controller2D>();
        if (player != null)
        {
            if(player.stat.Hp > 0 && player.stat.Hp < player.stat.MaxHp)
            {
                isActive = false;
                coll.enabled = false;
                sr.enabled = false;
                healPacketCreator.StartCoroutine(healPacketCreator.CollStart());
                healPacketCreator.PV.RPC("EatHealPacket", player.PV.Owner);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
            if (isActive)
                coll.enabled = true;
    }
}
