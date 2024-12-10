using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public abstract class Controller3D : BaseController
{
    #region 이동 방식(캐컨/물컨)
    [SerializeField]
    protected Define.MoveMethod moveMethod;

    public Define.MoveMethod MoveMethod
    {
        get
        {
            return moveMethod;
        }
    }
    #endregion

    #region 캐릭터 컨트롤러
    [HideInInspector]
    public CharacterController cController; // 캐릭터 이동, 충돌 
    [HideInInspector]
    public CRigidBody3D crb3D;
    #endregion

    #region 물리 컨트롤러
    [HideInInspector]
    public Rigidbody rb; // 캐릭터 이동
    [HideInInspector]
    public Collider baseCollider3D; // 충돌
    protected Vector3 gravity;
    #endregion

    public override bool SetGetKinematic
    {
        get
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.RColliderController:
                    return rb.isKinematic;
                case Define.MoveMethod.CController:
                    return crb3D.isKinematic;
                default:
                    return false;
            }
        }
        set
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.RColliderController:
                    rb.isKinematic = value;
                    break;
                case Define.MoveMethod.CController:
                    crb3D.isKinematic = value;
                    break;
            }
        }
    }

    public override bool SetGetGravtiy
    {
        get
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    return crb3D.useGravity;
                case Define.MoveMethod.RColliderController:
                    return rb.useGravity;
                default:
                    return false;
            }
        }
        set
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    crb3D.useGravity = value;
                    break;
                case Define.MoveMethod.RColliderController:
                    rb.useGravity = value;
                    break;
            }
        }
    }

    public override bool SetGetColliderTrigger
    {
        get
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    return cController.isTrigger;
                case Define.MoveMethod.RColliderController:
                    return baseCollider3D.isTrigger;
                default:
                    return false;
            }
        }
        set
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    cController.isTrigger = value;
                    break;
                case Define.MoveMethod.RColliderController:
                    baseCollider3D.isTrigger = value;
                    break;
            }
        }
    }

    public override bool SetGetColliderActive
    {
        get
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    return cController.enabled;
                case Define.MoveMethod.RColliderController:
                    return baseCollider3D.enabled;
                default:
                    return false;
            }
        }
        set
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    cController.enabled = value;
                    break;
                case Define.MoveMethod.RColliderController:
                    baseCollider3D.enabled = value;
                    break;
            }
        }
    }

    public override Vector2 SetGetColiiderSize
    {
        get
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    return new Vector2(cController.radius, cController.height);
                case Define.MoveMethod.RColliderController:
                    if (baseCollider3D is CapsuleCollider)
                    {
                        CapsuleCollider cc = baseCollider3D as CapsuleCollider;
                        return new Vector2(cc.radius, cc.height);
                    }
                    else
                    {
                        return Vector2.zero;
                    }
                default:
                    return Vector2.zero;
            }
        }
        set
        {
            switch (moveMethod)
            {
                case Define.MoveMethod.CController:
                    cController.radius = value.x;
                    cController.height = value.y;
                    return;
                case Define.MoveMethod.RColliderController:
                    if (baseCollider3D is CapsuleCollider)
                    {
                        CapsuleCollider cc = baseCollider3D as CapsuleCollider;
                        cc.radius = value.x;
                        cc.height = value.y;
                    }
                    return;
            }
        }
    }

    [PunRPC]
    public override void SetPhysics(int hitPlayerId, int type,int dir, float pow, float duration)
    {
        GameObject hitPlayer = Managers.Game.GetPhotonObject(hitPlayerId);

        if (hitPlayer == null)
            return;

        Define.PhysicsType _type = (Define.PhysicsType)type;
        Define.PhysicsDir _dir = (Define.PhysicsDir)dir;

        Vector3 pDir = Vector3.zero;
        switch (_type)
        {
            case Define.PhysicsType.Position:
                if (!isPhysicsP)
                {
                    switch(_dir)
                    {
                        case Define.PhysicsDir.Push:
                            pDir = (transform.position - hitPlayer.transform.position).normalized;
                            pDir.y = 0;
                            break;
                        case Define.PhysicsDir.Pull:
                            pDir = (hitPlayer.transform.position - transform.position).normalized;
                            pDir.y = 0;
                            break;
                        case Define.PhysicsDir.Up:
                            pDir = Vector3.up;
                            break;
                        case Define.PhysicsDir.Down:
                            pDir = Vector3.down;
                            break;
                        case Define.PhysicsDir.Knockback:
                            pDir = (transform.position - hitPlayer.transform.position).normalized;
                            break;
                    }

                    //pDir = -transform.forward * 10;
                    PhysicsStateP(pDir * pow, duration);
                }
                break;
            case Define.PhysicsType.Rotation:
                if (!isPhysicsR)
                {
                    switch (_dir)
                    {
                        case Define.PhysicsDir.Up:
                            pDir = (hitPlayer.transform.position - transform.position).normalized;
                            break;
                    }
                    PhysicsStateR(pDir * pow, duration);
                }
                break;
        } 
    }

    public override void SetPhysics(int type, Vector3 dir, float pow, float duration)
    {
        Define.PhysicsType _type = (Define.PhysicsType)type;

        switch(_type)
        {
            case Define.PhysicsType.Position:
                if (!isPhysicsP)
                {
                    PhysicsStateP(dir * pow, duration);
                }
                break;
            case Define.PhysicsType.Rotation:
                if (!isPhysicsR)
                {
                    PhysicsStateR(dir * pow, duration);
                }
                break;
        }
    }

    protected override IEnumerator PhysicsP(Vector3 pow, float duration)
    {
        isPhysicsP = true;
        if(State != Define.State.CC)
        {
            State = Define.State.CC;
            if(nav)
                nav.enabled = false;
        }
        ApplyOutPhysics(pow, Define.PhysicsType.Position);
        yield return new WaitForSeconds(duration);
        if (State == Define.State.CC)
            State = Define.State.Idle;
        isPhysicsP = false;
    }

    protected override IEnumerator PhysicsR(Vector3 pow, float duration)
    {
        isPhysicsR = true;
        ApplyOutPhysics(pow, Define.PhysicsType.Rotation);
        yield return new WaitForSeconds(duration);
        isPhysicsR = false;
    }

    public override void ApplyOutPhysics(Vector3 pow, Define.PhysicsType type)
    {
        switch (type)
        {
            case Define.PhysicsType.Position:
                switch (moveMethod)
                {
                    case Define.MoveMethod.CController:
                        crb3D.AddForce(pow, ForceMode.Impulse);
                        break;
                    case Define.MoveMethod.RColliderController:
                        rb.AddForce(pow, ForceMode.Impulse);
                        break;

                }
                break;
            case Define.PhysicsType.Rotation:
                switch (moveMethod)
                {
                    case Define.MoveMethod.CController:
                        crb3D.AddTorque(pow, ForceMode.Impulse);
                        break;
                    case Define.MoveMethod.RColliderController:
                        rb.AddTorque(pow, ForceMode.Impulse);
                        break;

                }
                break;
        }
    }

    protected override bool FGroundCheck()
    {
        // 플레이어 피봇이 맨밑에 있을때
        Vector3 boxSize = new Vector3(transform.lossyScale.x * 0.5f, 0.1f, transform.lossyScale.z * 0.5f);

        isGround = Physics.CheckBox(GroundCheck.position, boxSize, Quaternion.identity, GroundMask);
        //isRBGround = Physics.CheckSphere(transform.position, 0.5f, 1 << LayerMask.NameToLayer("Ground"));
        if (isGround && !isJump)
            stat.JumpCount = stat.JumpMaxCount;

        return isGround;
    }

    [HideInInspector]
    public NavMeshAgent nav;
    protected NavMeshPath path;
    protected OffMeshLinkData offMeshLinkData; // 메쉬위에 있지 않을때 데이터 저장

    protected override void SetMoveMethodComponent()
    {
        switch (moveMethod)
        {
            case Define.MoveMethod.CController:
                cController = GetComponent<CharacterController>();
                crb3D = GetComponent<CRigidBody3D>();
                break;
            case Define.MoveMethod.RColliderController:
                rb = GetComponent<Rigidbody>();
                baseCollider3D = GetComponent<Collider>();
                break;
        }
    }

    protected virtual bool NavMoveStart(Define.NavMoveDest type, float moveSpeed = 0f, bool fastLookat = true)
    {
        if (!isGround)
            return false;

        nav.enabled = true;

        if (!nav.isOnNavMesh && !nav.Warp(transform.position))
            return false;

        Vector3 pos = Vector3.zero;
        switch(type)
        {
            case Define.NavMoveDest.Target:
                pos = target.transform.position;
                break;
            case Define.NavMoveDest.Point:
                pos = _destPos;
                break;
            default:
                return false;
        }

        nav.CalculatePath(pos, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            if (fastLookat)
                transform.LookAt(new Vector3(pos.x, 0f, pos.z));
            nextFrameMSpeed = moveSpeed;
            nav.speed = nextFrameMSpeed;
            nav.SetDestination(pos);
            nav.isStopped = false;

            return true;
        }

        return false;
    }

    public void NavMoveStop()
    {
        if (nav.enabled)
        {
            nav.isStopped = true;
            nextFrameMSpeed = 0f;
            nav.speed = nextFrameMSpeed;
        }
    }

    #region RB 쓸때 필요함
    private bool CalculateNextFrameFrontObject(ref float framePerMove)
    {
        if (Physics.Raycast(FrontCheck.position, moveDir, framePerMove, GWMask))
            return true;

        return false;
    }


    private float CalculateNextFrameGroundAngle(float moveSpeed)
    {
        float framePerMove = moveSpeed * Time.fixedDeltaTime;
        var nextFramePlayerPosition = FrontCheck.position + moveDir * framePerMove;

        if (Physics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo, RAY_DISTANCE, GWMask))
        {
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            if(angle == 0f)
            {
                return CalculateNextFrameFrontObject(ref framePerMove) ? 90f : 0f;
            }
            else
            {
                return angle;
            }
        }

        return 0f;
    }

    protected Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    protected Vector3 GetCorrectionDirection(float currentMoveSpeed)
    {
        IsOnSlope();
        FGroundCheck();
        LookAt(new Vector3(moveDir.x, 0, moveDir.z));
        Vector3 calculatedDirection =
            CalculateNextFrameGroundAngle(currentMoveSpeed) < maxSlopeAngle ?
            moveDir : Vector3.zero;

        calculatedDirection = (isGround && isOnSlope) ?
            AdjustDirectionToSlope(calculatedDirection) : calculatedDirection.normalized;

        return calculatedDirection;
    }

    public void LookAt(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetAngle = Quaternion.LookRotation(direction);
            rb.rotation = targetAngle;
        }
    }

    protected void ControlGravity()
    {
        if (isGround && IsOnSlope())
        {
            //gravity = Vector3.zero;
            
            rb.useGravity = false;
            return;
        }

        //gravity = Vector3.down * Mathf.Abs(rb.velocity.y);
        rb.useGravity = true;
    }
    #endregion
}
