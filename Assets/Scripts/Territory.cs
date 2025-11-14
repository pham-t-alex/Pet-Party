using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Territory : MonoBehaviour
{
    [field: SerializeField] public int Id { get; private set; }
    public HashSet<ulong> PlayersInTerritory { get; private set; } = new HashSet<ulong>();
    public event Action TerritoryPlayerUpdatedEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (collision.gameObject.GetComponent<PlayerNetworkBehaviour>() != null)
        {
            PlayersInTerritory.Add(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
        TerritoryPlayerUpdatedEvent?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (collision.gameObject.GetComponent<PlayerNetworkBehaviour>() != null)
        {
            PlayersInTerritory.Remove(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId);
        }
        TerritoryPlayerUpdatedEvent?.Invoke();
    }
}
