using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public AttributesController aController;
    public Animator animator;

    public Transform attackPoint;
    public LayerMask enemyLayers;

    // for animations, dont change
    float cooldown = 0f;
    float attackcooldown = 0.5f;

    private bool IsInputBlocked()
    {
        return RunController.Instance != null && RunController.Instance.IsGameplayInputBlocked;
    }

    private bool IsMultiplayer()
    {
        return GameNetworkManager.Instance != null && GameNetworkManager.Instance.IsMultiplayer;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInputBlocked() && animator != null)
        {
            animator.ResetTrigger("Attack");
        }
    }

    private void FixedUpdate()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.fixedDeltaTime;
        }
    }

    public void Attack()
    {
        if (IsInputBlocked())
            return;

        if (cooldown <= 0)
        {
            cooldown = attackcooldown;
            animator.SetTrigger("Attack");

            // In multiplayer, broadcast attack animation to other clients
            if (IsMultiplayer())
            {
                var netPlayer = GetComponent<NetworkPlayerController>();
                if (netPlayer != null && netPlayer.IsOwner)
                {
                    netPlayer.AttackServerRpc();
                }
            }
        }
    }

    public void DetectEnemiesHit()
    {
        if (IsInputBlocked())
            return;

        // In multiplayer, only the server should do hit detection
        if (IsMultiplayer())
        {
            var netPlayer = GetComponent<NetworkPlayerController>();
            if (netPlayer != null)
            {
                if (netPlayer.IsServer)
                {
                    PerformHitDetection();
                }
                else if (netPlayer.IsOwner)
                {
                    netPlayer.DetectEnemiesHitServerRpc();
                }
                return;
            }
        }

        PerformHitDetection();
    }

    public void PerformHitDetection()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, aController.attackRange, enemyLayers);
        List<GameObject> hitEnemyObjects = new List<GameObject>();

        Debug.Log("Hit " + hitEnemies + " enemies!");

        foreach (Collider2D enemy in hitEnemies)
        {
            GameObject enemyObject = enemy.gameObject;

            if (!hitEnemyObjects.Contains(enemyObject))
            {
                hitEnemyObjects.Add(enemyObject);
                if(enemyObject.GetComponent<EnemyMovement>() != null)
                    enemyObject.GetComponent<EnemyMovement>().manageEnemyHit(aController.damage);
                else if(enemyObject.GetComponent<FlyingEyeBehavior>() != null)
                    enemyObject.GetComponent<FlyingEyeBehavior>().manageEnemyHit(aController.damage);
            }
        }
    }

    public void takeDamage(int damage)
    {
        if(aController.currentHealth <= 0)
        {
            return;
        }

        // In multiplayer, route damage through the network
        if (IsMultiplayer())
        {
            var netPlayer = GetComponent<NetworkPlayerController>();
            if (netPlayer != null)
            {
                netPlayer.TakeDamageNetwork(damage);
                return;
            }
        }

        // Single-player path
        aController.currentHealth -= (int)(damage*(1-aController.damageReduction));
        if(aController.currentHealth <= 0)
        {
            animator.SetTrigger("Death");
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || aController == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, aController.attackRange);
    }
}
