using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;
    [Tooltip("radius of the hit area around the attack point")]
    [SerializeField] private float attackSize = 1f;
    [Tooltip("in secs")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackKnockbackForce = 1f;
    [Tooltip("how long the knockback is being applied for")]
    [SerializeField] private float attackKnockbackTime = 1f;

    private Rigidbody2D rb;
    private Camera cam;

    private Vector2 movementInput;
    private Vector2 knockbackImpulse;
    private float remainingKnockbackTime = 0f;
    private float nextAttackTime;

    [SerializeField] private PlayerInput playerInput;
    private PlayerInteract playerInteraction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInteraction = this.GetComponent<PlayerInteract>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            playerInteraction.UpdatePromptDisplay(playerInput.actions["Interact"].bindings[0].ToDisplayString());
        }
        
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        remainingKnockbackTime = Mathf.Max(0f, remainingKnockbackTime - Time.fixedDeltaTime);
        Vector2 move = Vector2.ClampMagnitude(movementInput, 1f) * moveSpeed;
        float k = (attackKnockbackTime > 0f)
            ? (remainingKnockbackTime / attackKnockbackTime)
            : 0f;
        Vector2 kb = knockbackImpulse * k;
        rb.linearVelocity = kb + move;
    }

    void OnDrawGizmos()
    {
        if (!IsOwner) return;

        Gizmos.color = Color.green;

        Vector2 direction = ((Vector2) cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - (Vector2) transform.position).normalized;
        Gizmos.DrawWireSphere((Vector2) transform.position + direction * attackRange, attackSize);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        MoveRpc(ctx.ReadValue<Vector2>());
    }

    public void OnInteract(InputAction.CallbackContext ctx) 
    {
        if(!IsOwner || !ctx.performed) return;

        Debug.Log($"Hi hi I'm interacting key: {playerInput.actions["Interact"].bindings[0].ToDisplayString()}");
        playerInteraction.TryFindClosestInteractableServerRpc();
    }

    [Rpc(SendTo.Server)]
    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (ctx.performed)
        {
            Vector2 direction = ((Vector2)cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - (Vector2)transform.position).normalized;
            AttackRpc(direction);
        }
    }

    public void OnHit()
    {
        Debug.Log("Player has been hit");
    }

    [Rpc(SendTo.Server)]
    public void MoveRpc(Vector2 movement)
    {
        movementInput = movement;
    }

    [Rpc(SendTo.Server)]
    public void AttackRpc(Vector2 direction)
    {
        if (Time.time > nextAttackTime) {
            nextAttackTime = Time.time + attackCooldown;

            RaycastHit2D hit = Physics2D.CircleCast((Vector2) transform.position + direction * attackRange, attackSize, Vector2.up);
            if (hit.collider != null)
            {
                Vector2 knockbackDirection = ((Vector2)hit.collider.transform.position - (Vector2)transform.position).normalized;
                var targetPlayer = hit.collider.GetComponentInParent<Player>();
                if (targetPlayer != null)
                {
                    targetPlayer.ApplyKb(knockbackDirection, attackKnockbackForce);
                    targetPlayer.OnHit();
                }
            }
        }
    }

    public void ApplyKb(Vector2 direction, float knockbackAmount)
    {
        Vector2 impulse = direction.normalized * knockbackAmount;
        knockbackImpulse = impulse;
        remainingKnockbackTime = attackKnockbackTime;
    }

}