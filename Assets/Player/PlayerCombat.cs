using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

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
        if(cooldown<=0 && !animator.GetBool("IsJumping"))
        {
            cooldown = attackcooldown;
            animator.SetTrigger("Attack");

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            List<GameObject> hitEnemyObjects = new List<GameObject>();

            foreach (Collider2D enemy in hitEnemies)
            {
                GameObject enemyObject = enemy.gameObject;

                if (!hitEnemyObjects.Contains(enemyObject))
                {
                    hitEnemyObjects.Add(enemyObject);
                    Debug.Log("Hit " + enemy.name);
                }

            }
        }


        
    }

    private void OnDrawGizmosSelected()
    {

        if(attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
