using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCursorManager
{
    public bool defaultCursorLock = false;

    public bool CursorLock
    {
        set
        {
            Cursor.visible = !value;
            Cursor.lockState = !value ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void Init()
    {
        SettingGameCursor(false);
    }

    public void SettingGameCursor(bool defaultState)
    {
        defaultCursorLock = defaultState;
        CursorLock = defaultState;
    }

    public void Clear()
    {
        SettingGameCursor(false);
    }
}
