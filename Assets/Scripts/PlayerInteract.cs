using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour
{
    [SerializeField] private KeyCode interactkey = KeyCode.E;
    [SerializeField] private float interactRange = 10f;

    [SerializeField] private bool debug = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(interactkey))
        {
            if (debug)
            {
                Debug.Log($"{gameObject.name} pressed {interactkey}");

            }
            TryInteractServerRpc();
        }
    }
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    [Rpc(SendTo.Server)]
    void TryInteractServerRpc()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<IInteract>(out var interactable))
            {
                Debug.Log($"Caught {this.name}");
                interactable.Interact(this.gameObject);
            }
        }
    }
}
