using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerNetworkBehaviour : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    private Vector2 movementInput;

    [Header("UI")]
    [SerializeField] private EnergyBarUI energyBarUI;
    [SerializeField] private AffectionBarUI affectionBarUI;

    [Header("Stats")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float maxAffection = 100f;

    private NetworkVariable<float> energy =
        new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> affection =
        new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);

    private bool _energySubscribed, _affSubscribed;

    public override void OnNetworkSpawn()
{
    if (IsServer)
    {
        energy.Value = maxEnergy;
        affection.Value = maxAffection;
    }

    if (IsOwner)
    {
        if (energyBarUI == null)
            energyBarUI = FindObjectOfType<EnergyBarUI>(includeInactive: true);
        if (affectionBarUI == null)
            affectionBarUI = FindObjectOfType<AffectionBarUI>(includeInactive: true);

        if (energyBarUI != null)
        {
            energyBarUI.SetMaxEnergy(maxEnergy);
            energy.OnValueChanged += OnEnergyChanged;
            _energySubscribed = true;
            OnEnergyChanged(0f, energy.Value);
        }

        if (affectionBarUI != null)
        {
            affectionBarUI.SetMaxAffection(maxAffection);
            affection.OnValueChanged += OnAffectionChanged;
            _affSubscribed = true;
            OnAffectionChanged(0f, affection.Value);
        }
    }
}

    private void OnDestroy()
    {
        if (_energySubscribed) energy.OnValueChanged -= OnEnergyChanged;
        if (_affSubscribed)    affection.OnValueChanged -= OnAffectionChanged;
    }

    private void OnEnergyChanged(float _, float curr)
    {
        energyBarUI?.SetEnergy(curr);
    }

    private void OnAffectionChanged(float _, float curr)
    {
        affectionBarUI?.SetAffection(curr);
    }

    private void Update()
    {
        HandleDebugHotkeys();

        if (!IsServer) return;
        transform.Translate(movementInput * moveSpeed * Time.deltaTime);
    }


    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        MoveRpc(ctx.ReadValue<Vector2>());
    }

    private void HandleDebugHotkeys()
    {
        if (!IsClient || !IsOwner) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        // 1 = -5 affection, 2 = -5 energy
        if (kb.digit1Key.wasPressedThisFrame) AddAffectionRpc(-5f);
        if (kb.digit2Key.wasPressedThisFrame) SpendEnergyRpc(5f);
    }

    [Rpc(SendTo.Server)]
    private void MoveRpc(Vector2 movement)
    {
        movementInput = movement;
    }

    [Rpc(SendTo.Server)]
    public void SpendEnergyRpc(float amount)
        => energy.Value = Mathf.Max(0f, energy.Value - Mathf.Abs(amount));

    [Rpc(SendTo.Server)]
    public void AddAffectionRpc(float amount)
        => affection.Value = Mathf.Clamp(affection.Value + amount, 0f, maxAffection);
}
