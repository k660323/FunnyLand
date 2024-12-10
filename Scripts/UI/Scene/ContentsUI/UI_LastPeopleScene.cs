using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LastPeopleScene : UI_ContentsScene
{
    LastPeopleScene LPS;
    Stat stat;

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
        StaminaSlider
    }

    public enum Images
    {
        Fill
    }

    public override void Init()
    {
        base.Init();
        LPS = GameObject.FindGameObjectWithTag("ContentsScene").GetComponent<LastPeopleScene>();
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

    void UpdateUI()
    {
        if (stat != null)
        {
            Get<Text>((int)Texts.HpText).text = $"HP : {stat.Hp} / {stat.MaxHp}";

            float staminaPercent = (stat.Stamina / stat.MaxStamina);
            Get<Image>((int)Images.Fill).color = staminaPercent > 0.3f ? Color.yellow : Color.red;
            Get<Slider>((int)Sliders.StaminaSlider).value = staminaPercent;
        }
        else
        {
            Get<Text>((int)Texts.HpText).text = $"HP : -";
        }
    }
}
