using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestTracker : MonoBehaviour
{
    [SerializeField]
    private Text questTitleText;
    [SerializeField]
    private TaskDescriptor taskDescriptorPrefab;

    private Dictionary<Task, TaskDescriptor> taskDescriptorsByTask = new Dictionary<Task, TaskDescriptor>();
    
    private Quest targetQuest;

    private void OnDestroy()
    {
        if(targetQuest!=null)
        {
            targetQuest.onNewTaskGroup -= UpdateTaskDescriptos;
            targetQuest.onCompleted -= DestroySelf;
            targetQuest.onCanceled -= DestroySelf;
        }

        foreach(var tuple in taskDescriptorsByTask)
        {
            var task = tuple.Key;
            task.onSuccessChanged -= UpdateText;
        }
    }

    public void Setup(Quest targetQuest, Color titleColor)
    {
        this.targetQuest = targetQuest;

        questTitleText.text = targetQuest.Category == null ?
            targetQuest.DisplayName :
            $"[{targetQuest.Category.DisplayName}] {targetQuest.DisplayName}";

        questTitleText.color = titleColor;

        targetQuest.onNewTaskGroup += UpdateTaskDescriptos;
        targetQuest.onCompleted += DestroySelf;
        targetQuest.onCanceled += DestroySelf;
        var taskGroups = targetQuest.TaskGroups;
        UpdateTaskDescriptos(targetQuest, taskGroups[0]);

        if (taskGroups[0] != targetQuest.CurrentTaskGroup)
        {
            for (int i = 1; i < taskGroups.Count; i++)
            {
                var taskGroup = taskGroups[i];
                UpdateTaskDescriptos(targetQuest, taskGroup, taskGroups[i - 1]);

                if (taskGroup == targetQuest.CurrentTaskGroup)
                    break;
            }
        }
    }

    private void UpdateTaskDescriptos(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup = null)
    {
        foreach (var task in currentTaskGroup.Tasks)
        {
            var taskDescriptor = Instantiate(taskDescriptorPrefab, transform);
            taskDescriptor.UpdateText(task);
            task.onSuccessChanged += UpdateText;
            taskDescriptorsByTask.Add(task, taskDescriptor);
        }

        if (prevTaskGroup != null)
        {
            foreach (var task in prevTaskGroup.Tasks)
            {
                var taskDescriptor = taskDescriptorsByTask[task];
                taskDescriptor.UpdateTextUsingStrikeThrough(task);
            }
        }
    }

    private void UpdateText(Task task,int currentSuccess,int prevSuccess)
    {
        taskDescriptorsByTask[task].UpdateText(task);
    }

    private void DestroySelf(Quest quest)
    {
        Destroy(gameObject);
    }

}
