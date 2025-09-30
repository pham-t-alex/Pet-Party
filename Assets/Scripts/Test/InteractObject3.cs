using Unity.Netcode;
using UnityEngine;

public class InteractObject3 : NetworkBehaviour, IInteract
{ 
    public void Interact(GameObject interactor)
    {
        Debug.Log($"{gameObject.name} has been interacted!");
        GameObject.Find("Wall").GetComponent<NetworkObject>().Despawn(true);

    }
}
