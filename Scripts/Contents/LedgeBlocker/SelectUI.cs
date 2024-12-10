using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectUI : MonoBehaviour
{
    Button button;
    public KeyCode mappingKey;
    void Start()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (!Managers.Game.isGameStart)
            return;

        if(button.interactable && Input.GetKeyDown(mappingKey))
        {
            button.onClick?.Invoke();
        }
    }
}
