using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviourPunCallbacks
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    public abstract void Init();
    private void Awake()
    {
        Init();
    }

    public virtual void LateInit() { }
    private void Start()
    {
        LateInit();
    }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    public T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected Text GetText(int idx) { return Get<Text>(idx); }

    protected Button GetButton(int idx) { return Get<Button>(idx); }

    protected Image GetImage(int idx) { return Get<Image>(idx); }

    // �ܺ� ����
    public void CoverBindEvent<T>(int btn, UnityAction<PointerEventData> action) where T : Component
    {
        Get<T>(btn).gameObject.CoverBindEvent(action);
    }

    public static void AddUIEvent(GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Enter:
                evt.OnEnterHandler -= action;
                evt.OnEnterHandler += action;
                break;
            case Define.UIEvent.Down:
                evt.OnDownHandler -= action;
                evt.OnDownHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
            case Define.UIEvent.Up:
                evt.OnUpHandler -= action;
                evt.OnUpHandler += action;
                break;
            case Define.UIEvent.Exit:
                evt.OnExitHandler -= action;
                evt.OnExitHandler += action;
                break;
        }
        //evt.OnDragHandler += ((PointerEventData data) => evt.gameObject.transform.position = data.position);
    }

    public static void RemoveUIEvent(GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= action;
                break;
            case Define.UIEvent.Up:
                evt.OnUpHandler -= action;
                break;
            case Define.UIEvent.Enter:
                evt.OnEnterHandler -= action;
                break;
            case Define.UIEvent.Exit:
                evt.OnExitHandler -= action;
                break;
        }
    }
    
    public static void CoverUIEvent(GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler = action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler = action;
                break;
            case Define.UIEvent.Up:
                evt.OnUpHandler = action;
                break;
            case Define.UIEvent.Enter:
                evt.OnEnterHandler = action;
                break;
            case Define.UIEvent.Exit:
                evt.OnExitHandler = action;
                break;
        }
    }

    public static void RemoveAllUIEvent(GameObject go, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_EventHandler evt = go.GetComponent<UI_EventHandler>();

        if (evt == null)
            return;

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler = null;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler = null;
                break;
            case Define.UIEvent.Up:
                evt.OnUpHandler = null;
                break;
            case Define.UIEvent.Enter:
                evt.OnEnterHandler = null;
                break;
            case Define.UIEvent.Exit:
                evt.OnExitHandler = null;
                break;
        }
    }

}
