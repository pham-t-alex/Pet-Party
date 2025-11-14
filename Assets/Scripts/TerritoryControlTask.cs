using UnityEngine;

public class TerritoryControlTask : PetTask
{
    private Territory territory;
    private bool controlling = false;

    public TerritoryControlTask(ulong player, string taskName, int pointValue, float maxCompletionValue, float startingCompletionValue, int id) : base(player, taskName, pointValue, maxCompletionValue, startingCompletionValue)
    {
        territory = TaskManager.Instance.FindTerritoryById(id);
        if (territory != null)
        {
            territory.TerritoryPlayerUpdatedEvent += HandleTerritoryPlayerUpdate;
            HandleTerritoryPlayerUpdate();
        }
    }

    void HandleTerritoryPlayerUpdate()
    {
        if (territory.PlayersInTerritory.Count == 1 && territory.PlayersInTerritory.Contains(Player))
        {
            controlling = true;
        }
        else
        {
            controlling = false;
        }
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (controlling)
        {
            UpdateTaskCompletionValue(CompletionValue + Time.deltaTime);
            Debug.Log($"Controlling: {CompletionValue}");
        }
    }
}
