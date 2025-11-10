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

    }

    // ----- Server-side functions -----
    void AddTask(ulong player)
    {
        if (!IsServer) return;
        PetTask task = tasks[Random.Range(0, tasks.Count)].CreateTask();
        playerTasks[player].Add(task);
        task.TaskCompletedEvent += () => CompleteTask(player, task);
    }

    void CompleteTask(ulong player, PetTask task)
    {
        if (!IsServer) return;
        playerTasks[player].Remove(task);
        playerPoints[player] += task.PointValue;
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
