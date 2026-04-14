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

    [Header("UI References")]
    public GameObject deathCanvas; // Reference to the YOU DIED!

    private bool IsInputBlocked()
    {
        return RunController.Instance != null && RunController.Instance.IsGameplayInputBlocked;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (aController.currentHealth <= 0)
        {
            return;
        }

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
        if (IsInputBlocked() || aController.currentHealth <= 0)
            return;

        if (cooldown <= 0 && aController.ConsumeStamina(aController.attackStaminaCost))
        {
            cooldown = attackcooldown;
            animator.SetTrigger("Attack");
        }
    }

    public void DetectEnemiesHit()
    {
        if (IsInputBlocked())
            return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, aController.attackRange, enemyLayers);

        Debug.Log("Hit " + hitEnemies + " enemies!");

        // Pavel: Zde jsem si dovolil drobnou úpravu aby to rovnou fungovalo s novou komponentou
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHitInfo enemyHitInfo = enemy.gameObject.GetComponent<EnemyHitInfo>();

            if (enemyHitInfo != null)
            {
                Debug.Log("EnemyHitInfo komponenta byla nalezena.");
                enemyHitInfo.manageEnemyHit(aController.damage);
            }
            else
            {
                Debug.LogWarning("Neexistuje komponenta EnemyHitInfo na zasaženém objektu.");
            }

        }
    }

    public void takeDamage(int damage)
    {
        if (aController.currentHealth <= 0)
        {
            return;
        }

        aController.TakeDamage(damage);
        if(aController.currentHealth <= 0)
        {
            animator.SetTrigger("Death");

            SaveSystem.DeleteRun();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    public void ShowDeathCanvas()
    {
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || aController == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, aController.attackRange);
    }
}
