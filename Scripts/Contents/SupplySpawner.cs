using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SupplySpawner : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    GameObject[] spawnerObject;

    [SerializeField]
    Vector3 spawnMinRange;
    [SerializeField]
    Vector3 spawnMaxRange;
    [SerializeField]
    float addtionHeight;

    [SerializeField]
    bool MaxSpawnStop;

    [SerializeField]
    int curSpawnCount;
    [SerializeField]
    int maxSpawnCount;
    [SerializeField]
    int curCoolTime;
    [SerializeField]
    int maxCoolTime;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(newMasterClient == PhotonNetwork.LocalPlayer)
        {

        }
    }

    void StartSpawn()
    {

    }
}