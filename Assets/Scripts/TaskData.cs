using UnityEngine;

public abstract class TaskData : ScriptableObject
{
    [Header("Task Info")]
    [field: SerializeField, Tooltip("The task name")]
    public string TaskName { get; private set; }
    [field: SerializeField, Tooltip("Task point value")]
    public int PointValue { get; private set; }
    [field: SerializeField, Tooltip("Task max completion value")]
    public float MaxCompletionValue { get; private set; }
    [field: SerializeField, Tooltip("Task starting value")]
    public float StartCompletionValue { get; private set; }

    public abstract PetTask CreateTask(ulong player, int taskId);
}
