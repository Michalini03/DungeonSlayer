using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public AttributesController aController;
    public Animator animator;

    public Transform attackPoint;
    public LayerMask enemyLayers;



    private int currentComboStep = 0;
    private bool canInputNextCombo = true;
    // for animations, dont change 
    private float cooldown = 0f;

    [Header("Combo Timing System")]
    public float currentComboWindowDuration = 1.0f; // The 1 second total window
    private float comboWindowStartTime = 0f;

    [Header("UI References")]
    public GameObject deathCanvas; // Reference to the YOU DIED!
    public GameObject comboFeedbackPrefab; // Reference to the ComboFeedbackText prefab
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
        if (IsInputBlocked() || aController.currentHealth <= 0) return;


        if (canInputNextCombo)
        {


            float elapsedTime = Time.time - comboWindowStartTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime/ currentComboWindowDuration);
            float staminaMultiplier = EvaluateAttackPrecision(normalizedTime);
            int finalStaminaCost = Mathf.RoundToInt(aController.attackStaminaCost * staminaMultiplier);

            if (aController.ConsumeStamina(finalStaminaCost))
            {
                this.GetComponent<PlayerMovement>().canDash = false;
                if(currentComboStep != 0)
                {
                    if (staminaMultiplier == 0.5f)
                    {
                        SpawnFeedbackText("Perfect!", Color.green);
                    }
                    else if (staminaMultiplier == 0.75f)
                    {
                        SpawnFeedbackText("Good!", Color.darkOrange);
                    }
                    else
                    {
                        SpawnFeedbackText("Poor", Color.red);
                    }
                }
                

                canInputNextCombo = false;
                currentComboStep++;

                int maxCombos = animator.GetBool("IsJumping") ? aController.maxAirCombos : aController.maxGroundCombos;
                if(currentComboStep > maxCombos)
                {
                    currentComboStep = 1;
                }
                animator.SetInteger("ComboStep", currentComboStep);
                animator.SetTrigger("Attack");

            }
        }

    }

    private float EvaluateAttackPrecision(float normalizedTime)
    {
        float distanceFromPerfect = Mathf.Abs(normalizedTime - 0.5f)*10;
        Debug.Log("Distance from center: " + (normalizedTime - 0.5f));
        if (distanceFromPerfect < aController.staminaBar[2] / 2f)
        {
            return 0.5f; // 50% stamina cost reduction
        }
        else if(distanceFromPerfect < aController.staminaBar[2]/ 2f + aController.staminaBar[1] / 2f)
        {
            return 0.75f; // 25% stamina cost reduction
        }
        else
        {
            return 1f; // No reduction
        }
    }

    public void DetectEnemiesHit()
    {
        if (IsInputBlocked())
            return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, aController.attackRange, enemyLayers);

        

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
        Gizmos.DrawCube(new Vector3(this.transform.position.x,this.transform.position.y-0.6f), new Vector3(0.4f,0.3f,0));
    }

    // ==========================================
    // ANIMATION EVENTS 
    // ==========================================

    // Calling this slightly after the hit lands. Allows the player to chain the next hit.
    public void OpenComboWindow(float windowDuration)
    {
        canInputNextCombo = true;
        currentComboWindowDuration = windowDuration;
        comboWindowStartTime = Time.time; // Record the exact moment the window opens
        
    }

    // Calling this at the very end of every attack animation's recovery frames.
    public void ResetCombo()
    {
        currentComboStep = 0;
        canInputNextCombo = true;
        animator.SetInteger("ComboStep", 0);
        this.GetComponent<PlayerMovement>().canDash = true;


    }

    private void SpawnFeedbackText(string message, Color textColor)
    {
        if (comboFeedbackPrefab == null) return;

        // Spawn it slightly above the player's head (adjust the Y offset as needed)
        Vector3 spawnPos = transform.position + new Vector3(0f, 0.5f, 0f);

        GameObject feedbackObj = Instantiate(comboFeedbackPrefab, spawnPos, Quaternion.identity);
        ComboFeedbackText script = feedbackObj.GetComponent<ComboFeedbackText>();

        if (script != null)
        {
            script.Setup(message, textColor);
        }
    }
}
