using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public abstract class BotController3D : Controller3D
{
    [SerializeField]
    protected string botNickName;
    [SerializeField]
    protected float thinkTime;
    [SerializeField]
    protected float maxCheckCounter = 100f;

    protected IEnumerator actionroutine;

    public override void Init()
    {
        base.Init();
        nav = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        baseCollider3D = GetComponent<CapsuleCollider>();

        if (nickNameTextMesh != null)
            nickNameTextMesh.text = botNickName;
        else if (nickNameText != null)
            nickNameText.text = botNickName;

        if (PV.IsMine)
        {
            Managers.Game.ContentsScene.contentFixedAction -= FixedUpdateState;
            Managers.Game.ContentsScene.contentFixedAction += FixedUpdateState;
            StartCoroutine(AIActive());
        }
        else
        {
            Managers.Game.ContentsScene.contentUpdateAction -= UpdateSyncValue;
            Managers.Game.ContentsScene.contentUpdateAction += UpdateSyncValue;
        }
    }

    protected abstract IEnumerator AIActive();

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(newMasterClient == PhotonNetwork.LocalPlayer)
        {
            Managers.Game.ContentsScene.contentUpdateAction -= UpdateSyncValue;
            Managers.Game.ContentsScene.contentFixedAction += FixedUpdateState;
            StartCoroutine(AIActive());
        }
    }

    protected override void OnDestroy()
    {
        if (!PV.IsMine)
        {
            if (Managers.Game.ContentsScene != null)
                Managers.Game.ContentsScene.contentUpdateAction -= UpdateSyncValue;
        }
        else
        {
            if (Managers.Game.ContentsScene != null)
                Managers.Game.ContentsScene.contentFixedAction -= FixedUpdateState;
        }
    }
}
