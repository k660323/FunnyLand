using System.Collections;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : BaseScene
{
    UI_LoginScene uls;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        uls = Managers.UI.ShowSceneUI<UI_LoginScene>("UI_LoginScene");
        Managers.Sound.Play2D("BGM/Login", Define.Sound2D.Bgm);
    }

    public override void Clear()
    {

    }
}
