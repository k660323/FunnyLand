using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Register : UI_Base
{
    public enum InputFields
    {
        InputField_Email,
        InputField_ID,
        InputField_PW
    }

    public enum Buttons
    {
        CloseButton,
        CreateButton,
        CancleButton
    }

    public override void Init()
    {
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.CreateButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D);
            CreateAccount();
        });
        GetButton((int)Buttons.CancleButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });
        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(data =>
        {
            Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D);
            gameObject.SetActive(false);
        });
    }

    void CreateAccount()
    {
        string email = Get<InputField>((int)InputFields.InputField_Email).text;
        string id = Get<InputField>((int)InputFields.InputField_ID).text;
        string pw = Get<InputField>((int)InputFields.InputField_PW).text;

        if (isValidEmail(email))
            return;
        SetAccount(email, id, pw);
    }

    bool isValidEmail(string email)
    {
        // �̸��� �ߺ� Ȯ��
        var result = Backend.BMember.FindCustomID(email);
        if (result.IsSuccess())
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            FailSign("�����ϴ� ����(404)");
        }

        return result.IsSuccess();
    }
    void SetAccount(string email, string id, string pw)
    {
        var result = Backend.BMember.CustomSignUp(id, pw);

        if (result.IsSuccess())
        {
            // 1. �������
            SetCountry();

            // 2. �̸��� ���
            SetEmail(email);

            // ���� ���� �˸�
            SuccessSign();
        }
        else
        {
            // ���� ���� �˸�
            FailSign("ȸ�� ���� ���� \n(�ߺ��� ID �Ǵ� ��Ʈ��ũ ����)");
        }
    }

    
    bool SetCountry()
    {
        var result = Backend.BMember.UpdateCountryCode(BackEnd.GlobalSupport.CountryCode.SouthKorea);
        if (!result.IsSuccess())
        {
            FailSign("���� ���� ����(204)");
        }

        return result.IsSuccess();
    }

    bool SetEmail(string email)
    {
        var result2 = Backend.BMember.UpdateCustomEmail(email);
        return result2.IsSuccess();
    }

    void SuccessSign()
    {
        Managers.Sound.Play2D("SFX/UI_Success");
        Util.SimplePopup("ȸ�� ���� ����");
        gameObject.SetActive(false);
    }

    void FailSign(string message)
    {
        Managers.Sound.Play2D("SFX/UI_Error");
        Util.SimplePopup(message);
    }
}
