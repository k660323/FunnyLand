using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    [HideInInspector]
    public UI_Register register;
    [HideInInspector]
    public UI_FindAccount findAccount;

    UI_Setting setting;

    enum Logins
    {
        Login,
    } 

    enum Buttons
    {
        SettingButton,
    }

    enum Texts
    {
        LoginSceneTitleText,
        VersionText
    }

    public override void Init()
    {
        base.Init();

        Bind<UI_Login>(typeof(Logins));
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        register = Util.FindChild<UI_Register>(gameObject);
        findAccount = Util.FindChild<UI_FindAccount>(gameObject);

        Get<Button>((int)Buttons.SettingButton).gameObject.BindEvent((data) =>
        {
            Managers.Sound.Play2D("SFX/UI_Click");
            if (setting == null)
                setting = Managers.UI.ShowPopupUI<UI_Setting>("Setting", true);
        });

        Get<Text>((int)Texts.VersionText).text = $"Ver {Managers.Photon.Version}";

        //StartCoroutine(ReinBowColor(Get<Text>((int)Texts.LoginSceneTitleText)));
    }

    IEnumerator ReinBowColor(Text text)
    {
        Color color = Random.ColorHSV();
        while (true)
        {
            Color color1 = text.color - color;
            if (color1.r < 0.001f && color1.g < 0.001f && color1.b < 0.001f)
                color = Random.ColorHSV();
            text.color = Color.Lerp(text.color, color, 0.5f * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
    }
}