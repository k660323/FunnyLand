using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    #region 이벤트 추가,삭제,덮어쓰기,모두삭제
    public static void BindEvent(this GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.AddUIEvent(go, action, type);
    }

    public static void RemoveBindEvent(this GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.RemoveUIEvent(go, action, type);
    }

    public static void CoverBindEvent(this GameObject go, UnityAction<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.CoverUIEvent(go, action, type);
    }

    public static void RemoveAllEvent(this GameObject go, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.RemoveAllUIEvent(go, type);
    }

    #endregion

    public static void Resize<T>(this List<T> list, int size)
    {
        int count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            if (size > list.Capacity)
            {
                list.Capacity = size;
            }

            list.AddRange(new T[size - count]);
        }
    }

    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }
}
