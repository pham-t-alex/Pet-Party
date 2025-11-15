using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class TaskManager : NetworkBehaviour
{
    public static TaskManager Instance { get; private set; }

    // ---- Server & client side variables ----
    [SerializeField] private int maxTasks = 3;

    // ---- Server-side variables ----
    private Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();
    private Dictionary<ulong, List<PetTask>> playerTasks = new Dictionary<ulong, List<PetTask>>();
    private Dictionary<ulong, int> playerTaskIds = new Dictionary<ulong, int>();
    [SerializeField] private List<TaskData> tasks = new List<TaskData>();
    [SerializeField] private List<Territory> territories = new List<Territory>();
    private HashSet<PetTask> completedTasks = new HashSet<PetTask>();
    private Dictionary<ulong, int> serverSidePointCounter = new Dictionary<ulong, int>();

    // ---- Client-side variables ----
    [SerializeField] private GameObject taskDisplayPrefab;
    [SerializeField] private GameObject taskDisplayParent;
    private List<TaskDisplayBox> taskDisplayBoxes = new List<TaskDisplayBox>();
    [SerializeField] private float taskDisplayBoxOffset = 120;
    [SerializeField] private GameObject pointDisplayPrefab;
    [SerializeField] private GameObject pointDisplayParent;
    [SerializeField] private float pointDisplayOffset = 50;
    private List<PointDisplay> pointDisplays = new List<PointDisplay>();
    private Dictionary<ulong, int> clientSidePointCounter = new Dictionary<ulong, int>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                playerPoints[client] = 0;
                playerTaskIds[client] = 0;
                playerTasks[client] = new List<PetTask>();
                serverSidePointCounter[client] = 0;

                for (int i = 0; i < maxTasks; i++)
                {
                    AddTask(client);
                }
            }
        }
        if (IsClient)
        {
            foreach (ulong client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                clientSidePointCounter[client] = 0;
                PointDisplay pointDisplay = Instantiate(pointDisplayPrefab, pointDisplayParent.transform).GetComponent<PointDisplay>();
                pointDisplays.Add(pointDisplay);
                pointDisplay.InitializePointDisplay(client);
            }
            ArrangePointDisplays();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        foreach (ulong client in playerTasks.Keys)
        {
            foreach (PetTask task in playerTasks[client])
            {
                task.Tick(Time.deltaTime);
            }
        }

        foreach (PetTask task in completedTasks)
        {
            CompleteTask(task);
        }
        completedTasks.Clear();
    }

    // ----- Server-side functions -----
    void AddTask(ulong player)
    {
        if (!IsServer) return;
        PetTask task = tasks[Random.Range(0, tasks.Count)].CreateTask(player, playerTaskIds[player]);
        playerTaskIds[player]++;
        playerTasks[player].Add(task);
        task.TaskCompletedEvent += () => TaskCompleteUpdate(task);
        task.TaskValueUpdatedEvent += (prev, current) => UpdateTaskValueRpc(task.TaskId, current, RpcTarget.Single(player, RpcTargetUse.Temp));
        AddTaskClientRpc(task.TaskId, task.TaskName, task.CompletionValue, task.MaxCompletionValue, task.PointValue, RpcTarget.Single(player, RpcTargetUse.Temp));
    }

    void CompleteTask(PetTask task)
    {
        if (!IsServer) return;
        playerTasks[task.Player].Remove(task);
        RemoveTaskClientRpc(task.TaskId, RpcTarget.Single(task.Player, RpcTargetUse.Temp));
        playerPoints[task.Player] += task.PointValue;
        serverSidePointCounter[task.Player]++;
        UpdatePointsRpc(task.Player, playerPoints[task.Player], serverSidePointCounter[task.Player]);
        AddTask(task.Player);
    }

    // defer removing and adding tasks until after tasks are done ticking
    void TaskCompleteUpdate(PetTask task)
    {
        if (!IsServer) return;
        completedTasks.Add(task);
    }

    public Territory FindTerritoryById(int id)
    {
        foreach (Territory territory in territories)
        {
            if (territory.Id == id)
            {
                return territory;
            }
        }
        return null;
    }

    // ----- Client-side functions -----
    [Rpc(SendTo.SpecifiedInParams)]
    void AddTaskClientRpc(int taskId, string taskName, float value, float maxValue, int points, RpcParams rpcParams)
    {
        TaskDisplayBox taskBox = Instantiate(taskDisplayPrefab, taskDisplayParent.transform).GetComponent<TaskDisplayBox>();
        taskBox.InitializeTaskInfo(taskId, taskName, value, maxValue, points);
        taskDisplayBoxes.Add(taskBox);
        UpdateTasksDisplayed();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void RemoveTaskClientRpc(int taskId, RpcParams rpcParams)
    {
        int index = 0;
        while (index < taskDisplayBoxes.Count)
        {
            if (taskDisplayBoxes[index].TaskId == taskId)
            {
                TaskDisplayBox box = taskDisplayBoxes[index];
                taskDisplayBoxes.RemoveAt(index);
                Destroy(box.gameObject);
                break;
            }
            index++;
        }
        UpdateTasksDisplayed();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void UpdateTaskValueRpc(int taskId, float value, RpcParams rpcParams)
    {
        int index = 0;
        while (index < taskDisplayBoxes.Count)
        {
            if (taskDisplayBoxes[index].TaskId == taskId)
            {
                taskDisplayBoxes[index].SetTaskValue(value);
                return;
            }
            index++;
        }
    }

    void UpdateTasksDisplayed()
    {
        for (int i = 0; i < taskDisplayBoxes.Count; i++)
        {
            if (i >= maxTasks)
            {
                taskDisplayBoxes[i].gameObject.SetActive(false);
                continue;
            }
            taskDisplayBoxes[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, - i * taskDisplayBoxOffset);
            taskDisplayBoxes[i].gameObject.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdatePointsRpc(ulong player, int points, int counter)
    {
        if (counter <= clientSidePointCounter[player]) return;
        clientSidePointCounter[player] = counter;
        int index = 0;
        while (index < pointDisplays.Count)
        {
            if (pointDisplays[index].PlayerId == player)
            {
                pointDisplays[index].SetPoints(points);
                break;
            }
            index++;
        }
        ArrangePointDisplays();
    }

    void ArrangePointDisplays()
    {
        pointDisplays.Sort(ComparePointDisplays);
        for (int i = 0; i < pointDisplays.Count; i++)
        {
            pointDisplays[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * pointDisplayOffset);
        }
    }

    int ComparePointDisplays(PointDisplay one, PointDisplay two)
    {
        // reverse order to sort by descending point value
        int compareVal = two.Points.CompareTo(one.Points);
        if (compareVal == 0) return one.PlayerId.CompareTo(two.PlayerId);
        return compareVal;
    }
}
