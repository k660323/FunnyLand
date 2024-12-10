using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    Text text;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void LateUpdate()
    {
        //text.text = string.Format("{0:F0}", GameManager.Instance.score);
    }
}
