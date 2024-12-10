using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stat : MonoBehaviourPunCallbacks,IPunObservable
{
    BaseController myController;

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Level);
            stream.SendNext(Hp);
            stream.SendNext(MaxHp);
            stream.SendNext(Attack);
            stream.SendNext(AttackSpeed);
            stream.SendNext(Critical);
            stream.SendNext(Defense);
            stream.SendNext(MoveSpeed);
            stream.SendNext(JumpPower);
            stream.SendNext(RunSpeed);
            stream.SendNext(Stamina);
            stream.SendNext(MaxStamina);
            stream.SendNext(RunCost);
        }
        else
        {
            Level = (short)stream.ReceiveNext();
            Hp = (int)stream.ReceiveNext();
            MaxHp = (int)stream.ReceiveNext();
            Attack = (int)stream.ReceiveNext();
            AttackSpeed = (float)stream.ReceiveNext();
            Critical = (float)stream.ReceiveNext();
            Defense = (int)stream.ReceiveNext();
            MoveSpeed = (float)stream.ReceiveNext();
            JumpPower = (float)stream.ReceiveNext();
            RunSpeed = (float)stream.ReceiveNext();
            Stamina = (float)stream.ReceiveNext();
            MaxStamina = (float)stream.ReceiveNext();
            RunCost = (float)stream.ReceiveNext();
        }
    }

    protected object _lock = new object();

    [SerializeField]
    protected short _level;
    [SerializeField]
    protected int _hp;
    [SerializeField]
    [Min(0)]
    protected int _maxHp;
    [SerializeField]
    protected int _mp;
    [SerializeField]
    [Min(0)]
    protected int _maxMp;
    [SerializeField]
    protected int _attack;
    [SerializeField]
    protected float _attSpd = 1f;
    [SerializeField]
    protected float _critical;
    [SerializeField]
    protected int _defense;
    [SerializeField]
    protected float _moveSpeed;
    [SerializeField]
    protected float _jumpPower;
    [SerializeField]
    protected int _jumpCount;
    [SerializeField]
    protected int _jumpMaxCount;
    [SerializeField]
    protected float _jumpCool;
    [SerializeField]
    protected float _jumpMaxCool;
    [SerializeField]
    protected float _runSpeed;
    [SerializeField]
    protected float _stamina;
    [SerializeField]
    protected float _maxStamina;
    [SerializeField]
    protected float _runCost;

    public short Level { get { return _level; } set { _level = value; } }
    public int Hp { get { return _hp; } set { _hp = Mathf.Clamp(value, 0, MaxHp); } }
    public int MaxHp { get { return _maxHp; } set { _maxHp = value; } }
    public int Mp { get { return _mp; } set { _mp = Mathf.Clamp(value, 0, MaxMp); } }
    public int MaxMp { get { return _maxMp; } set { _maxMp = value; } }
    public int Attack { get { return _attack; } set { _attack = value; } }
    public float AttackSpeed { get { return _attSpd; } set { _attSpd = value; } }
    public float Critical { get { return _critical; } set { _critical = value; } }
    public int Defense { get { return _defense; } set { _defense = value; } }
    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float JumpPower { get { return _jumpPower; } set { _jumpPower = value; } }
    public int JumpCount { get { return _jumpCount; } set { _jumpCount = value; } }
    public int JumpMaxCount { get { return _jumpMaxCount; } set { _jumpMaxCount = value; } }
    public float JumpCool { get { return _jumpCool; } set { _jumpCool = value; } }
    public float JumpMaxCool { get { return _jumpMaxCool; } set { _jumpMaxCool = value; } }
    public float RunSpeed { get { return _runSpeed; } set { _runSpeed = value; } }
    public float Stamina { get { return _stamina; } set { _stamina = value; } }
    public float MaxStamina { get { return _maxStamina; } set { _maxStamina = value; } }
    public float RunCost { get { return _runCost; } set { _runCost = value; } }

    void Start()
    {
        myController = gameObject.GetComponent<BaseController>();
    }

    [PunRPC]
    public virtual void OnAttacked(int attackerID)
    {
        lock (_lock)
        {
            if (Hp <= 0)
                return;

            if (myController.soundList.HitClipName != "")
            {
                myController.PV.RPC("OnHitSound", RpcTarget.AllViaServer);
                //myController.PV.RPC("Rpc3DSound", RpcTarget.AllViaServer, myController.soundList.HitClipName, transform.position, "", (int)Define.Sound3D.Effect3D, 1f, 0.5f, myController.soundList.HitMaxSound);
            }
            Stat attacker = Managers.Game.GetPhotonObject(attackerID).GetComponent<Stat>();
            if (attacker == null)
                return;

            int damage = Mathf.Max(0, attacker.Attack - Defense);
            Hp -= damage;
            if (Hp <= 0)
            {
                Hp = 0;
                OnDead(attackerID, attacker);
            }
        }
    }

    [PunRPC]
    public virtual void OnAttacked(int attackerID , int attackDamage)
    {
        lock (_lock)
        {
            if (Hp <= 0)
                return;

            if (myController.soundList.HitClipName != "")
                myController.PV.RPC("OnHitSound", RpcTarget.AllViaServer);

                int damage = Mathf.Max(0, attackDamage - Defense);
            Hp -= damage;

            if (Hp <= 0)
            {
                Hp = 0;
                OnDead(attackerID);
            }
        }
    }

    [PunRPC]
    public virtual void OnAttacked(int attackDamage, string killerName)
    {
        lock (_lock)
        {
            if (Hp <= 0)
                return;

            if (myController.soundList.HitClipName != "")
                myController.PV.RPC("OnHitSound", RpcTarget.AllViaServer);

            int damage = Mathf.Max(0, attackDamage - Defense);
            Hp -= damage;
            
            if (Hp <= 0)
            {
                Hp = 0;
                OnDead(killerName);
            }
        }
    }

    protected virtual void OnDead(int attackerId, Stat attacker)
    {
        if (myController != null)
        {
            myController.StopAllCoroutines();
            myController.SetGetKinematic = true;
           
            myController.SetGetColliderActive = false;
            myController.State = Define.State.Die;
        }
    }
    protected virtual void OnDead(int attackerId)
    {
        if (myController != null)
        {
            myController.StopAllCoroutines();
            myController.SetGetKinematic = true;

            myController.SetGetColliderActive = false;
            myController.State = Define.State.Die;
        }
    }
    protected virtual void OnDead(string killMessage = "")
    {
        if (myController != null)
        {
            myController.StopAllCoroutines();
            myController.SetGetKinematic = true;

            myController.SetGetColliderActive = false;
            myController.State = Define.State.Die;
        }
    }

    [PunRPC]
    public virtual bool SetHp(int amount)
    {
        if (Hp <= 0)
            return false;

        if (amount > 0 && Hp == MaxHp)
            return false;

        Hp += amount;

        return true;
    }
}
