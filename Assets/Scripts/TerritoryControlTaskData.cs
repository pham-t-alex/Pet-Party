using UnityEngine;

[CreateAssetMenu(fileName = "TerritoryControlTaskData", menuName = "TaskData/TerritoryControlTaskData")]
public class TerritoryControlTaskData : TaskData
{
    [Header("Territory Control Task Info")]
    [field: SerializeField, Tooltip("Territory Id")]
    public int TerritoryId { get; private set; }

    public override PetTask CreateTask()
    {
        TerritoryControlTask task = new TerritoryControlTask(TaskName, PointValue, MaxCompletionValue, StartCompletionValue);
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
