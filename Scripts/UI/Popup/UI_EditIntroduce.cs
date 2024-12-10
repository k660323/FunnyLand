using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_EditIntroduce : UI_Popup
{
    enum InputFields
    {
        ContentInputField
    }

    enum Buttons
    {
        OkButton,
        CloseButton
    }

    UnityAction<string> editAction;

    public override void Init()
    {
        base.Init();

        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            ClosePopupUI();
        });
    }

    public void SetAction(UnityAction<string> action)
    {
        editAction -= action;
        editAction += action;
        Get<Button>((int)Buttons.OkButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click");
            editAction.Invoke(Get<InputField>((int)InputFields.ContentInputField).text);
        });
    }
}
