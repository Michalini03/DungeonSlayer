using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Syncs enemy position and state from host to clients.
/// Enemies are host-authoritative: only the host runs AI/physics.
/// Attach to enemy prefabs alongside their existing behavior scripts.
/// </summary>
public class NetworkEnemySync : NetworkBehaviour
{
    private Rigidbody2D rb;

    private NetworkVariable<Vector2> syncedPosition = new NetworkVariable<Vector2>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<Vector3> syncedScale = new NetworkVariable<Vector3>(
        Vector3.one,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> syncedIsDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private float interpolationSpeed = 12f;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!IsServer)
        {
            // Clients don't run enemy AI - disable behavior scripts
            var skeleton = GetComponent<EnemyMovement>();
            if (skeleton != null) skeleton.enabled = false;

            var flyingEye = GetComponent<FlyingEyeBehavior>();
            if (flyingEye != null) flyingEye.enabled = false;

            // Make kinematic on client
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        syncedIsDead.OnValueChanged += OnDeadChanged;
    }

    private void Update()
    {
        if (!IsSpawned) return;

        if (IsServer)
        {
            syncedPosition.Value = rb != null ? rb.position : (Vector2)transform.position;
            syncedScale.Value = transform.localScale;
        }
        else
        {
            // Interpolate position on clients
            Vector2 targetPos = syncedPosition.Value;
            if (rb != null)
                rb.position = Vector2.Lerp(rb.position, targetPos, Time.deltaTime * interpolationSpeed);
            else
                transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * interpolationSpeed);

            transform.localScale = syncedScale.Value;
        }
    }

    public void SetDead()
    {
        if (IsServer)
        {
            syncedIsDead.Value = true;
        }
    }

    private void OnDeadChanged(bool oldValue, bool newValue)
    {
        if (newValue && !IsServer)
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("tookHit");
                animator.SetBool("isDead", true);
            }
        }
    }

    [ClientRpc]
    public void TriggerHitAnimationClientRpc()
    {
        if (IsServer) return;

        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("tookHit");
        }
    }

    [ClientRpc]
    public void TriggerAttackAnimationClientRpc(bool isAttacking)
    {
        if (IsServer) return;

        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Attack", isAttacking);
        }
    }
}
