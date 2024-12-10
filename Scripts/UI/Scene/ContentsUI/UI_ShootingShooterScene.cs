using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShootingShooterScene : UI_ContentsScene
{
    ShootingShooterScene SSS;
    public UI_KillBoard ukb;
    public enum CanvasGroups
    {
        Timer,
        RespawnPanel
    }

    enum Texts
    {
        TimerText,
        ReSpawnText
    }

    enum Images
    {
        RespawnGauge
    }

    public override void Init()
    {
        base.Init();
      
        SSS = GameObject.FindGameObjectWithTag("ContentsScene").GetComponent<ShootingShooterScene>(); ;
        ukb = GameObject.FindGameObjectWithTag("UI_KillBoard").GetComponent<UI_KillBoard>();

        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
    }

    public override void LateInit()
    {
        Managers.Game.ContentsScene.gameTimeUIEvent -= (value) => {
            Get<Text>((int)Texts.TimerText).text = Managers.Game.gameScene.SecondsToTime(value);
            Get<CanvasGroup>((int)CanvasGroups.Timer).alpha = 1;
        };
        Managers.Game.ContentsScene.gameTimeUIEvent += (value) => {
            Get<Text>((int)Texts.TimerText).text = Managers.Game.gameScene.SecondsToTime(value);
            Get<CanvasGroup>((int)CanvasGroups.Timer).alpha = 1;
        };
    }


    public IEnumerator ReSpawnRoutine(float spawnTime = 10f)
    {
        Get<CanvasGroup>((int)CanvasGroups.RespawnPanel).alpha = 1;

        float curGauge = 0f;
        float maxGauge = spawnTime;
        while (curGauge < maxGauge)
        {
            curGauge += Time.deltaTime;
            Get<Text>((int)Texts.ReSpawnText).text = $"{(int)(maxGauge - curGauge)}초 뒤에 부활 합니다.";
            Get<Image>((int)Images.RespawnGauge).fillAmount = curGauge / maxGauge;
            yield return null;
        }

        BaseController bc = Managers.Game.myPlayer.GetComponent<BaseController>();
        bc.stat.Hp = bc.stat.MaxHp;
        bc.SetGetKinematic = false;
        bc.SetGetColliderActive = true;
        bc.transform.position = Managers.Game.spawner.spawnList[Random.Range(0, Managers.Game.spawner.spawnList.Length)].transform.position;
        bc.PV.RPC("TriggerAnim", RpcTarget.AllViaServer, "isResurrection");
        bc.State = Define.State.Idle;
        Get<CanvasGroup>((int)CanvasGroups.RespawnPanel).alpha = 0;
    }
}
