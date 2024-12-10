using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public abstract class BaseController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Define.Team team;

    [Header("애니메이션 속도 보정")]
    public bool SlowAction = false;
    public QuestReporter[] reporterQuests;

    [HideInInspector]
    public SoundEvent soundList;

    [HideInInspector]
    public PhotonViewEx PV;
    protected Animator anim;
    [HideInInspector]
    public Stat stat { get; protected set; }
    [HideInInspector]
    public EquipmentWindowData equipmentWindowData;
    public Weapon weapon { get; private set; }

    public Color teamColor = Color.yellow;
    public Color enemyColor = Color.red;

    [HideInInspector]
    public TextMesh nickNameTextMesh;
    [HideInInspector]
    public Transform worldCanvas;
    [HideInInspector]
    public Text nickNameText;

    protected bool isMove;
    protected bool isGround = true;
    protected bool isOnSlope;
    protected bool isJump;

    [SerializeField]
    protected Transform GroundCheck;
    [SerializeField]
    protected Transform FrontCheck;
    protected Transform Model;

    [SerializeField]
    protected Define.State _state = Define.State.Idle;
    public virtual Define.State State
    {
        get { return _state; }
        set
        {
            _state = value;
            if (anim == null)
                TryGetComponent(out anim);
        }
    }

    protected Vector2 moveInput;
    public Vector3 moveDir { get; protected set; }
    public float nextFrameMSpeed { get; set; }

    protected Ray ray = new Ray();
    public Vector3 _destPos;
    
    protected Vector3 DestPos;
    protected Quaternion DestRot;

    protected int GWMask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Wall);
    protected int GroundMask = (1 << (int)Define.Layer.Ground);
    protected int WallMask = (1 << (int)Define.Layer.Wall);

    #region RB 전용 옵션
    [SerializeField]
    protected float RAY_DISTANCE = 2f;
    protected RaycastHit slopeHit;
    #endregion

    [SerializeField]
    protected GameObject target;

    [Header("RB 사용시 필요")]
    public float maxSlopeAngle = 45f;

    public abstract bool SetGetKinematic { get; set; }

    public abstract bool SetGetGravtiy { get; set; }

    public abstract bool SetGetColliderTrigger { get; set; }

    public abstract bool SetGetColliderActive { get; set; }

    public abstract Vector2 SetGetColiiderSize { get; set; }

    protected bool isPhysicsP;
    protected bool isPhysicsR;
    protected bool isCC;
    protected bool isSlow;
    
    protected float preSlowSpeed;

    public IEnumerator ppCoroutine { get; private set; }
    public IEnumerator prCoroutine { get; private set; }
    public IEnumerator ccCoroutine { get; private set; }
    public IEnumerator slCoroutine { get; private set; }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(SetGetKinematic);
            stream.SendNext(SetGetGravtiy);
            stream.SendNext(SetGetColliderTrigger);
            stream.SendNext(SetGetColliderActive);
            stream.SendNext(SetGetColiiderSize);
            stream.SendNext(team);
        }
        else
        {
            SetGetKinematic = (bool)stream.ReceiveNext();
            SetGetGravtiy = (bool)stream.ReceiveNext();
            SetGetColliderTrigger = (bool)stream.ReceiveNext();
            SetGetColliderActive = (bool)stream.ReceiveNext();
            SetGetColiiderSize = (Vector2)stream.ReceiveNext();
            team = (Define.Team)stream.ReceiveNext();
        }
    }

    protected void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        PV = GetComponent<PhotonViewEx>();
        anim = GetComponent<Animator>();
        stat = GetComponent<Stat>();
        soundList = GetComponent<SoundEvent>();
        if (TryGetComponent(out equipmentWindowData))
            equipmentWindowData.Init(stat);
        if (TryGetComponent(out Weapon w))
        {
            weapon = w;
            weapon.Init(this);
        }

        State = Define.State.Idle;

        SetMoveMethodComponent();

        GroundCheck = transform.Find("GroundCheck");
        FrontCheck = transform.Find("FrontCheck");

        Transform worldCanvas = transform.Find("Canvas");

        Transform nickTextMesh = transform.Find("NickNameTextMesh");
        if (nickTextMesh != null && nickTextMesh.TryGetComponent(out TextMesh textMesh))
            nickNameTextMesh = textMesh;

        if(worldCanvas != null)
        {
            Transform nickText = worldCanvas.Find("NickNameText");
            if (nickText != null && nickText.TryGetComponent(out Text text))
                nickNameText = text;
        }
     

        Managers.Game.SetPhotonObject(gameObject);
        SetTeam();
    }

    protected abstract void SetMoveMethodComponent();

    public virtual void SetNickNameViewInit()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == PV.Owner.CustomProperties["Team"].ToString())
        {
            if(PV.IsMine)
            {
                if (nickNameTextMesh != null)
                    nickNameTextMesh.color = Color.green;
                else if (nickNameText != null)
                    nickNameText.color = Color.green;
            }
            else
            {
                if (nickNameTextMesh != null)
                    nickNameTextMesh.color = teamColor;
                else if (nickNameText != null)
                    nickNameText.color = teamColor;
            }
        }
        else
        {
            if (nickNameTextMesh != null)
                nickNameTextMesh.color = enemyColor;
            else if (nickNameText != null)
                nickNameText.color = enemyColor;
        }
    }

    public virtual void SetTeamNickNameViewInit(bool isAll = false)
    {

        if (isAll) // true일 경우 팀 상관없이 보여줌
        {
            if (nickNameTextMesh != null)
                nickNameTextMesh.transform.gameObject.SetActive(true);
            else if (nickNameText != null)
                nickNameText.transform.gameObject.SetActive(true);
        }
        else // false일 경우 같은팀만 보여줌
        {
            if (Managers.Game.isTeamMode)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == PV.Owner.CustomProperties["Team"].ToString())
                {
                    if (nickNameTextMesh != null)
                        nickNameTextMesh.transform.gameObject.SetActive(true);
                    else if (nickNameText != null)
                        nickNameText.transform.gameObject.SetActive(true);

                    return;
                }
            }

            if (nickNameTextMesh != null)
                nickNameTextMesh.transform.gameObject.SetActive(false);
            else if (nickNameText != null)
                nickNameText.transform.gameObject.SetActive(false);
        }
    }

    public virtual void SetNickNameView(bool isShow)
    {
        if (Managers.Game.isTeamMode)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == PV.Owner.CustomProperties["Team"].ToString())
            {
                if (nickNameTextMesh != null)
                    nickNameTextMesh.transform.gameObject.SetActive(isShow);
                else if (nickNameText != null)
                    nickNameText.transform.gameObject.SetActive(isShow);
            }
        }
        else
        {
            if (nickNameTextMesh != null)
                nickNameTextMesh.transform.gameObject.SetActive(isShow);
            else if (nickNameText != null)
                nickNameText.transform.gameObject.SetActive(isShow);
        }
    }

    #region 상태이상 중첩x 취소o 슬로우만 취소x
    [PunRPC]
    public virtual void SetPhysics(int hitPlayerId, int type, int dir, float pow, float duration) { }

    public virtual void SetPhysics(int type, Vector3 dir, float pow, float duration) { }

    public void PhysicsStateP(Vector3 pow, float duration)
    {
        if (!isPhysicsP)
        {
            ppCoroutine = PhysicsP(pow, duration);
            StartCoroutine(ppCoroutine);
        }
    }

    public void CancelPhysicsPState()
    {
        if (ppCoroutine != null)
        {
            StopCoroutine(ppCoroutine);
            ppCoroutine = null;
            isPhysicsP = false;
        }
    }

    public void PhysicsStateR(Vector3 pow, float duration)
    {
        if (!isPhysicsR)
        {
            ppCoroutine = PhysicsR(pow, duration);
            StartCoroutine(prCoroutine);
        }
    }

    public void CancelPhysicsRState()
    {
        if (prCoroutine != null)
        {
            StopCoroutine(prCoroutine);
            prCoroutine = null;
            isPhysicsR = false;
        }
    }

    public abstract void ApplyOutPhysics(Vector3 pow, Define.PhysicsType type);

    public void CCState(float duration)
    {
        if (!isCC)
        {
            ccCoroutine = CC(duration);
            StartCoroutine(ccCoroutine);
        }
    }

    public void CancelCC()
    {
        if (ccCoroutine != null)
        {
            StopCoroutine(ccCoroutine);
            ccCoroutine = null;
            if (State == Define.State.CC)
                State = Define.State.Idle;
            isCC = false;
        }
    }

    public void SlowState(float duration, float _slowSpeed)
    {
        if (!isSlow)
        {
            slCoroutine = Slow(duration, _slowSpeed);
            StartCoroutine(slCoroutine);
        }
    }

    public void CancelSlow()
    {
        if (slCoroutine != null)
        {
            StopCoroutine(slCoroutine);
            slCoroutine = null;
            stat.MoveSpeed += preSlowSpeed;
            preSlowSpeed = 0;
            isSlow = false;
        }
    }

    protected abstract IEnumerator PhysicsP(Vector3 pow, float duration);

    protected abstract IEnumerator PhysicsR(Vector3 pow, float duration);

    IEnumerator CC(float duration)
    {
        State = Define.State.CC;
        isCC = true;
        yield return new WaitForSeconds(duration);
        if (State == Define.State.CC)
            State = Define.State.Idle;
        isCC = false;
    }

    IEnumerator Slow(float duration, float _slowSpeed)
    {
        isSlow = true;
        preSlowSpeed = _slowSpeed;
        stat.MoveSpeed -= _slowSpeed;
        yield return new WaitForSeconds(duration);
        stat.MoveSpeed += _slowSpeed;
        preSlowSpeed = 0;
        isSlow = false;
    }
    #endregion

    protected virtual void Input() { }
    protected virtual Vector3 GetMoveDir() { return Vector3.zero; }
    protected abstract bool FGroundCheck();
    protected virtual bool IsOnSlope()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out slopeHit, RAY_DISTANCE, GWMask))
        {
            var angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            isOnSlope = (angle != 0f && angle < maxSlopeAngle);
            return isOnSlope;
        }

        isOnSlope = false;
        return isOnSlope;
    }

    protected virtual void PhysicsCheck() { }

    protected virtual bool JumpCheck() {
        // 기본은 1단 점프
        // 바꾸고 싶으면 재정의
        if (isGround && stat.JumpCount > 0)
        {
            stat.JumpCount--;
            StartCoroutine(ResetJC());
            return true;
        }


        return false;
    }

    protected IEnumerator ResetJC()
    {
        isJump = true;
        yield return new WaitForSeconds(0.5f);
        isJump = false;
    }

    protected virtual void FixedUpdateState() { }
    protected virtual void LateUpdateState() { }

    protected virtual void UpdateState() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMoving() { }
    protected virtual void UpdateRunning() { }
    protected virtual void UpdateJumping() { }
    protected virtual void UpdateSkill() { }
    protected virtual void UpdateAttack() { }

    protected virtual void UpdateDie() { }
    protected virtual void UpdateSyncValue() { }

    protected virtual void OnDeadEnd()
    {
        if (PV.IsMine)
            Invoke("DestoryObject", 3f);
    }

    protected virtual void DestoryObject()
    {
        Managers.Resource.PhotonDestroy(gameObject);
    }
    protected virtual void OnDestroy() { }

    protected void SetAnimSpeed(string key, float value)
    {
        anim.SetFloat(key, SlowAction ? value : (value > 1 ? value : 1));
    }

    protected void SetTeam()
    {
        if (!PV.IsRoomView)
        {
            if(Managers.Game.isTeamMode)
            {
                string myteam = PV.Owner.CustomProperties["Team"].ToString();
                switch (myteam)
                {
                    case "RedTeam":
                        team = Define.Team.RedTeam;
                        break;
                    case "BlueTeam":
                        team = Define.Team.BlueTeam;
                        break;
                    default:
                        team = Define.Team.None;
                        break;
                }
            }
            else
            {
                team = Define.Team.None;
            }
        
        }
        else
        {
            team = Define.Team.BotTeam;
        }
    }

    [PunRPC]
    protected virtual void SetSkin(int index)
    {
        Model.GetChild(index).gameObject.SetActive(true);
    }
}
