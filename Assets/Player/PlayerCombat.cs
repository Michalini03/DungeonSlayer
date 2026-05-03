using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public AttributesController aController;
    public Animator animator;
    private PlayerMovement pMovement;

    public Transform attackPoint;
    public LayerMask enemyLayers;

    private int currentComboStep = 0;
    private bool canInputNextCombo = true;
    private int currentMaxCombos = 0;
    private float lastAttackSequenceTime = 0f;
    // for animations, dont change 
    private float start = 0f;
    private float cooldown = 1f;

    [Header("Combo Timing System")]
    public float currentComboWindowDuration = 4.0f; // The 4 second total window
    private float comboWindowStartTime = 0f;

    [Header("UI References")]
    public PlayerComboBarUI comboBarUI;
    public GameObject deathCanvas; // Reference to the YOU DIED!
    public GameObject comboFeedbackPrefab; // Reference to the ComboFeedbackText prefab

    [Header("Attack Visual")]
    [SerializeField] private SpriteRenderer attackVisual;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Color attackVisualColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float visualScaleMultiplier = 1f;

    private bool IsInputBlocked()
    {
        return RunController.Instance != null && RunController.Instance.IsGameplayInputBlocked;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pMovement = GetComponent<PlayerMovement>();

        if (attackVisual != null)
        {
            attackVisual.color = attackVisualColor;
            attackVisual.enabled = false;
        }

        start = Time.time - cooldown;
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


        if (comboBarUI != null && currentComboStep > 0 && canInputNextCombo)
        {
            if (canInputNextCombo)
            {
                float elapsedTime = Time.time - comboWindowStartTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / currentComboWindowDuration);

                comboBarUI.SetMarkerPosition(normalizedTime);

                if (elapsedTime >= currentComboWindowDuration)
                {
                    ResetCombo();
                }
            }
            else
            {
                if(Time.time - lastAttackSequenceTime > 1.5f)
                {
                    Debug.Log("combo got stuck, resetting");
                    ResetCombo();
                }
            }
        }
        animator.SetBool("canCombo", canInputNextCombo);
    }

    private void FixedUpdate()
    {
        
    }

    public void Attack()
    {
        if (IsInputBlocked() || aController.currentHealth <= 0)
        {
            return;
        }

        if (!canInputNextCombo)
        {
            return;
        }

        if (currentComboStep == 0)
        {
            currentMaxCombos = animator.GetBool("IsJumping") ? aController.maxAirCombos : aController.maxGroundCombos;
        }

        float staminaMultiplier = 1f;

        if (currentComboStep > 0)
        {
            float elapsedTime = Time.time - comboWindowStartTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / currentComboWindowDuration);
            staminaMultiplier = EvaluateAttackPrecision(normalizedTime);
        }
        else
        {
            if(Time.time - start < cooldown)
            {
                return;
            }
        }

        int finalStaminaCost = Mathf.RoundToInt(aController.attackStaminaCost * staminaMultiplier);

        if (aController.ConsumeStamina(finalStaminaCost))
        {
            pMovement.canDash = false;
            lastAttackSequenceTime = Time.time;

            if (currentComboStep > 0)
            {
                if (staminaMultiplier == 0.5f)
                {
                    SpawnFeedbackText("Perfect!", Color.green);
                }
                else if (staminaMultiplier == 0.75f)
                {
                    SpawnFeedbackText("Good!", new Color(1f, 0.55f, 0f));
                }
                else
                {
                    SpawnFeedbackText("Poor", Color.red);
                }
            }

            canInputNextCombo = false;
            currentComboStep++;

            
            if (currentComboStep > currentMaxCombos)
            {
                ResetCombo();
                return;
            }

            animator.SetInteger("ComboStep", currentComboStep);
            animator.SetTrigger("Attack");
            pMovement.canDash = true;
        }
    }

    private float EvaluateAttackPrecision(float normalizedTime)
    {
        int red = aController.comboBar[0];
        int yellow = aController.comboBar[1];
        int green = aController.comboBar[2];

        float totalWidth = red * 2f + yellow * 2f + green;

        if (totalWidth <= 0f)
        {
            return 1f;
        }

        float greenStart = (red + yellow) / totalWidth;
        float greenEnd = (red + yellow + green) / totalWidth;

        float yellowLeftStart = red / totalWidth;
        float yellowLeftEnd = greenStart;

        float yellowRightStart = greenEnd;
        float yellowRightEnd = (red + yellow + green + yellow) / totalWidth;

        if (normalizedTime >= greenStart && normalizedTime <= greenEnd)
        {
            return 0.5f;    // 50% stamina cost reduction
        }
        else if ((normalizedTime >= yellowLeftStart && normalizedTime < yellowLeftEnd) || (normalizedTime > yellowRightStart && normalizedTime <= yellowRightEnd))
        {
            return 0.75f;   // 25% stamina cost reduction
        }
        else
        {
            return 1f;      // No reduction
        }
    }

    public void DetectEnemiesHit()
    {
        if (IsInputBlocked())
        {
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(GetAttackCenter(), aController.attackRange, enemyLayers);

        //Debug.Log("Hit " + hitEnemies + " enemies!");

        int dealtDamage = GetEffectiveDamage();

        // Pavel: Zde jsem si dovolil drobnou úpravu aby to rovnou fungovalo s novou komponentou
        // Roman: Taky mala uprava (dealtDamage) pro Berserker augment
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHitInfo enemyHitInfo = enemy.gameObject.GetComponent<EnemyHitInfo>();

            if (enemyHitInfo != null)
            {
                Debug.Log("EnemyHitInfo komponenta byla nalezena.");
                enemyHitInfo.manageEnemyHit(dealtDamage);
                ApplyOnHitEffects(dealtDamage);
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
            if (aController.lives == 0)
            {
                ResetCombo();
                animator.SetTrigger("Death");
            }

            else
            {
                aController.lives--;
                aController.currentHealth = aController.maxHealth;
            }
        }
        else
        {
            ResetCombo();
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
        {
            return;
        }

        Gizmos.DrawWireSphere(GetAttackCenter(), aController.attackRange);

    }

    // ==========================================
    // ANIMATION EVENTS 
    // ==========================================

    
    public void OpenComboWindow(float windowDuration)
    {

        if(currentComboStep>=currentMaxCombos)
        {
            animator.ResetTrigger("Attack");
            comboBarUI.HideBar();
            ResetCombo();
            return;
        }

        canInputNextCombo = true;
        currentComboWindowDuration = windowDuration;
        comboWindowStartTime = Time.time;

        if (comboBarUI != null && currentComboStep > 0)
        {
            comboBarUI.ResetBar(aController.comboBar);
        }
    }
    
    public void ResetCombo()
    {
        animator.ResetTrigger("Attack");
        if (currentComboStep > 0)
        {
            start = Time.time;
        }
        currentComboStep = 0;
        canInputNextCombo = true;
        animator.SetInteger("ComboStep", currentComboStep);
        pMovement.canDash = true;

        if (comboBarUI != null)
        {
            comboBarUI.HideBar();
        }
    }

    private void SpawnFeedbackText(string message, Color textColor)
    {
        if (comboFeedbackPrefab == null) return;

        Vector3 spawnPos = transform.position + new Vector3(0f, 0.5f, 0f);

        GameObject feedbackObj = Instantiate(comboFeedbackPrefab, spawnPos, Quaternion.identity);
        ComboFeedbackText script = feedbackObj.GetComponent<ComboFeedbackText>();

        if (script != null)
        {
            script.Setup(message, textColor);
        }
    }


    private int GetEffectiveDamage()
    {
        float damage = aController.damage;

        if (aController.berserker)
        {
            if (aController.maxHealth > 0)
            {
                float healthPercent = (float)aController.currentHealth / aController.maxHealth;

                if (healthPercent <= 0.25f)
                {
                    damage *= 2f;
                }
            }
        }

        return Mathf.RoundToInt(damage);
    }

    private void ApplyOnHitEffects(int dealtDamage)
    {
        if (aController.lifeSteal)
        {
            int healAmount = Mathf.Max(1, Mathf.RoundToInt(dealtDamage * 0.05f));
            aController.Heal(healAmount);
        }
    }

    public void ShowAttackVisual()
    {
        if (attackVisual == null || aController == null)
        {
            return;
        }

        if (attackVisual.sprite == null)
        {
            return;
        }

        attackVisual.enabled = true;
        attackVisual.color = attackVisualColor;

        Vector3 pos = GetAttackCenter();
        pos.z = attackVisual.transform.position.z;
        attackVisual.transform.position = pos;

        float desiredDiameter = aController.attackRange * 2f * visualScaleMultiplier;

        Vector2 spriteSize = attackVisual.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float scaleX = desiredDiameter / spriteSize.x;
        float scaleY = desiredDiameter / (spriteSize.y / 2f);

        attackVisual.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        if (IsFacingLeft())
        {
            attackVisual.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            attackVisual.transform.rotation = Quaternion.identity;
        }
    }

    public void HideAttackVisual()
    {
        if (attackVisual == null)
            return;

        attackVisual.enabled = false;
    }

    private bool IsFacingLeft()
    {
        if (playerSprite != null)
        {
            return playerSprite.flipX;
        }

        return transform.localScale.x < 0f;
    }

    private Vector3 GetAttackCenter()
    {
        bool facingLeft = transform.localScale.x < 0f;
        float direction = facingLeft ? -1f : 1f;

        float distance = aController.attackRange;
        return transform.position + new Vector3(distance * direction, 0f, 0f);
    }

}

