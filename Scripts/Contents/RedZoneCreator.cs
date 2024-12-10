using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RedZoneCreator : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    GameObject redZonePrefab;

    PhotonViewEx redZonePV;

    float curSpawnCT = 0f;
    [SerializeField]
    float maxSpawnCT = 30f;

    IEnumerator creatorRoutine;
    int mask = 1 << 7;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curSpawnCT);
            stream.SendNext(maxSpawnCT);
        }
        else
        {
            curSpawnCT = (float)stream.ReceiveNext();
            maxSpawnCT = (float)stream.ReceiveNext();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(PhotonNetwork.LocalPlayer == newMasterClient)
        {
            if(creatorRoutine == null)
            {
                creatorRoutine = RedZoneCreateRoutine();
                StartCoroutine(creatorRoutine);
            }
        }
    }

    public void RoutineStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            curSpawnCT = 0;
            if (creatorRoutine != null)
                StopCoroutine(creatorRoutine);
            creatorRoutine = RedZoneCreateRoutine();
            StartCoroutine(creatorRoutine);
        }
    }

    IEnumerator RedZoneCreateRoutine()
    {
        while (Managers.Game.isGameStart)
        {
            if (curSpawnCT >= maxSpawnCT)
            {
                curSpawnCT = 0f;
                Vector3 rayPos = new Vector3(Random.Range(Managers.Game.navMeshMinSize.x, Managers.Game.navMeshMaxSize.x), Managers.Game.defaultPointHeight, Random.Range(Managers.Game.navMeshMinSize.z, Managers.Game.navMeshMaxSize.z));
                if (Physics.Raycast(rayPos, Vector3.down, out RaycastHit hit, Managers.Game.defaultRayLength, mask))
                {
                    if (redZonePV == null)
                    {
                        GameObject go = Util.FindChild(Managers.Game.photonRoomGroup.gameObject, "RedZone");
                        if (go != null)
                            go.TryGetComponent(out redZonePV);
                    }

                    if (redZonePV == null)
                        redZonePV = Managers.Resource.PhotonInstantiate("RedZone", hit.point, Quaternion.identity, redZonePrefab.transform.localScale, Define.PhotonObjectType.RoomObject, Managers.Game.photonRoomGroup.name).GetComponent<PhotonViewEx>();
                    else
                        redZonePV.RPC("RpcPos", RpcTarget.AllViaServer, hit.point);

                    redZonePV.RPC("ActiveBoom", RpcTarget.AllViaServer);
                }
                else
                {
                    curSpawnCT = maxSpawnCT / 2;
                }
            }
            else
                curSpawnCT += Time.deltaTime;

            yield return null;
        }
    }
}
