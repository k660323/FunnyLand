using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RedZone : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonViewEx PV;
    Collider col;

    IEnumerator BoomCoroutine;

    [SerializeField]
    int damage = 100;

    float curTime = 0f;
    [SerializeField]
    float boomTime = 7f;

    float curDestoryTime = 0f;
    [SerializeField]
    float destoryTime = 0.5f;

    [Header("Effect")]
    [SerializeField]
    GameObject Effect;
    bool effectActive;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curTime);
            stream.SendNext(curDestoryTime);
        }
        else
        {
            curTime = (float)stream.ReceiveNext();
            curDestoryTime = (float)stream.ReceiveNext();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(PhotonNetwork.LocalPlayer == newMasterClient)
        {
            if (Managers.Game.isGameStart)
            {
                if (BoomCoroutine != null)
                    StopCoroutine(BoomCoroutine);
                ActiveBoom();
            }
        }
    }

    void Awake()
    {
        col = GetComponent<Collider>();
        PV = GetComponent<PhotonViewEx>();
    }

    [PunRPC]
    public void ActiveBoom()
    {
        gameObject.SetActive(true);
        if (!effectActive)
            effectActive = Managers.Paticle.ParticleAwakePlay(Effect.name, transform.position, null);

        if(PhotonNetwork.IsMasterClient)
        {
            curTime = 0;
            curDestoryTime = 0;
            BoomCoroutine = Boom();
            StartCoroutine(BoomCoroutine);
        }
    }


    IEnumerator Boom()
    {
        while (curTime < boomTime)
        {
            curTime += Time.deltaTime;
            yield return null;
        }

        if (col.enabled == false)
        {
            col.enabled = true;
            PV.RPC("Rpc3DSound", RpcTarget.AllViaServer, "SFX/RedZone", transform.position, "", (int)Define.Sound3D.Effect3D, 1f, 7.5f, 15f);
        }   

        while (curDestoryTime < destoryTime)
        {
            curDestoryTime += Time.deltaTime;
            yield return null;
        }

        PV.RPC("RpcActive", RpcTarget.AllViaServer, false);
    }

    public override void OnDisable()
    {
        effectActive = false;
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out PhotonViewEx targetPV) && other.gameObject.TryGetComponent(out Stat targetStat) && Managers.Game.isGameStart)
            targetPV.RPC("OnAttacked", targetPV.Owner, damage, "레드존에 폭사 당했습니다!!!");
    }
}
