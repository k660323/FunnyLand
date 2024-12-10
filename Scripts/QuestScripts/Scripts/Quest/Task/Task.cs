using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskState
{
    Inactive,
    Running,
    Complete,
}

[CreateAssetMenu(menuName = "Quest/Task/Task",fileName = "Task_")]
public class Task : ScriptableObject
{
    #region Events
    public delegate void StateChangedHandler(Task task, TaskState currentState, TaskState prevState);
    public delegate void SuccessChangedHandler(Task task, int currentSuccess, int prevSuccess);
    #endregion

    [SerializeField]
    private Category category;

    [Header("Text")]
    [SerializeField]
    private string codeName; // 내부적으로 사용되는 이름 // 데이터를 비교할때 쓰임
    [SerializeField]
    private string description;

    [Header("Action")]
    [SerializeField]
    private TaskAction action;

    [Header("Target")]
    [SerializeField]
    private TaskTarget[] targets;

    [Header("Setting")]
    [SerializeField]
    private InitialSuccessValue initialSuccessValue;
    [SerializeField]
    private int needSuccessToComplete;
    [SerializeField]
    private bool canReceiveReportsDuringCompletion; // 퀘스트 성공 했는데 계속 보고 받을 것 인가

    private TaskState state;
    private int currentSuccess;

    public event StateChangedHandler onStateChanged;
    public event SuccessChangedHandler onSuccessChanged;

    public int CurrentSuccess 
    { 
        get => currentSuccess;
        set
        {
            int prevSuccess = currentSuccess;

            currentSuccess = canReceiveReportsDuringCompletion ? Mathf.Clamp(value, 0, int.MaxValue) : Mathf.Clamp(value, 0, needSuccessToComplete);
           
            if(currentSuccess != prevSuccess)
            {
                if(canReceiveReportsDuringCompletion)
                    State = (currentSuccess >= needSuccessToComplete) ? TaskState.Complete : TaskState.Running;
                else
                    State = (currentSuccess == needSuccessToComplete) ? TaskState.Complete : TaskState.Running;
                
                onSuccessChanged?.Invoke(this, CurrentSuccess, prevSuccess);
            }
        } 
    }
    public Category Category => category;
    public string CodeName => codeName;
    public string Description => description;
    public int NeedSuccessToComplete => needSuccessToComplete;
    public bool CanReceiveReportsDuringCompletion => canReceiveReportsDuringCompletion;

    public TaskState State
    {
        get => state;
        set
        {
            var prevState = state;
            state = value;
            onStateChanged?.Invoke(this, state, prevState);
        }
    }

    public bool IsComplete => State == TaskState.Complete;
    public  Quest Owner { get; private set; }

    public void Setup(Quest owner)
    {
        Owner = owner;
    }

    public void Start()
    {
        State = TaskState.Running;
        if (initialSuccessValue)
            CurrentSuccess = initialSuccessValue.GetValue(this);
    }

    public void End()
    {
        onStateChanged = null;
        onSuccessChanged = null;
    }

    public void ReceiveReport(int successCount)
    {
        CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
    }

    public void Complete()
    {
        if (canReceiveReportsDuringCompletion)
            currentSuccess = currentSuccess > needSuccessToComplete ? currentSuccess : needSuccessToComplete;
        else
            currentSuccess = needSuccessToComplete;
    }

    public bool IsTarget(string category, object target)
        => Category == category &&
        targets.Any(x => x.IsEqual(target)) &&
        (!IsComplete || (IsComplete && canReceiveReportsDuringCompletion));

    public bool ContainsTarget(object target) => targets.Any(x => x.IsEqual(target));
}
