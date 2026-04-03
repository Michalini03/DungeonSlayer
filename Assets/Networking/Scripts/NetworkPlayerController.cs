using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network wrapper for the player. Ensures only the owning client
/// can control this player, and syncs position/health to all clients.
/// Attach to the Player prefab alongside PlayerMovement, PlayerCombat, etc.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(AttributesController))]
public class NetworkPlayerController : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private AttributesController attributesController;
    private Rigidbody2D rb;

    // Synced health so all clients see correct HP
    private NetworkVariable<int> syncedHealth = new NetworkVariable<int>(
        120,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<Vector2> syncedPosition = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<Vector2> syncedVelocity = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<bool> syncedFacingRight = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [Header("Network Interpolation")]
    [SerializeField] private float interpolationSpeed = 15f;

    public override void OnNetworkSpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        attributesController = GetComponent<AttributesController>();
        rb = GetComponent<Rigidbody2D>();

        PlayerRegistry.Register(gameObject);

        if (!IsOwner)
        {
            // Disable input-driven components for non-owners
            playerMovement.enabled = false;

            // Make the rigidbody kinematic for non-owners (position is synced)
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        syncedHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        PlayerRegistry.Unregister(gameObject);
        syncedHealth.OnValueChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (!IsSpawned) return;

        if (IsOwner)
        {
            // Owner sends their position to others
            syncedPosition.Value = rb.position;
            syncedVelocity.Value = rb.linearVelocity;
            syncedFacingRight.Value = transform.localScale.x > 0;
        }
        else
        {
            // Non-owners interpolate towards synced position
            Vector2 targetPos = syncedPosition.Value;
            rb.position = Vector2.Lerp(rb.position, targetPos, Time.deltaTime * interpolationSpeed);

            // Apply facing direction
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);
            scale.x = syncedFacingRight.Value ? absX : -absX;
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// Called by PlayerCombat when this player takes damage.
    /// Routes through server so health is authoritative.
    /// </summary>
    public void TakeDamageNetwork(int damage)
    {
        if (IsServer)
        {
            ApplyDamageOnServer(damage);
        }
        else
        {
            TakeDamageServerRpc(damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
    {
        ApplyDamageOnServer(damage);
    }

    private void ApplyDamageOnServer(int damage)
    {
        int reducedDamage = (int)(damage * (1 - attributesController.damageReduction));
        attributesController.currentHealth -= reducedDamage;
        syncedHealth.Value = attributesController.currentHealth;

        if (attributesController.currentHealth <= 0)
        {
            TriggerDeathClientRpc();
        }
        else
        {
            TriggerHurtClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerDeathClientRpc()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetTrigger("Death");
    }

    [ClientRpc]
    private void TriggerHurtClientRpc()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetTrigger("Hurt");
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        attributesController.currentHealth = newValue;
    }

    /// <summary>
    /// Called by the owner to tell the server to play the attack animation on all clients.
    /// </summary>
    [ServerRpc]
    public void AttackServerRpc()
    {
        AttackClientRpc();
    }

    [ClientRpc]
    private void AttackClientRpc()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Request the server to perform hit detection (only host runs physics).
    /// </summary>
    [ServerRpc]
    public void DetectEnemiesHitServerRpc()
    {
        playerCombat.DetectEnemiesHit();
    }
}
