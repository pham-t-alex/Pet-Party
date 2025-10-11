using Unity.Netcode;
using UnityEngine;

public class InteractObject2 : NetworkBehaviour, IInteract
{
    [SerializeField] GameObject prefab;
    public void Interact(GameObject interactor)
    {
        Debug.Log($"{gameObject.name} has been interacted!");
        float x = Random.Range(-10f, 10f);
        float y = Random.Range(-10f, 10f);

        GameObject go = Instantiate(prefab);
        go.transform.position = new Vector3(x, y, 0);

        go.GetComponent<SpriteRenderer>().material.color = Random.ColorHSV();
        go.GetComponent<NetworkObject>().Spawn();
    }
}
