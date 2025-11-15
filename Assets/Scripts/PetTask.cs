using System;
using UnityEngine;

public abstract class PetTask
{
    public ulong Player { get; private set; }
    public int TaskId { get; private set; }
    public string TaskName { get; private set; }
    public int PointValue { get; private set; }
    public float MaxCompletionValue { get; private set; }

    public event Action<float, float> TaskValueUpdatedEvent;
    public event Action TaskCompletedEvent;
    public float CompletionValue { get; private set; }

    public PetTask(ulong player, int taskId, string taskName, int pointValue, float maxCompletionValue, float startingCompletionValue)
    {
        Player = player;
        TaskId = taskId;
        TaskName = taskName;
        PointValue = pointValue;
        MaxCompletionValue = maxCompletionValue;
        CompletionValue = startingCompletionValue;
    }

    protected void UpdateTaskCompletionValue(float newCompletionValue)
    {
        float prevValue = CompletionValue;
        CompletionValue = Mathf.Clamp(newCompletionValue, Mathf.NegativeInfinity, MaxCompletionValue);
        TaskValueUpdatedEvent?.Invoke(prevValue, newCompletionValue);
        if (CompletionValue >= MaxCompletionValue)
        {
            TaskCompletedEvent?.Invoke();
            CompleteTaskCleanup();
        }
    }

    public virtual void Tick(float deltaTime)
    {

    }

    protected virtual void CompleteTaskCleanup()
    {
        TaskValueUpdatedEvent = null;
        TaskCompletedEvent = null;
    }
}
