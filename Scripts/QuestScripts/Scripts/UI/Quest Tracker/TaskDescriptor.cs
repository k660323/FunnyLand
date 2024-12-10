using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskDescriptor : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Color normalColor;
    [SerializeField]
    private Color taskCompletionColor;
    [SerializeField]
    private Color taskSuccessCountColor;
    [SerializeField]
    private Color strikeThroughColor;

    public void UpdateText(string text)
    {
        this.text.text = text;
    }

    public void UpdateText(Task task)
    {
        if (task.IsComplete)
        {
            var colorCode = ColorUtility.ToHtmlStringRGB(taskCompletionColor);
            text.text = BuildText(task, colorCode, colorCode);
        }
        else
            text.text = BuildText(task, ColorUtility.ToHtmlStringRGB(normalColor), ColorUtility.ToHtmlStringRGB(taskSuccessCountColor));
    }

    public void UpdateTextUsingStrikeThrough(Task task)
    {
        var colorCode = ColorUtility.ToHtmlStringRGB(strikeThroughColor);
        //text.fontStyle = FontStyles.Strikethrough;
        text.text = BuildText(task, colorCode, colorCode);
    }

    private string BuildText(Task task, string textColorCode, string successCountColorCode)
    {
        return $"<color=#{textColorCode}>¡Ü {task.Description} <color=#{successCountColorCode}>{task.CurrentSuccess}</color>/{task.NeedSuccessToComplete}</color>";
    }
}
