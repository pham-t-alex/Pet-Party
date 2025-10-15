using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerNetworkBehaviour : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    private Vector2 movementInput;

    private PlayerInteract playerInteraction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInteraction = this.GetComponent<PlayerInteract>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInteraction.InteractEvent();

        if (!IsServer) return;
        transform.Translate(movementInput * moveSpeed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        MoveRpc(ctx.ReadValue<Vector2>());
    }

    [Rpc(SendTo.Server)]

    public void MoveRpc(Vector2 movement)
    {
        movementInput = movement;
    }

    
}