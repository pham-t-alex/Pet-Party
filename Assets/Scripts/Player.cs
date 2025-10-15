using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        if (remainingKnockbackTime > 0)remainingKnockbackTime -= Time.fixedDeltaTime;
        rb.linearVelocity = remainingKnockbackTime > 0 ? (moveSpeed * movementInput + knockbackImpulse * (remainingKnockbackTime / attackKnockbackTime)) : moveSpeed * movementInput;
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

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (ctx.performed && Time.time > nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Vector2 direction = ((Vector2) cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - (Vector2) transform.position).normalized;
            RaycastHit2D hit = Physics2D.CircleCast((Vector2) transform.position + direction * attackRange, attackSize, Vector2.up);
            if (hit.collider != null)
            {
                Vector2 knockbackDirection = ((Vector2)hit.collider.transform.position - (Vector2)transform.position).normalized;
                hit.collider.GetComponentInParent<Player>().OnHit(knockbackDirection);
            }
        }
    }

    public void OnHit(Vector2 direction)
    {
        OnHitRpc(direction, attackKnockbackForce);
    }

    [Rpc(SendTo.Server)]
    public void MoveRpc(Vector2 movement)
    {
        movementInput = movement;
    }

    [Rpc(SendTo.Server)]
    public void OnHitRpc(Vector2 direction, float knockbackAmount)
    {
        knockbackImpulse = direction.normalized * knockbackAmount;
        remainingKnockbackTime = attackKnockbackTime;
    }

}