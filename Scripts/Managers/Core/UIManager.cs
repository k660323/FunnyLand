using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager
{
    UI_Scene sceneUI;
    UI_ContentsScene contentsSceneUI;

    public UI_Scene SceneUI
    {
        get
        {
            if(sceneUI == null)
            {
                sceneUI = GameObject.FindObjectOfType<UI_Scene>();
            }

            return sceneUI;
        }
        set
        {
            sceneUI = value;
        }
    }

    public UI_ContentsScene ContentsSceneUI
    {
        get
        {
            if (contentsSceneUI == null)
            {
                contentsSceneUI = GameObject.FindObjectOfType<UI_ContentsScene>();
            }

            return contentsSceneUI;
        }
        set
        {
            contentsSceneUI = value;
        }
    }

    int _order = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    Stack<UI_Popup> _comPopupStack = new Stack<UI_Popup>();

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject() { name = "@UI_Root" };
            return root;
        }
    }

    public GameObject ContentsRoot
    {
        get
        {
            GameObject root = GameObject.Find("@UI_ContentsRoot");
            if (root == null)
                root = new GameObject() { name = "@UI_ContentsRoot" };
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        CanvasScaler scaler = Util.GetOrAddComponent<CanvasScaler>(go);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Util.GetOrAddComponent<GraphicRaycaster>(go);

        if (sort)
        {
            canvas.sortingOrder = _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    public void SetCanvas(GameObject go,int sortIndex)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortIndex;

        CanvasScaler scaler = Util.GetOrAddComponent<CanvasScaler>(go);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Util.GetOrAddComponent<GraphicRaycaster>(go);
    }

    public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/WorldSpace/{name}");

        if (parent != null)
            go.transform.SetParent(parent);

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return Util.GetOrAddComponent<T>(go);
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");

        if (parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        T component = Managers.FindObjectOfType<T>();
        if (component != null)
        {
            this.sceneUI = component;
            component.transform.SetParent(Root.transform);
            return component;
        }

        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        this.sceneUI = sceneUI;

        go.transform.SetParent(Root.transform);
        return sceneUI;
    }

    public T ShowCSceneUI<T>(string name = null) where T : UI_ContentsScene
    {
        T component = Managers.FindObjectOfType<T>();
        if (component != null)
        {
            this.contentsSceneUI = component;
            component.transform.SetParent(ContentsRoot.transform);
            return component;
        }

        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/Contents/{name}");
        T sceneUI = Util.GetOrAddComponent<T>(go);
        this.contentsSceneUI = sceneUI;

        go.transform.SetParent(ContentsRoot.transform);
        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null, bool isCommandPopup = false) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go;

        go = Managers.Resource.Instantiate($"UI/Popup/{name}");

        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        if (isCommandPopup)
            _comPopupStack.Push(popup);

        go.transform.SetParent(Root.transform);

        if (Managers.GameCursor.defaultCursorLock)
            Managers.GameCursor.CursorLock = false;

        Managers.Input.IsCameraMove = false;
        return popup;
    }

    public T CloseNewShowPopUp<T>(bool isAll, string prefabName, string content = "") where T : UI_Popup
    {
        if (isAll)
            CloseAllPopupUI();
        else
            ClosePopupUI();
            
        var component = Managers.UI.ShowPopupUI<T>(prefabName);
        var notice = component as UI_Notice;
        if (notice)
            notice.SetContent(content);

        return component;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
            return;

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();

        if (popup != null && popup.gameObject != null)
        {
            if (_comPopupStack.Count != 0 && popup == _comPopupStack.Peek())
                _comPopupStack.Pop();
            Managers.Resource.Destroy(popup.gameObject);
        }
        popup = null;

        _order--;

        if (_popupStack.Count == 0)
        {
            if (Managers.GameCursor.defaultCursorLock)
                Managers.GameCursor.CursorLock = true;
            Managers.Input.IsCameraMove = true;
        }
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void CommandSetting()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_comPopupStack.Count == 0)
            {
                Managers.UI.ShowPopupUI<UI_Setting>("Setting", true);
            }
            else
            {
                Managers.UI.ClosePopupUI(_comPopupStack.Peek());
            }
        }
    }

    public int PopupCount()
    {
        return _popupStack.Count;
    }

    public int ComPopupCount()
    {
        return _comPopupStack.Count;
    }

    public void Clear()
    {
        CloseAllPopupUI();
        if (Managers.Scene.loadSceneMode == UnityEngine.SceneManagement.LoadSceneMode.Single)
            sceneUI = null;
        contentsSceneUI = null;
    }
}
