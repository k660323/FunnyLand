using BackEnd;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Managers : MonoBehaviourPunCallbacks
{
    static Managers instacne;
    public static Managers Instance
    {
        get
        {
            Init();
            return instacne;
        }
    }

    #region Contents
    GameCursorManager gameCursor = new GameCursorManager();
    GameOptionManager setting = new GameOptionManager();
    GameManager game = new GameManager();
    QuestSystem quest = new QuestSystem();

    public static GameCursorManager GameCursor { get { return Instance.gameCursor; } }
    public static GameOptionManager Setting { get { return Instance.setting; } }

    public static GameManager Game { get { return Instance.game; } }

    public static QuestSystem Quest { get { return Instance.quest; } }
    #endregion

    #region Core
    DataManager data = new DataManager();
    InputManager input = new InputManager();
    PhotonNetworkManager photon = new PhotonNetworkManager();
    PoolManager pool = new PoolManager();
    ResourceManager resource = new ResourceManager();
    SceneManagerEx scene = new SceneManagerEx();
    SoundManager sound = new SoundManager();
    ParticleManager particle = new ParticleManager();
    UIManager ui = new UIManager();

    public static DataManager Data { get { return Instance.data; } }
    public static InputManager Input { get { return Instance.input; } }
    public static PhotonNetworkManager Photon { get { return Instance.photon; } }
    public static PoolManager Pool { get { return Instance.pool; } }
    public static ResourceManager Resource { get { return Instance.resource; } }
    public static SceneManagerEx Scene { get { return Instance.scene; } }
    public static SoundManager Sound { get { return Instance.sound; } }
    public static ParticleManager Paticle { get { return Instance.particle; } }
    public static UIManager UI { get { return Instance.ui; } }
    #endregion

    static void Init()
    {
        if(instacne == null)
        {
            var go = FindObjectOfType<Managers>();
            if(go == null)
            {
                go = new GameObject() { name = "@Managers" }.AddComponent<Managers>();
                instacne = go;
            }
            else
            {
                instacne = go;
            }
            
            instacne.gameCursor.Init();
            instacne.input.Init();
            instacne.setting.Init();
            instacne.data.Init();
            instacne.photon.Init();
            instacne.pool.Init();
            instacne.sound.Init();
        }
    }

    public static void Clear()
    {
        GameCursor.Clear();
        Input.Clear();
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Game.Clear();
        Pool.Clear();
        Pool.ClearRpc();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 초기화
        Init();
        if (instacne != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        Setting.sOption.ApplyOption(false);

        Backend.InitializeAsync(false, callback => {
            if (!callback.IsSuccess())
            {
                // 초기화 실패 시 로직
                //TODO 에러메시지 뛰우며 애플리케이션 종료시키기
                var notice = Util.SimplePopup("연결 실패");
                notice.CoverBindEvent<Button>((int)UI_Notice.Buttons.OKButton, data => { Application.Quit(); notice.ClosePopupUI(); });
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        Backend.AsyncPoll();
        input.OnUpdate();
    }

    public T FindObject<T>() where T : Component
    {
        return FindObjectOfType<T>();
    }

    private void OnApplicationQuit()
    {
        Clear();

        if (Backend.BMember != null && Backend.UserNickName != "")
            Backend.BMember.Logout();
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }
}
