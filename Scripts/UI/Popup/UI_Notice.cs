using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Notice : UI_Popup
{
    enum Texts
    {
        ContentText
    }

    public enum Buttons
    {
        OKButton,
        CancelButton
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.OKButton).gameObject.BindEvent(data => { Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D); ClosePopupUI(); });
        Get<Button>((int)Buttons.CancelButton).gameObject.BindEvent(data => { Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D); ClosePopupUI(); });

        Get<Button>((int)Buttons.CancelButton).gameObject.SetActive(false);
    }

    public void ActiveCancelBtn()
    {
        Get<Button>((int)Buttons.CancelButton).gameObject.SetActive(true);
    }

    public void SetContent(string message)
    {
        Get<Text>((int)Texts.ContentText).text = message;
    }   
}
