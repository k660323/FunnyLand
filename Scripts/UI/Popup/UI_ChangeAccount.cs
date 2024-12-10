using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeAccount : UI_Popup
{
    State state = State.NickNameChange;

    enum State
    {
        NickNameChange,
        PassWordChange,
        RegisterCancel
    }

    enum Toggles
    {
        NickNameChange,
        PassWordChange,
        RegisterCancel
    }

    enum InputFields
    {
        InputField_NickName,
        InputField_CurPW,
        InputField_PW,
        InputField_PWConfirm
    }

    enum Buttons
    {
        ConfirmButton,
        CloseButton
    }

    enum Images
    {
        CheckCurPW,
        CheckPW,
        CheckPWConfirm
    }

    public override void Init()
    {
        base.Init();

        Bind<Toggle>(typeof(Toggles));
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        Get<Toggle>((int)Toggles.NickNameChange).onValueChanged.AddListener(value => { if (!value) return; state = State.NickNameChange; BtnState(); });
        Get<Toggle>((int)Toggles.PassWordChange).onValueChanged.AddListener(value => { if (!value) return; state = State.PassWordChange; BtnState(); });
        Get<Toggle>((int)Toggles.RegisterCancel).onValueChanged.AddListener(value => { if (!value) return; state = State.RegisterCancel; BtnState(); });

        Get<Button>((int)Buttons.ConfirmButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            Confirm();
        });
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent(data =>{
            Managers.Sound.Play2D("SFX/UI_Click");
            ClosePopupUI();
        });

        Get<Image>((int)Images.CheckCurPW).gameObject.BindEvent(data =>
        {
            Get<InputField>((int)InputFields.InputField_CurPW).contentType = InputField.ContentType.Standard;
            Get<InputField>((int)InputFields.InputField_CurPW).ForceLabelUpdate();
        }, Define.UIEvent.Enter);

        Get<Image>((int)Images.CheckCurPW).gameObject.BindEvent(data =>
        {
            Get<InputField>((int)InputFields.InputField_CurPW).contentType = InputField.ContentType.Password;
            Get<InputField>((int)InputFields.InputField_CurPW).ForceLabelUpdate();
        }, Define.UIEvent.Exit);

        Get<Image>((int)Images.CheckPW).gameObject.BindEvent(data =>
        {
            Get<InputField>((int)InputFields.InputField_PW).contentType = InputField.ContentType.Standard;
            Get<InputField>((int)InputFields.InputField_PW).Select();
            Get<InputField>((int)InputFields.InputField_PW).ForceLabelUpdate();
        }, Define.UIEvent.Enter);

        Get<Image>((int)Images.CheckPW).gameObject.BindEvent(data =>
        {
            Get<InputField>((int)InputFields.InputField_PW).contentType = InputField.ContentType.Password;
            Get<InputField>((int)InputFields.InputField_PW).Select();
            Get<InputField>((int)InputFields.InputField_PW).ForceLabelUpdate();
        }, Define.UIEvent.Exit);

        Get<Image>((int)Images.CheckPWConfirm).gameObject.BindEvent(data =>
        {
            Get<InputField>((int)InputFields.InputField_PWConfirm).contentType = InputField.ContentType.Standard;
            Get<InputField>((int)InputFields.InputField_PWConfirm).Select();
            Get<InputField>((int)InputFields.InputField_PWConfirm).ForceLabelUpdate();
        }, Define.UIEvent.Enter);

        Get<Image>((int)Images.CheckPWConfirm).gameObject.BindEvent(data =>
        {
            Get<InputField>((int)InputFields.InputField_PWConfirm).contentType = InputField.ContentType.Password;
            Get<InputField>((int)InputFields.InputField_PWConfirm).Select();
            Get<InputField>((int)InputFields.InputField_PWConfirm).ForceLabelUpdate();
        }, Define.UIEvent.Exit);

        BtnState();
    }

    void BtnState()
    {
        switch (state)
        {
            case State.NickNameChange:
                Get<InputField>((int)InputFields.InputField_NickName).text = "";
                Get<InputField>((int)InputFields.InputField_PW).text = "";
                Get<InputField>((int)InputFields.InputField_PWConfirm).text = "";

                Get<InputField>((int)InputFields.InputField_NickName).gameObject.SetActive(true);
                Get<InputField>((int)InputFields.InputField_CurPW).gameObject.SetActive(false);
                Get<InputField>((int)InputFields.InputField_PW).gameObject.SetActive(true);
                Get<InputField>((int)InputFields.InputField_PWConfirm).gameObject.SetActive(true);
                break;
            case State.PassWordChange:
                Get<InputField>((int)InputFields.InputField_CurPW).text = "";
                Get<InputField>((int)InputFields.InputField_PW).text = "";
                Get<InputField>((int)InputFields.InputField_PWConfirm).text = "";

                Get<InputField>((int)InputFields.InputField_NickName).gameObject.SetActive(false);
                Get<InputField>((int)InputFields.InputField_CurPW).gameObject.SetActive(true);
                Get<InputField>((int)InputFields.InputField_PW).gameObject.SetActive(true);
                Get<InputField>((int)InputFields.InputField_PWConfirm).gameObject.SetActive(true);
                break;
            case State.RegisterCancel:
                Get<InputField>((int)InputFields.InputField_PW).text = "";
                Get<InputField>((int)InputFields.InputField_PWConfirm).text = "";

                Get<InputField>((int)InputFields.InputField_NickName).gameObject.SetActive(false);
                Get<InputField>((int)InputFields.InputField_CurPW).gameObject.SetActive(false);
                Get<InputField>((int)InputFields.InputField_PW).gameObject.SetActive(true);
                Get<InputField>((int)InputFields.InputField_PWConfirm).gameObject.SetActive(true);
                break;
        }
    }

    void Confirm()
    {
        switch (state)
        {
            case State.NickNameChange:
                {
                    UI_Notice notice = Util.SimplePopup("닉네임을 변경 하시겠습니까? \n 비용은 500코인 입니다.");
                    notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) =>
                    {
                        if (Util.PurchaseItemCheck(Define.GoodsType.COIN, 500))
                        {
                            string nickName = Get<InputField>((int)InputFields.InputField_NickName).text;
                            string pw = Get<InputField>((int)InputFields.InputField_PW).text;
                            string pwConfirm = Get<InputField>((int)InputFields.InputField_PWConfirm).text;

                            if (!PasswordCheck(pw, pwConfirm))
                                return;
                            if (!NickNameCheck(nickName))
                                return;
                            if(Util.PurchaseItemResult(Define.GoodsType.COIN, 500))
                                ChangeNickName(nickName);                   
                        }
                        else
                        {
                            Util.SimplePopup("코인이 부족합니다.");
                        }

                    });
                    notice.ActiveCancelBtn();
                }
                break;
            case State.PassWordChange:
                {
                    string curPW = Get<InputField>((int)InputFields.InputField_CurPW).text;
                    string pw = Get<InputField>((int)InputFields.InputField_PW).text;
                    string pwConfirm = Get<InputField>((int)InputFields.InputField_PWConfirm).text;

                    ChangePassWord(curPW, pw, pwConfirm);
                }
                break;
            case State.RegisterCancel:
                {
                    string pw = Get<InputField>((int)InputFields.InputField_PW).text;
                    string pwConfirm = Get<InputField>((int)InputFields.InputField_PWConfirm).text;

                    if (!PasswordCheck(pw, pwConfirm))
                        return;
                    RegisterCancel();
                }
                break;
        }
    }

    bool PasswordCheck(string _pw,string _pwConfirm)
    {
        if (_pw == _pwConfirm)
        {
            var result = Backend.BMember.ConfirmCustomPassword(_pw);
            if (result.IsSuccess())
            {
                return true;
            }
            else
            {
                Util.SimplePopup("계정의 비밀번호가 아닙니다.");
            }
        }
        else
        {
            Util.SimplePopup("비밀번호가 서로 일치 하지 않습니다.");
        }

        return false;
    }

    bool NickNameCheck(string nickName)
    {
        var result = Backend.BMember.CheckNicknameDuplication(nickName);
        if(result.IsSuccess())
        {
            return true;
        }
        else
        {
            Util.SimplePopup("중복되는 닉네임이 존재합니다.");
            return false;
        }
    }

    bool ChangeNickName(string nickName)
    {
        var result = Backend.BMember.UpdateNickname(nickName);
        if(result.IsSuccess())
        {
            Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
            UI_Notice notice = Util.SimplePopup("닉네임이 성공적으로 변경되었습니다.");
            notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) => CloseAllPopupUI());
            return true;
        }
        else
        {
            Util.SimplePopup("서버 오류로 변경되지 않았습니다.");
            return false;
        }
        
    }

    bool ChangePassWord(string _oldPW, string _newPW, string _newPWConfirm)
    {
        if(_newPW == _newPWConfirm)
        {
            var result = Backend.BMember.UpdatePassword(_oldPW, _newPW);
            if (result.IsSuccess())
            {
                Util.SimplePopup("성공적으로 비밀번호가 변경되었습니다.");
                return true;
            }
            else
            {
                Util.SimplePopup("기존 비밀번호가 올바르지 않습니다.");
                return false;
            }
        }
        else
        {
            Util.SimplePopup("새로운 비밀번호와 재확인용 비밀번호가 일치하지 않습니다.");
            return false;
        }
      
    }

    bool RegisterCancel()
    {
        var result = Backend.BMember.WithdrawAccount(24 * 7);
        if(result.IsSuccess())
        {
            Util.SimplePopup("회원 탈퇴가 등록되었습니다.\n 일주일뒤에 완전히 삭제됩니다.\n 취소를 원하실 경우 재접속 하시면 됩니다.");
            return true;
        }
        else
        {
            Util.SimplePopup("네트워크 에러");
            return false;
        }
    }
}
