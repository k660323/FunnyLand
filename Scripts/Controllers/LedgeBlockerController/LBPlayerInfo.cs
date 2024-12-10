using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LBPlayerInfo : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonViewEx PV;
    [Header("Ledge")]
    [SerializeField]
    Ledge Ledge;
    [Header("Player")]
    public bool isDie = false;
    public float score;
    public float hit;
    public int combo;
    public float maxLifeGauge;
    public float lifeGauge;
    float decreaseSpeed = 5f;
    IEnumerator comboBreakCoroutine;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(isDie);
            stream.SendNext(score);
            stream.SendNext(hit);
            stream.SendNext(combo);
            stream.SendNext(lifeGauge);
            stream.SendNext(maxLifeGauge);
        }
        else
        {
            isDie = (bool)stream.ReceiveNext();
            score = (float)stream.ReceiveNext();
            hit = (float)stream.ReceiveNext();
            combo = (int)stream.ReceiveNext();
            lifeGauge= (float)stream.ReceiveNext();
            maxLifeGauge = (float)stream.ReceiveNext();
        }
    }

    void Awake()
    {
        PV = GetComponent<PhotonViewEx>();
    }

    public void Init()
    {
        isDie = false;
        score = 0;
        hit = 0;
        combo = 0;
        maxLifeGauge = 100f;
        lifeGauge = maxLifeGauge;
    }

    public void StartGaugeCoroutine()
    {
        StartCoroutine(DecreaseLifeSpeed());
        StartCoroutine(DecreaseLifeGauge());
    }


    public IEnumerator DecreaseLifeSpeed()
    {
        while (!isDie && !Managers.Game.ContentsScene.endFlag)
        {
            yield return new WaitForSeconds(1f);

            if (hit > 420)
            {
                decreaseSpeed = 15;
                yield break;
            }
            else if (hit > 370)
            {
                decreaseSpeed = 14;
            }
            else if (hit > 320)
            {
                decreaseSpeed = 13;
            }
            else if (hit > 270)
            {
                decreaseSpeed = 12;
            }
            else if (hit > 230)
            {
                decreaseSpeed = 11;
            }
            else if (hit > 170)
            {
                decreaseSpeed = 10;
            }
            else if (hit > 130)
            {
                decreaseSpeed = 9;
            }
            else if (hit > 90)
            {
                decreaseSpeed = 8;
            }
            else if (hit > 60)
            {
                decreaseSpeed = 7;
            }
            else if (hit > 30)
            {
                decreaseSpeed = 6;
            }
            else
            {
                decreaseSpeed = 5;
            }
        }
    }

    public IEnumerator DecreaseLifeGauge()
    {
        while (!isDie && !Managers.Game.ContentsScene.endFlag)
        {
            lifeGauge -= decreaseSpeed * Time.deltaTime;

            if (lifeGauge <= 0f)
            {
                isDie = true;
                Managers.Photon.SetPlayerPropertie("Die", true);
                Managers.Sound.Play2D("SFX/Warp Jingle", Define.Sound2D.Effect2D);
                var ULBS = Managers.UI.ContentsSceneUI as UI_LedgeBlockerScene;
                ULBS.SetBtnAction(false);
                StartCoroutine(Managers.Game.ChangeObserveMode());
            }

            yield return null;
        }
    }

    public void ComboBreak()
    {
        if (comboBreakCoroutine != null)
            StopCoroutine(comboBreakCoroutine);
        comboBreakCoroutine = BreakCombo();
        StartCoroutine(comboBreakCoroutine);
    }

    public IEnumerator BreakCombo()
    {
        yield return new WaitForSeconds(3f);
        combo = 0;
    }
}
