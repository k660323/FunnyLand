using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public abstract class BotController2D : Controller2D
{
    [SerializeField]
    protected Define.Team team;
    [SerializeField]
    protected string botNickName;
    [SerializeField]
    protected float thinkTime;
    [SerializeField]
    protected float maxCheckCounter = 100f;
    protected bool isAction;


    protected abstract IEnumerator AIActive();

    public override void Init()
    {
        base.Init();
        baseCollider2D = GetComponent<Collider2D>();

        if (nickNameTextMesh != null)
            nickNameTextMesh.text = botNickName;
        else if (nickNameText != null)
            nickNameText.text = botNickName;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            Managers.Game.ContentsScene.contentUpdateAction -= UpdateSyncValue;
            Managers.Game.ContentsScene.contentFixedAction += FixedUpdateState;
            StartCoroutine(AIActive());
        }
    }

    protected override void OnDestroy()
    {
        if (Managers.Game.ContentsScene != null)
        {
            Managers.Game.ContentsScene.contentUpdateAction -= UpdateSyncValue;
            Managers.Game.ContentsScene.contentFixedAction -= FixedUpdateState;
            Managers.Game.ContentsScene.contentLateAction -= LateUpdateState;
        }
    }
}
