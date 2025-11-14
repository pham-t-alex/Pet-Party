using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class TaskManager : NetworkBehaviour
{
    public static TaskManager Instance { get; private set; }

    // Server-side stuff
    private Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();
    private Dictionary<ulong, List<PetTask>> playerTasks = new Dictionary<ulong, List<PetTask>>();
    [SerializeField] private List<TaskData> tasks = new List<TaskData>();
    [SerializeField] private List<Territory> territories = new List<Territory>();
    private HashSet<PetTask> completedTasks = new HashSet<PetTask>();

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
                playerTasks[client] = new List<PetTask>();

                for (int i = 0; i < 3; i++)
                {
                    AddTask(client);
                }
            }
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
        PetTask task = tasks[Random.Range(0, tasks.Count)].CreateTask(player);
        playerTasks[player].Add(task);
        task.TaskCompletedEvent += () => TaskCompleteUpdate(task);
    }

    void CompleteTask(PetTask task)
    {
        if (!IsServer) return;
        playerTasks[task.Player].Remove(task);
        playerPoints[task.Player] += task.PointValue;
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
}
