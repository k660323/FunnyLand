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
                    UI_Notice notice = Util.SimplePopup("�г����� ���� �Ͻðڽ��ϱ�? \n ����� 500���� �Դϴ�.");
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
                            Util.SimplePopup("������ �����մϴ�.");
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
                Util.SimplePopup("������ ��й�ȣ�� �ƴմϴ�.");
            }
        }
        else
        {
            Util.SimplePopup("��й�ȣ�� ���� ��ġ ���� �ʽ��ϴ�.");
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
            Util.SimplePopup("�ߺ��Ǵ� �г����� �����մϴ�.");
            return false;
        }
    }

    bool ChangeNickName(string nickName)
    {
        var result = Backend.BMember.UpdateNickname(nickName);
        if(result.IsSuccess())
        {
            Managers.UI.SceneUI.UpdateSyncUI?.Invoke();
            UI_Notice notice = Util.SimplePopup("�г����� ���������� ����Ǿ����ϴ�.");
            notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) => CloseAllPopupUI());
            return true;
        }
        else
        {
            Util.SimplePopup("���� ������ ������� �ʾҽ��ϴ�.");
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
                Util.SimplePopup("���������� ��й�ȣ�� ����Ǿ����ϴ�.");
                return true;
            }
            else
            {
                Util.SimplePopup("���� ��й�ȣ�� �ùٸ��� �ʽ��ϴ�.");
                return false;
            }
        }
        else
        {
            Util.SimplePopup("���ο� ��й�ȣ�� ��Ȯ�ο� ��й�ȣ�� ��ġ���� �ʽ��ϴ�.");
            return false;
        }
      
    }

    bool RegisterCancel()
    {
        var result = Backend.BMember.WithdrawAccount(24 * 7);
        if(result.IsSuccess())
        {
            Util.SimplePopup("ȸ�� Ż�� ��ϵǾ����ϴ�.\n �����ϵڿ� ������ �����˴ϴ�.\n ��Ҹ� ���Ͻ� ��� ������ �Ͻø� �˴ϴ�.");
            return true;
        }
        else
        {
            Util.SimplePopup("��Ʈ��ũ ����");
            return false;
        }
    }
}
