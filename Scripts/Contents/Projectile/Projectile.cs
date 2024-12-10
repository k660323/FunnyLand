using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

public abstract class Projectile : MonoBehaviourPunCallbacks, IPunObservable
{
    protected PhotonViewEx PV;

    [SerializeField]
    protected Define.DimensionMode dimensionMode;
    protected int masterID;

    [SerializeField]
    protected Vector2 attackSize;
    [SerializeField]
    protected float attackRange;
    protected Vector3 dir;

    protected Collider collider3d;
    protected Collider2D collider2d;
    protected Rigidbody rb3D;
    protected Rigidbody2D rb2D;

    protected bool isAtk;

    protected bool isUse;
    protected int damage;
    protected float speed;

    protected float liftTime;
    [SerializeField]
    protected float maxLiftTime;

    Vector3 curPos;
    Quaternion curRot;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(transform.localScale);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            curRot = (Quaternion)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            if (dimensionMode == Define.DimensionMode._2D)
                collider2d.enabled = isUse;
            else
                collider3d.enabled = isUse;
        }
    }

    private void Awake()
    {
        PV = GetComponent<PhotonViewEx>();
        Managers.Game.SetPhotonObject(gameObject);

        if (dimensionMode == Define.DimensionMode._2D)
        {
            rb2D = GetComponent<Rigidbody2D>();
            collider2d = GetComponent<Collider2D>();
            
        }
        else
        {
            rb3D = GetComponent<Rigidbody>();
            collider3d = GetComponent<Collider>();
        }
    }

    public override void OnEnable()
    {
        if(isUse)
        {
            StopCoroutine(RegisterInsertQueue());
            StartCoroutine(RegisterInsertQueue());
        }
        else
        {
            gameObject.SetActive(false);
        }
     
    }

    protected IEnumerator RegisterInsertQueue()
    {
        yield return new WaitForSeconds(liftTime);
        isUse = false;
        gameObject.SetActive(false);
        if(PV.IsMine)
            Managers.Resource.PhotonDestroy(gameObject);
    }

    [PunRPC]
    protected virtual void CancelRegisterInsertQueue()
    {
        StopCoroutine(RegisterInsertQueue());
        isUse = false;
        gameObject.SetActive(false);
        if (PV.IsMine)
            Managers.Resource.PhotonDestroy(gameObject);
    }

    // 다같이 호출
    [PunRPC]
    public virtual void Init2D(int _masterID, int _damage, float _speed, Vector2 _dir, float _maxLiftTime)
    {
        masterID = _masterID;
        damage = _damage;
        speed = _speed;
        dir = _dir;
        maxLiftTime = _maxLiftTime;
        liftTime = _maxLiftTime;
        isUse = true;
        if(PhotonNetwork.IsMasterClient)
            collider2d.enabled = true;
        gameObject.SetActive(true);

        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    // 주인이 넣어줌
    [PunRPC]
    public virtual void Init3D(int _masterID, int _damage, float _speed, Vector3 _dir)
    {
        masterID = _masterID;
        damage = _damage;
        speed = _speed;
        dir = _dir;
        liftTime = maxLiftTime;
        if (PhotonNetwork.IsMasterClient)
            collider3d.enabled = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if(PV.IsMine)
        {
            OnMove();
        }
        else
        {
            if ((transform.position - curPos).sqrMagnitude > 50)
            {
                transform.position = curPos;
                transform.rotation = curRot;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, curPos, 35f * Time.deltaTime);
                float t = Mathf.Clamp(20f * Time.deltaTime, 0f, 0.99999f);
                transform.rotation = Quaternion.Lerp(transform.rotation, curRot, t);
            }
        }
    }

    protected abstract void OnMove();

    protected virtual void OnTriggerEnter2D(Collider2D collision) { }

    protected virtual void OnTriggerEnter(Collider collision) { }
}
