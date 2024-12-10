using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public abstract class Controller2D : BaseController
{
    protected SpriteRenderer sr;

    public override void Init()
    {
        base.Init();

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    #region 물리 컨트롤러
    [HideInInspector]
    public Rigidbody2D rb; // 캐릭터 이동
    [HideInInspector]
    public Collider2D baseCollider2D; // 충돌
    #endregion

    public override bool SetGetKinematic
    {
        get
        {
            return rb.isKinematic;
        }
        set
        {
            rb.isKinematic = value;
        }
    }

    public override bool SetGetGravtiy
    {
        get
        {
            return rb.gravityScale != 0 ? true : false;
        }
        set
        {
            rb.gravityScale = value ? Mathf.Clamp(rb.gravityScale, 0f, 1f) : 0f;
        }
    }

    public override bool SetGetColliderTrigger
    {
        get
        {
            return baseCollider2D.isTrigger;
        }
        set
        {
            baseCollider2D.isTrigger = value;
        }
    } 

    public override bool SetGetColliderActive
    {
        get
        {
            return baseCollider2D.enabled;
        }
        set
        {
            baseCollider2D.enabled = value;
        }
    }

    public override Vector2 SetGetColiiderSize
    {
        get
        {
            if (baseCollider2D is CapsuleCollider2D)
            {
                CapsuleCollider2D cc = baseCollider2D as CapsuleCollider2D;
                return cc.size;
            }
            else
            {
                return Vector2.zero;
            }
        }
        set
        {
            if (baseCollider2D is CapsuleCollider2D)
            {
                CapsuleCollider2D cc = baseCollider2D as CapsuleCollider2D;
                cc.size = value;
            }
        }
    }

    [PunRPC]
    public override void SetPhysics(int hitPlayerId, int type,int dir, float pow, float duration)
    {
        if (Define.PhysicsType.Rotation == (Define.PhysicsType)type)
        {
            Debug.Log("2D에는 회전값을 적용시킬수 없습니다.");
            return;

        }

        GameObject hitPlayer = Managers.Game.GetPhotonObject(hitPlayerId);

        if (hitPlayer == null)
            return;

        Define.PhysicsDir _dir = (Define.PhysicsDir)dir;

        Vector3 pDir = Vector3.zero;

        if (!isPhysicsP)
        {
            switch (_dir)
            {
                case Define.PhysicsDir.Push:
                    pDir = (hitPlayer.transform.position - transform.position).normalized;
                    break;
            }

            PhysicsStateP(pDir * pow, duration);
        }
    }

    public override void SetPhysics(int type, Vector3 dir, float pow, float duration)
    {
        if (Define.PhysicsType.Rotation == (Define.PhysicsType)type)
        {
            Debug.Log("2D에는 회전값을 적용시킬수 없습니다.");
            return;
        }    

        if (!isPhysicsP)
        {
            PhysicsStateP(dir * pow, duration);
        }
    }

    protected override IEnumerator PhysicsP(Vector3 pow, float duration)
    {
        isPhysicsP = true;
        ApplyOutPhysics(pow, Define.PhysicsType.Position);
        yield return new WaitForSeconds(duration);
        isPhysicsP = false;
    }

    protected override IEnumerator PhysicsR(Vector3 pow, float duration)
    {
        isPhysicsR = true;
        yield return new WaitForSeconds(duration);
        isPhysicsR = false;
    }

    public override void ApplyOutPhysics(Vector3 pow, Define.PhysicsType type)
    {
        rb.AddForce(pow, ForceMode2D.Impulse);
    }

    protected override bool FGroundCheck()
    {
        // 플레이어 피봇이 맨밑에 있을때
        isGround = Physics2D.OverlapCircle(transform.position, 0.07f, 1 << LayerMask.NameToLayer("Ground"));

        if (isGround)
        {
            stat.JumpCount = stat.JumpMaxCount;
            //if (rb.velocity.y < 0f)
            //    rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        return isGround;
    }

    protected override void PhysicsCheck()
    {
        if (!isPhysicsP && !isMove) // 물리 효과를 받지 않으면 초기화 여부
            rb.velocity = new Vector2(0, rb.velocity.y);
    }

    protected void FlipSprite(float horizontal)
    {
        if (horizontal != 0f)
            sr.flipX = horizontal < 0 ? true : false;
    }
    protected override void SetMoveMethodComponent()
    {
        rb = GetComponent<Rigidbody2D>();
        baseCollider2D = GetComponent<Collider2D>();
    }

    #region RB 쓸때 필요함
    private float CalculateNextFrameGroundAngle(float moveSpeed)
    {
        // 다음 프레임 위치
        var nextFramePlayerPosition = FrontCheck.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        if (Physics.Raycast(nextFramePlayerPosition, Vector3.down, out RaycastHit hitInfo, RAY_DISTANCE, GWMask))
        {
            return Vector3.Angle(Vector3.up, hitInfo.normal);
        }

        return 0f;
    }

    protected Vector3 AdjustDirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    protected Vector3 GetDirection(float currentMoveSpeed)
    {
        IsOnSlope();
        FGroundCheck();

        Vector3 calculatedDirection =
            CalculateNextFrameGroundAngle(currentMoveSpeed) < maxSlopeAngle ?
            moveDir : Vector3.zero;

        calculatedDirection = (isGround && isOnSlope) ?
            AdjustDirectionToSlope(calculatedDirection) : calculatedDirection.normalized;

        return calculatedDirection;
    }

    protected void ControlGravity()
    {
        if (isGround && IsOnSlope())
        {
            rb.gravityScale = 0f;
            return;
        }

        rb.gravityScale = 1f;
    }
    #endregion
}
