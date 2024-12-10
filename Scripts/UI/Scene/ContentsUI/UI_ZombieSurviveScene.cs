using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ZombieSurviveScene : UI_ContentsScene
{
    ZombieSurviveScene ZSS;
    Stat stat;
    Weapon weapon;

    public enum CanvasGroups
    {
        Timer
    }

    public enum Texts
    {
        TimerText,
        HpText
    }

    public enum Sliders
    {
        HpSlider
    }

    public enum Images
    {
        AtkCool
    }

    public override void Init()
    {
        base.Init();
       
        ZSS = GameObject.FindGameObjectWithTag("ContentsScene").GetComponent<ZombieSurviveScene>();

        Bind<CanvasGroup>(typeof(CanvasGroups));
        Bind<Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));
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

    public void SetStatUI(Stat _stat)
    {
        stat = _stat;

        Managers.Game.ContentsScene.contentLateAction -= UpdateUI;
        Managers.Game.ContentsScene.contentLateAction += UpdateUI;
    }

    public void SetWeaponCoolUI(Weapon _weapon)
    {
        weapon = _weapon;

        Managers.Game.ContentsScene.contentLateAction -= UpdateWeaponUI;
        Managers.Game.ContentsScene.contentLateAction += UpdateWeaponUI;
    }

    void UpdateUI()
    {
        if (stat != null)
        {
            Get<Text>((int)Texts.HpText).text = $"HP : {stat.Hp} / {stat.MaxHp}";
            Get<Slider>((int)Sliders.HpSlider).value = (float)stat.Hp / stat.MaxHp;
        }
        else
        {
            Get<Text>((int)Texts.HpText).text = $"HP : -";
            Get<Slider>((int)Sliders.HpSlider).value = 0f;
        }
    }

    void UpdateWeaponUI()
    {
        if(weapon != null && weapon.curTime != 0f)
        {
            Get<Image>((int)Images.AtkCool).fillAmount = weapon.curTime / weapon.waitTime;
        }
    }
}
