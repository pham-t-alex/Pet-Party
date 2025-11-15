using UnityEngine;

[CreateAssetMenu(fileName = "TerritoryControlTaskData", menuName = "TaskData/TerritoryControlTaskData")]
public class TerritoryControlTaskData : TaskData
{
    [Header("Territory Control Task Info")]
    [field: SerializeField, Tooltip("Territory Id")]
    public int TerritoryId { get; private set; }

    public override PetTask CreateTask(ulong player, int taskId)
    {
        TerritoryControlTask task = new TerritoryControlTask(player, taskId, TaskName, PointValue, MaxCompletionValue, StartCompletionValue, TerritoryId);
        return task;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
