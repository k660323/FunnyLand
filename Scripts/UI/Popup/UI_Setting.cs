using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    bool isLobby = false;
    bool isRoom = false;

    enum Buttons
    {
        PreferencesButton,
        PreviewSceneButton,
        ExitButton,
        CloseButton
    }

    public override void Init()
    {
        base.Init();
        Managers.Sound.Play2D("SFX/UI_Pop");
        Bind<Button>(typeof(Buttons));

        Get<Button>((int)Buttons.PreferencesButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            Preferences(); 
        });
        Get<Button>((int)Buttons.PreviewSceneButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            PreviewWindow(); 
        });
        Get<Button>((int)Buttons.ExitButton).gameObject.BindEvent(data => {
            Managers.Sound.Play2D("SFX/UI_Click");
            ExitWindow(); 
        });
        Get<Button>((int)Buttons.CloseButton).gameObject.BindEvent((data) => {
            Managers.Sound.Play2D("SFX/UI_Click"); 
            ClosePopupUI();
        });

        Managers.Input.IsControll = false;
    }

    void Preferences()
    {
        // �׷��� ������Ʈ Ȱ��ȭ
        Managers.UI.ShowPopupUI<UI_Preferences>("UI_Preferences", true);
    }

    void PreviewWindow()
    {
        Define.Scene type = Managers.Scene.CurrentScene.SceneType;
        if(type == Define.Scene.Lobby)
        {
            UI_Notice notice = Util.SimplePopup("�α׾ƿ� �Ͻðڽ��ϱ�?");
            notice.ActiveCancelBtn();
            notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D); if (PhotonNetwork.InLobby) { isLobby = true; PhotonNetwork.LeaveLobby(); }; });
        }
        else if(type == Define.Scene.Game)
        {
            UI_Notice notice = Util.SimplePopup("������ ���� �������Դϴ�.\n ���� �����ðڽ��ϱ�?");
            notice.ActiveCancelBtn();
            notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) => { Managers.Sound.Play2D("SFX/UI_Click", Define.Sound2D.Effect2D); isRoom = true; PhotonNetwork.LeaveRoom(); });
        }
    }

    public override void OnLeftLobby()
    {
        if (isLobby)
            Managers.Scene.LeaveLobby();
    }

    public override void OnLeftRoom()
    {
        if (isRoom)
            Managers.Scene.LeaveRoom();
    }

    void ExitWindow()
    {
        UI_Notice notice = Util.SimplePopup("UI_Notice");
        notice.ActiveCancelBtn();
        notice.Get<Button>((int)UI_Notice.Buttons.OKButton).gameObject.CoverBindEvent((data) =>
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif  
        });
        notice.SetContent("���� ������ \n ���� �Ͻðڽ��ϱ�?");
    }

    private void OnDestroy()
    {
        Managers.Input.IsControll = true;
    }
}
