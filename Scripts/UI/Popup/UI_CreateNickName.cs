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
            Util.SimplePopup("�г����� 2���� �̻�, 20���� ���Ϸ� \n �ۼ����ּ���.");
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
                    UI_Notice notice = Util.SimplePopup($"{name} �̶� \n �̸��� ���������� ���� �Ǿ����ϴ�!");
                    notice.CoverBindEvent<Button>((int)UI_Notice.Buttons.OKButton, data =>
                    {
                        Managers.Scene.CurrentScene.Connect();
                        CloseAllPopupUI();
                    });
                }
                else
                {
                    Managers.Sound.Play2D("SFX/UI_Error");
                    Util.SimplePopup($"��Ʈ��ũ ���� \n �ٽ� �õ����ּ���!");
                }
            }
            else
            {
                Managers.Sound.Play2D("SFX/UI_Error");
                Util.SimplePopup("�̹� ������� �̸��Դϴ�.");
            }
        }
    }
}
