using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Weapon : MonoBehaviour
{
    protected int soundIndex = 0;

    [SerializeField]
    protected int multiAtkCnt = 1;
    protected int curCount = 0;

    protected BaseController bc;
    public bool isAtk;

    #region 공격 딜레이
    [Header("공격 후딜 설정")]
    [SerializeField]
    protected bool isAtkDelay;
    [SerializeField]
    protected AnimationClip atkClip;
    protected float attackAnimaionLength;
    public float waitTime { get; private set; }
    public float curTime { get; private set; }
    #endregion

    [HideInInspector]
    public UnityAction attackAction;

    [Header("공격 사운드")]
    [SerializeField]
    protected string[] attackClipName;
    [SerializeField]
    protected float[] attackMaxSound = { 1 };

    [Header("물리 효과")]
    [SerializeField]
    PhysicsEffect[] pEffect;

    public virtual void Init(BaseController _bc)
    {
        bc = _bc;
        if (isAtkDelay)
        {
            if (atkClip != null)
                attackAnimaionLength = atkClip.length;
            else
                attackAnimaionLength = 1f;

            attackAction += CoolAttack;
        }
        else
        {
            attackAnimaionLength = 1f;
            attackAction += Attack;
        }
    }

    public abstract void Attack();

    public abstract void CoolAttack();

    //private void OnDrawGizmos()
    //{
    //    if (Physics.BoxCast(transform.position + Vector3.up, attackSize / 2f, transform.forward, out RaycastHit hit, transform.rotation, attackRange))
    //    {
    //        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * hit.distance);
    //        Gizmos.DrawWireCube(hit.point, attackSize);
    //    }
    //    else
    //    {
    //        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * attackRange);
    //    }
    //}

    protected IEnumerator NormalAtkCool()
    {
        isAtk = true;

        waitTime = (bc.stat.AttackSpeed < 1) ? attackAnimaionLength + (1 / bc.stat.AttackSpeed) : attackAnimaionLength / bc.stat.AttackSpeed;
        curTime = waitTime;

        while(curTime > 0)
        {
            curTime -= Time.deltaTime;
            yield return null;
        }

        isAtk = false;
    }

    protected virtual void OnAttack() { Debug.Log("호출"); }

    protected virtual void OnAttack2D() { Debug.Log("호출"); }

    protected void OnAddtionPhysicsed(PhotonViewEx targetPV)
    {
        if (pEffect == null)
            return;

        for (int i = 0; i < pEffect.Length; i++)
        {
            targetPV.RPC("SetPhysics", targetPV.Owner, bc.PV.ViewID, (int)pEffect[i].type,
                            (int)pEffect[i].dir, pEffect[i].power, pEffect[i].duraiton);
        }
    }

    protected virtual void OnAttackEnd()
    {
        if (bc.State != Define.State.CC)
            bc.State = Define.State.Idle;
    }
}
