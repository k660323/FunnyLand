using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GunWeapon : Weapon
{
    public Transform firePos;
    public GameObject projectile;
    public float speed;
    public float bulletLifeTime;

    SpriteRenderer sr;

    public override void Init(BaseController _bc)
    {
        base.Init(_bc);

        if (firePos == null)
            firePos = transform.Find("FirePos");


        if (_bc is Controller2D)
            sr = GetComponent<SpriteRenderer>();
    }

    public override void Attack()
    {
        if (Managers.Input.SpaceBar)
        {
            int rand = Random.Range(0, attackClipName.Length);
            bc.PV.RPC("Rpc3DSound", RpcTarget.AllViaServer, attackClipName[rand], transform.position, "", (int)Define.Sound3D.Effect3D, 1f, 1f, attackMaxSound[rand]);
            bc.State = Define.State.NormalAttack;
        }
    }

    public override void CoolAttack()
    {
        if (!isAtk && Managers.Input.SpaceBar)
        {
            int rand = Random.Range(0, attackClipName.Length);
            bc.PV.RPC("Rpc3DSound", RpcTarget.AllViaServer, attackClipName[rand], transform.position, "", (int)Define.Sound3D.Effect3D, 1f, 1f, attackMaxSound[rand]);
            bc.State = Define.State.NormalAttack;
            StartCoroutine(NormalAtkCool());
        }
    }

    protected override void OnAttack2D()
    {
        if (!bc.PV.IsMine || bc.stat.Hp <= 0)
            return;

        //TODO ÃÑ¾Ë ½ºÆù
        Vector2 dir = sr.flipX ? Vector2.left : Vector2.right;
        Vector2 startPos = transform.TransformPoint(new Vector2(dir.x * firePos.localPosition.x, firePos.localPosition.y));
        PhotonViewEx pv = Managers.Resource.PhotonInstantiate(projectile.name, startPos, Quaternion.identity, projectile.transform.localScale, Define.PhotonObjectType.PlayerObject, "PhotonPlayerGroup").GetComponent<PhotonViewEx>();
        pv.RPC("Init2D", RpcTarget.AllBufferedViaServer, bc.PV.ViewID, bc.stat.Attack, speed, dir, bulletLifeTime);
    }

    protected override void OnAttack()
    {
        if (!PhotonNetwork.IsMasterClient || bc.stat.Hp <= 0)
            return;

        //TODO ÃÑ¾Ë ½ºÆù
    }
}
