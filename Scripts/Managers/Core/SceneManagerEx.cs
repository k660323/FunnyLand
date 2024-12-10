using BackEnd;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public LoadSceneMode loadSceneMode { get; private set; }

    public BaseScene CurrentScene{ get; private set; }
    public ContentsScene CurrentCScene { get; private set; }

    public void SetCurrentScene(BaseScene scene)
    {
        CurrentScene = scene;
    }

    public void SetCurrentCScene(string type, ContentsScene contentsScene)
    {
        if(SceneManager.SetActiveScene(SceneManager.GetSceneByName(type)))
            CurrentCScene = contentsScene;
    }

    #region 인덱스로 맵 로드
    // 싱글용(혼자 로딩)[동기] 씬 초기화, 씬 추가
    public void LoadScene(Define.Scene type,LoadSceneMode mode =  LoadSceneMode.Single)
    {
        loadSceneMode = mode;

        if (mode == LoadSceneMode.Single)
            Managers.Clear();

        SceneManager.LoadScene(GetSceneName(type), mode);
    }

    // 멀티(개별 로딩 시작, 다같이 로딩 시작)[동기] 씬 초기화
    public void PhotonLoadScene(Define.Scene type, bool isSync = true)
    {
        loadSceneMode = LoadSceneMode.Single;
        Managers.Clear();

        if (isSync)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
        }

        PhotonNetwork.AutomaticallySyncScene = isSync;
      
        PhotonNetwork.LoadLevel(GetSceneName(type));
    }

    // (비동기 로딩)[비동기] 씬 초기화, 씬 추가
    public AsyncOperation AsyncLoadScene(Define.Scene type, LoadSceneMode mode = LoadSceneMode.Single, bool isCompletedAndActvie = false)
    {
        loadSceneMode = mode;

        // 로딩 바로 시작
        AsyncOperation aSync = SceneManager.LoadSceneAsync(GetSceneName(type), new LoadSceneParameters { loadSceneMode = mode, localPhysicsMode = LocalPhysicsMode.Physics3D});
        // 로딩 끝나후 사후처리
        aSync.allowSceneActivation = isCompletedAndActvie;

        return aSync;
    }

    public AsyncOperation AsyncUnLoadScene(Define.Scene type)
    {
        AsyncOperation aSync = SceneManager.UnloadSceneAsync(GetSceneName(type));
        aSync.allowSceneActivation = true;
        return aSync;
    }
    #endregion

    #region string으로 맵 로드
    public AsyncOperation AsyncLoadScene(string type, LoadSceneMode mode = LoadSceneMode.Single, bool isCompletedAndActvie = false)
    {
        loadSceneMode = mode;

        // 로딩 바로 시작
        AsyncOperation aSync = SceneManager.LoadSceneAsync(type, mode);
        // 로딩 끝나후 사후처리
        aSync.allowSceneActivation = isCompletedAndActvie;
        return aSync;
    }

    public AsyncOperation AsyncUnLoadScene(string type)
    {
        if (SceneManager.GetSceneByName(type) == null)
            return null;

        AsyncOperation aSync = SceneManager.UnloadSceneAsync(type);
        aSync.allowSceneActivation = true;
        // SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
        return aSync;
    }
    #endregion

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void LeaveRoom()
    {
        LoadScene(Define.Scene.Lobby);
    }

    public void LeaveLobby()
    {
        if (Backend.BMember != null && Backend.UserNickName != "")
            Backend.BMember.Logout();

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            LoadScene(Define.Scene.Login);
        }
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
