using NUnit.Framework;
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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (cooldown <= 0)
        {
            cooldown = attackcooldown;
            animator.SetTrigger("Attack");
            
            
        }
    }

    public void DetectEnemiesHit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, aController.AttackRange, enemyLayers);
        List<GameObject> hitEnemyObjects = new List<GameObject>();

        foreach (Collider2D enemy in hitEnemies)
        {
            GameObject enemyObject = enemy.gameObject;

            if (!hitEnemyObjects.Contains(enemyObject))
            {
                hitEnemyObjects.Add(enemyObject);
                enemyObject.GetComponent<EnemyMovement>().manageEnemyHit(aController.Damage);
            }

        }
    }

    public void takeDamage(int damage)
    {
        if(aController.currentHealth <= 0)
        {
            return;
        }

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

        if(attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, aController.AttackRange);
    }
}
