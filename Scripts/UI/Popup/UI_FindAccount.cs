using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_FindAccount : UI_Base
{
    State curState = State.NONE;
    public enum State
    {
        NONE,
        ID,
        PW
    }

    enum Texts
    {
        TitleText,
    }

    enum InputFields
    {
        InputField_ID,
        InputField_Email,
    }

    enum Buttons
    {
        SendButton,
        CloseButton
    }

    public override void Init()
    {
        Bind<Text>(typeof(Texts));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(CloseWindow);

        Get<InputField>((int)InputFields.InputField_ID).gameObject.SetActive(false);
        Get<InputField>((int)InputFields.InputField_Email).gameObject.SetActive(false);
    }

    void CloseWindow(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        curState = State.NONE;
        gameObject.SetActive(false);
        Get<InputField>((int)InputFields.InputField_ID).gameObject.SetActive(false);
        Get<InputField>((int)InputFields.InputField_Email).gameObject.SetActive(false);
    }

    public void OpenGroup(State state)
    {
        curState = state;
        gameObject.SetActive(true);

        if (state == State.ID)
        {
            Get<InputField>((int)InputFields.InputField_Email).gameObject.SetActive(true);
            CoverUIEvent(Get<Button>((int)Buttons.SendButton).gameObject, FindID);
        }
        else if (state == State.PW)
        {
            Get<InputField>((int)InputFields.InputField_Email).gameObject.SetActive(true);
            Get<InputField>((int)InputFields.InputField_ID).gameObject.SetActive(true);
            CoverUIEvent(Get<Button>((int)Buttons.SendButton).gameObject, ResetPW);
        }
    }

    void FindID(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        string email = Get<InputField>((int)InputFields.InputField_Email).text;
        var notice = Managers.UI.ShowPopupUI<UI_Notice>("UI_Notice");
        var result = Backend.BMember.FindCustomID(email);

        if (result.IsSuccess())
        {
            Managers.Sound.Play2D("SFX/UI_Success");
            notice.gameObject.BindEvent(CloseWindow);
            notice.SetContent("���� ���� �Ϸ� (�ش� ���Ϸ� ���� Ȯ���غ�����!)");
        }
        else
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            if (result.GetStatusCode() == "404")
                notice.SetContent("�ش� �̸��Ϸ� ��ϵ� ������ �����ϴ�.(error : 404)");
            else if (result.GetStatusCode() == "429")
                notice.SetContent("���� ��ȣ�� ���� IDã��/PW�ʱ�ȭ�� �Ϸ� 5ȸ�� ���ѵǾ��־� \n ���� �ٽ� �õ��� �ּ���.");
            else if (result.GetStatusCode() == "400")
                notice.SetContent("Ư�����ڰ� ���� �Ǿ����ϴ�.");
        }
    }
    void ResetPW(PointerEventData data)
    {
        Managers.Sound.Play2D("SFX/UI_Click");
        string id = Get<InputField>((int)InputFields.InputField_ID).text;
        string email = Get<InputField>((int)InputFields.InputField_Email).text;

        var notice = Managers.UI.ShowPopupUI<UI_Notice>("UI_Notice");

        var result = Backend.BMember.ResetPassword(id, email);
        if (result.IsSuccess())
        {
            Managers.Sound.Play2D("SFX/UI_Success");
            notice.gameObject.BindEvent(CloseWindow);
            notice.SetContent("���� ���� �Ϸ� (�ش� ���Ϸ� ���� Ȯ���غ�����!)");
        }
        else
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            if (result.GetStatusCode() == "404")
                notice.SetContent("ID,�̸����� ���ų� �߸� �ԷµǾ����ϴ�.(error : 404)");
            else if (result.GetStatusCode() == "400")
                notice.SetContent("�߸��� ������ �̸��� �Դϴ�.");
            else if (result.GetStatusCode() == "429")
                notice.SetContent("������ȣ�� ���� IDã��/PW�ʱ�ȭ�� �Ϸ� 5ȸ�� ���ѵǾ��־� \n ���� �ٽ� �õ��� �ּ���.");
        }
    }
}
