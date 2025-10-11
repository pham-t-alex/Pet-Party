using Unity.Netcode;
using UnityEngine;

public class InteractObject : NetworkBehaviour , IInteract
{
    public string Name { get; private set; }

    public void Interact(GameObject interactor) 
    {
        Debug.Log($"{gameObject.name} has been interacted!");

        Destroy(this.gameObject);
    
    }
}
