using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEvent : MonoBehaviourPunCallbacks
{
    string DieLocalPath = "SFX/Die/";
    [SerializeField]
    protected string _dieClipName;
    [SerializeField]
    protected float _dieMaxSound = 3f;

    string MoveLocalPath = "SFX/FootStep/";
    [SerializeField]
    protected string _moveClipName;
    [SerializeField]
    protected float _moveMaxSound = 3f;

    string HitLocalPath = "SFX/Hit/";
    [SerializeField]
    protected string _hitClipName;
    [SerializeField]
    protected float _hitMaxSound = 5f;

    public string DieClipName { get { return _dieClipName; } set { _dieClipName = value; } }
    public float DieMaxSound { get { return _dieMaxSound; } set { _dieMaxSound = value; } }

    public string MoveClipName { get { return _moveClipName; } set { _moveClipName = value; } }
    public float MoveMaxSound { get { return _moveMaxSound; } set { _moveMaxSound = value; } }

    public string HitClipName { get { return _hitClipName; } set { _hitClipName = value; } }
    public float HitMaxSound { get { return _hitMaxSound; } set { _hitMaxSound = value; } }

    protected void OnDieSound()
    {
        if (DieClipName != "")
        {
            Managers.Sound.Play3D(DieLocalPath + _dieClipName, transform.position + Vector3.up, "", Define.Sound3D.Effect3D, 1f, 0.5f, _dieMaxSound);
        }
    }

    protected void OnMoveSound()
    {
        if (MoveClipName != "")
        {
            Managers.Sound.Play3D(MoveLocalPath + MoveClipName, transform.position, "", Define.Sound3D.Effect3D, 1f, 0.1f, MoveMaxSound);
        }
    }

    [PunRPC]
    protected void OnHitSound()
    {
        if (HitClipName != "")
        {
            Managers.Sound.Play3D(HitLocalPath + HitClipName, transform.position, "", Define.Sound3D.Effect3D, 1f, 0.5f, HitMaxSound);
        }
    }
}
