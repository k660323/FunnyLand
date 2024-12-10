using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SpawnList
{
    public int waitSpawnTime;
    public int[] spawnIndex;
    public int[] spawnCount;
}

public class SpawnRootine : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    MobSpawner spawner;

    [SerializeField]
    public SpawnList[] spawnList;

    PhotonViewEx PV;

    int listIndex = 0;

    private void Start()
    {
        spawner = GetComponent<MobSpawner>();
        PV = GetComponent<PhotonViewEx>();
    }

    public void RootineStart()
    {
        StartCoroutine(Counting());
    }

    IEnumerator Counting()
    {
        while(listIndex < spawnList.Length && !Managers.Game.ContentsScene.endFlag)
        {
            yield return new WaitForSeconds(spawnList[listIndex].waitSpawnTime);
            Managers.Game.gameScene.RegisterMessage($"Wave {listIndex + 1}", 5f);
            PV.Rpc2DSound("SFX/Zombie Spawn Sound", (int)Define.Sound2D.Effect2D, 1f);
            for (int i = 0; i < spawnList[listIndex].spawnIndex.Length; i++)
                spawner.SpawnMob(spawnList[listIndex].spawnIndex[i], spawnList[listIndex].spawnCount[i]);
            listIndex++;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(newMasterClient == PhotonNetwork.LocalPlayer)
        {
            StartCoroutine(Counting());
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(listIndex);
        }
        else
        {
            listIndex = (int)stream.ReceiveNext();
        }
    }
}
