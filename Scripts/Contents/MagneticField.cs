using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MagneticField : MonoBehaviourPunCallbacks,IPunObservable
{
    [SerializeField]
    int[] waitTime;
    public int WaitTime
    {
        get
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount < 6)
            {
                return waitTime[0];
            }
            else
            {
                return waitTime[1];
            }
        }
    }

    enum MagneticType
    {
        PlayerControllerOnly,
        PlayerObjectOnly,
        RoomObjectOnly,
        PlayerOnly,
        All
    }

    [SerializeField]
    MagneticType type;

    [SerializeField]
    List<float> pageSize;
    [SerializeField]
    List<int> pageDamage;
    [SerializeField]
    float pageSpeed;
    [SerializeField]
    float cycleAtk = 1f;

    [SerializeField]
    int curPage;
    public int CurPage {
        get
        {
            return curPage;
        }
        private set
        {
            curPage = value;
        } 
    }
    bool isStart = false;
    Vector3 nextPagePos;
    Vector3 nextPageSize;

    public float pRadius { get; private set; }
    float pDistacneToTarget;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.localScale);
            stream.SendNext(CurPage);
            stream.SendNext(isStart);
            stream.SendNext(nextPagePos);
            stream.SendNext(nextPageSize);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
            CurPage = (int)stream.ReceiveNext();
            isStart = (bool)stream.ReceiveNext();
            nextPagePos = (Vector3)stream.ReceiveNext();
            nextPageSize = (Vector3)stream.ReceiveNext();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(PhotonNetwork.LocalPlayer == newMasterClient)
        {
            StopAllCoroutines();
            StartCoroutine(Magnetic());

            if (isStart)
                StartCoroutine(PageCoroutine());
        }
    }

    void Awake()
    {
        pRadius = Mathf.Pow(transform.localScale.x / 2, 2);
        if (PhotonNetwork.IsMasterClient)
        {
            StopCoroutine(Magnetic());
            StartCoroutine(Magnetic());
        }
    }

    public bool PageStart()
    {
        if (pageSize.Count <= CurPage + 1)
            return false;

        if (pageSize.Count - 1 == CurPage + 1)
            gameObject.GetComponent<PhotonViewEx>().Rpc2DSound("BGM/LastPage", (int)Define.Sound2D.Bgm, 1f);

        StopCoroutine(PageCoroutine());
        NextDestination();
        StartCoroutine(PageCoroutine());
        Managers.Game.gameScene.PV.RPC("RegisterMessage", RpcTarget.AllViaServer, $"<color=red>주의!</color> \n자기장이 줄어듭니다.({curPage} 페이지)", 5f);
        return true;
    }

    void NextDestination()
    {
        int nextPage = CurPage + 1;

        if (pageSize[nextPage] == 0f)
            nextPagePos = transform.position;
        else
        {
            float curRadius = pageSize[CurPage] * 0.5f; // 현재 페이지 반지름 추출
            float nextRadius = pageSize[nextPage] * 0.5f; // 다음 페이지 반지름 추출

            float gap = curRadius - nextRadius;
           
            Vector3 nextPos = new Vector3(
            transform.position.x + Random.Range(-gap, gap),
            0,
            transform.position.z + Random.Range(-gap, gap)
            );

            nextPagePos = nextPos;
        }

        nextPageSize = new Vector3(pageSize[nextPage], transform.localScale.y, pageSize[nextPage]);
        CurPage++;
    }

    IEnumerator PageCoroutine()
    {
        isStart = true;
        while ((transform.position - nextPagePos).magnitude > 0.1f || (transform.localScale - nextPageSize).magnitude > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, nextPagePos, pageSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, nextPageSize, pageSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = nextPagePos;
        transform.localScale = nextPageSize;

        Managers.Game.ContentsScene.TimerRoutine = Managers.Game.ContentsScene.TimerStart(WaitTime);
        StartCoroutine(Managers.Game.ContentsScene.TimerRoutine);

        isStart = false;
    }

    IEnumerator Magnetic()
    {
        while(true)
        {
            if (Managers.Game.isGameStart)
            {
                pRadius = Mathf.Pow(transform.localScale.x * 0.5f, 2);
                pDistacneToTarget = 0;

                switch(type)
                {
                    case MagneticType.PlayerControllerOnly:
                        if (Managers.Game.isTeamMode)
                        {
                            CalcurationDistacne(Managers.Game.photonRedTeam);
                            CalcurationDistacne(Managers.Game.photonBlueTeam);
                        }
                        else
                        {
                            CalcurationDistacne(Managers.Game.photonSolo);
                        }
                        break;
                    case MagneticType.PlayerObjectOnly:
                        CalcurationDistacne(Managers.Game.photonPlayerGroup);
                        break;
                    case MagneticType.RoomObjectOnly:
                        CalcurationDistacne(Managers.Game.photonRoomGroup);
                        break;
                    case MagneticType.PlayerOnly:
                        if (Managers.Game.isTeamMode)
                        {
                            CalcurationDistacne(Managers.Game.photonRedTeam);
                            CalcurationDistacne(Managers.Game.photonBlueTeam);
                        }
                        else
                        {
                            CalcurationDistacne(Managers.Game.photonSolo);
                        }
                        CalcurationDistacne(Managers.Game.photonPlayerGroup);
                        break;
                    default:
                        if (Managers.Game.isTeamMode)
                        {
                            CalcurationDistacne(Managers.Game.photonRedTeam);
                            CalcurationDistacne(Managers.Game.photonBlueTeam);
                        }
                        else
                        {
                            CalcurationDistacne(Managers.Game.photonSolo);
                        }
                        CalcurationDistacne(Managers.Game.photonPlayerGroup);
                        CalcurationDistacne(Managers.Game.photonRoomGroup);
                        break;
                }
            }

            yield return new WaitForSeconds(cycleAtk);
        }
    }

    void CalcurationDistacne(Transform targetTrasform)
    {
        for (int i = 0; i < targetTrasform.childCount; i++)
        {
            if (targetTrasform.GetChild(i).TryGetComponent(out Controller3D baseController))
            {
                if (baseController.stat.Hp <= 0)
                    return;
                Vector3 targetPos = new Vector3(targetTrasform.GetChild(i).position.x, 0, targetTrasform.GetChild(i).position.z);
                pDistacneToTarget = (targetPos - transform.position).sqrMagnitude;

                if (pDistacneToTarget > pRadius)
                {
                    baseController.PV.RPC("OnAttacked", baseController.PV.Owner, pageDamage[CurPage], "자기장에 녹았습니다!");
                    baseController.PV.RPC("Rpc2DSound", baseController.PV.Owner, "SFX/Hit/Hit Sound", (int)Define.Sound2D.Effect2D, 1f);
                }
            }
        }      
    }
}
