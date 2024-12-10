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
            notice.SetContent("메일 전송 완료 (해당 메일로 가서 확인해보세요!)");
        }
        else
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            if (result.GetStatusCode() == "404")
                notice.SetContent("해당 이메일로 등록된 계정이 없습니다.(error : 404)");
            else if (result.GetStatusCode() == "429")
                notice.SetContent("정보 보호를 위해 ID찾기/PW초기화는 하루 5회로 제한되어있어 \n 내일 다시 시도해 주세요.");
            else if (result.GetStatusCode() == "400")
                notice.SetContent("특수문자가 감지 되었습니다.");
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
            notice.SetContent("메일 전송 완료 (해당 메일로 가서 확인해보세요!)");
        }
        else
        {
            Managers.Sound.Play2D("SFX/UI_Error");
            if (result.GetStatusCode() == "404")
                notice.SetContent("ID,이메일이 없거나 잘못 입력되었습니다.(error : 404)");
            else if (result.GetStatusCode() == "400")
                notice.SetContent("잘못된 형식의 이메일 입니다.");
            else if (result.GetStatusCode() == "429")
                notice.SetContent("정보보호를 위해 ID찾기/PW초기화는 하루 5회로 제한되어있어 \n 내일 다시 시도해 주세요.");
        }
    }
}
