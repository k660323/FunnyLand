using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Login : UI_Base
{
    UI_LoginScene uLS;

    enum Login
    {
        InputField_ID,
        InputField_PW,
    }

    enum Buttons
    {
        LoginButton,
        RegisterButton,
        FindID,
        FindPW,
    }

    enum Toggles
    {
        IdRememberToggle
    }

    public override void Init()
    {
        uLS = Managers.UI.SceneUI as UI_LoginScene;

        Bind<InputField>(typeof(Login));
        Bind<Button>(typeof(Buttons));
        Bind<Toggle>(typeof(Toggles));

        GetButton((int)Buttons.LoginButton).gameObject.BindEvent(LoginBtnClick);
        GetButton((int)Buttons.RegisterButton).gameObject.BindEvent(RegisterBtnClick);
        GetButton((int)Buttons.FindID).gameObject.BindEvent(FindIDBtnClick);
        GetButton((int)Buttons.FindPW).gameObject.BindEvent(FindPWBtnClick);

        string registerID = PlayerPrefs.GetString("ID");
        if (registerID != "")
        {
            Get<Toggle>((int)Toggles.IdRememberToggle).isOn = true;
            Get<InputField>((int)Login.InputField_ID).text = registerID;
        }
    }

    void LoginBtnClick(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        string id = Get<InputField>((int)Login.InputField_ID).text;
        string pw = Get<InputField>((int)Login.InputField_PW).text;
        bool isSaveID = Get<Toggle>((int)Toggles.IdRememberToggle).isOn;

        var result = Backend.BMember.CustomLogin(id, pw);

        if (result.IsSuccess())
        {
            if (Managers.Data.PlayerInfoInit() && Managers.Data.PlayerInventoryInit())
            {
                if (isSaveID)
                    PlayerPrefs.SetString("ID", id);
                else
                    PlayerPrefs.DeleteKey("ID");

                Managers.Scene.LoadScene(Define.Scene.Lobby);
            }
            else
            {
                Managers.Scene.LeaveLobby();
            }
        }
        else
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            Util.SimplePopup("ID 또는 PW가 존재하지 않거나, 잘못 입력되었습니다.");
        }
    }

    void RegisterBtnClick(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        uLS.register.gameObject.SetActive(true);
    }

    void FindIDBtnClick(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        uLS.findAccount.OpenGroup(UI_FindAccount.State.ID);
    }

    void FindPWBtnClick(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        uLS.findAccount.OpenGroup(UI_FindAccount.State.PW);
    }
}