using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_CreateNickName : UI_Popup
{
    enum InputFields
    {
        InputField_NickName
    }

    enum Buttons
    {
        SetNameButton,
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<InputField>(typeof(InputFields));

        Get<Button>((int)Buttons.SetNameButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            CreateNickName();
        });
    }

    void CreateNickName()
    {
        string name = Get<InputField>((int)InputFields.InputField_NickName).text;
        Get<InputField>((int)InputFields.InputField_NickName).text = "";
        if (name.Length < 2 || name.Length > 20)
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            Util.SimplePopup("닉네임을 2글자 이상, 20글자 이하로 \n 작성해주세요.");
        }
        else
        {
            var resultDuplication = Backend.BMember.CheckNicknameDuplication(name);

            if (resultDuplication.IsSuccess())
            {
                var resultCreate = Backend.BMember.CreateNickname(name);

                if (resultCreate.IsSuccess())
                {
                    Managers.Sound.Play2D("SFX/UI_Success");
                    UI_Notice notice = Util.SimplePopup($"{name} 이란 \n 이름이 성공적으로 생성 되었습니다!");
                    notice.CoverBindEvent<Button>((int)UI_Notice.Buttons.OKButton, data =>
                    {
                        Managers.Scene.CurrentScene.Connect();
                        CloseAllPopupUI();
                    });
                }
                else
                {
                    Managers.Sound.Play2D("SFX/UI_Error");
                    Util.SimplePopup($"네트워크 에러 \n 다시 시도해주세요!");
                }
            }
            else
            {
                Managers.Sound.Play2D("SFX/UI_Error");
                Util.SimplePopup("이미 사용중인 이름입니다.");
            }
        }
    }
}
