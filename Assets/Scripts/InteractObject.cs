using Unity.Netcode;
using UnityEngine;

public class InteractObject : NetworkBehaviour , IInteract
{
    public GameObject GetGameObject() => this.gameObject;

    public void Interact(GameObject interactor) 
    {
        Debug.Log($"{gameObject.name} has been interacted!");

        Destroy(this.gameObject);
    
    }
}
