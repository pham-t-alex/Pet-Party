using Unity.Netcode;
using UnityEngine;

public class InteractObject2 : NetworkBehaviour, IInteract
{
    [SerializeField] GameObject prefab;
    public void Start()
    {
        prefab = Resources.Load<GameObject>("Prefabs/InteractionObject2");
    }
        
    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
    public void Interact(GameObject interactor)
    {
        if (!IsServer) return; 

        Debug.Log($"{gameObject.name} has been interacted!");
        float x = Random.Range(-10f, 10f);
        float y = Random.Range(-10f, 10f);

        GameObject go = Instantiate(prefab);
        go.transform.position = new Vector3(x, y, 0);

        //The color only changes with the host, the client doesn't see any randomness with colors
        //  Doesn't really do anything bad now but if we need to change values of this in the future it
        //  it should be looked at
        go.GetComponent<SpriteRenderer>().material.color = Random.ColorHSV();
        go.GetComponent<NetworkObject>().Spawn();

        //Dynamically adds the object to the list
        PlayerInteract PI = interactor.GetComponent<PlayerInteract>();
        PI.Interacting.Value = false;
        PI.AddInteractables(go.GetComponent<NetworkObject>());
    }

}
