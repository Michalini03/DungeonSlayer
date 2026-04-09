using System.Collections;
using System.Text;
using UnityEngine;

public class AttributesController : MonoBehaviour
{
    public int baseMaxHealth = 120;
    public int baseDamage = 50;
    public int baseHealthRegen = 0;
    public int baseLives = 0;
    public float baseDamageReduction = 0f;

    public float baseAttackRange = 10f;
    public float baseKnockbackForce = 10f;

    public int baseMaxStamina = 100;
    public int[] baseStaminaBar = new int[3] { 6, 3, 1 };

    public float baseIframesDuration = 0.5f;
    public float baseMovementSpeed = 10f;

    public int minMaxHealth = 1;
    public int minDamage = 0;
    public int minHealthRegen = 0;
    public int minLives = 0;
    public float minDamageReduction = -1f;
    public float maxDamageReduction = 0.5f;

    public float minAttackRange = 5f;
    public float minKnockbackForce = 0f;

    public int minMaxStamina = 50;
    public int minStaminaBarSectionValue = 0;
        
    public float minIframesDuration = 0.1f;
    public float maxIframesDuration = 2f;

    public float minMovementSpeed = 1f;
    public float maxMovementSpeed = 20f;

    public int maxHealth;
    public int currentHealth;
    public int damage;
    public int healthRegen;
    public int lives;
    public float damageReduction;

    public float attackRange;
    public float knockbackForce;

    public int maxStamina;
    public int currentStamina;
    public int[] staminaBar;

    public float iframesDuration;
    public float movementSpeed;

    [Header("Abilities")]
    public bool canComboAttack3;
    public bool canAirComboAttack2;

    //marek: for ui updated, will be on more lines marked by comment //PlayerUI
    [Header("Player UI")]
    public PlayerUI playerUI;

    [Header("Stamina cost settings")]
    public int attackStaminaCost = 20;
    public int dashStaminaCost = 30;
    public int jumpStaminaCost = 15;

    [Header("Stamina Settings")]
    public float regenRate = 15f;      // Stamina per second
    public float regenDelay = 1f;      // Time to wait before starting regen
    private float lastStaminaUseTime;

    private void Awake()
    {
        ResetToBaseStats();
        StartCoroutine(RegenStaminaLoop());
    }

    public void ResetToBaseStats()
    {
        maxHealth = baseMaxHealth;
        damage = baseDamage;
        healthRegen = baseHealthRegen;
        lives = baseLives;
        damageReduction = baseDamageReduction;

        attackRange = baseAttackRange;
        knockbackForce = baseKnockbackForce;

        maxStamina = baseMaxStamina;
        staminaBar = (int[])baseStaminaBar.Clone();

        iframesDuration = baseIframesDuration;
        movementSpeed = baseMovementSpeed;

        canComboAttack3 = false;
        canAirComboAttack2 = false;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        //PlayerUI
        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
            playerUI.UpdateStaminaBar(currentStamina, maxStamina);
        }
    }

    public void ApplyCalculatedStats(PlayerBuildStats stats)
    {
        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;

        maxHealth = Mathf.Max(
            Mathf.RoundToInt(stats.maxHealth.FinalValue),
            minMaxHealth
        );

        damage = Mathf.Max(
            Mathf.RoundToInt(stats.damage.FinalValue),
            minDamage
        );

        healthRegen = Mathf.Max(
            Mathf.RoundToInt(stats.healthRegen.FinalValue),
            minHealthRegen
        );

        lives = Mathf.Max(
            Mathf.RoundToInt(stats.lives.FinalValue),
            minLives
        );

        damageReduction = Mathf.Clamp(
            stats.damageReduction.FinalValue,
            minDamageReduction,
            maxDamageReduction
        );

        attackRange = Mathf.Max(
            stats.attackRange.FinalValue,
            minAttackRange
        );

        knockbackForce = Mathf.Max(
            stats.knockbackForce.FinalValue,
            minKnockbackForce
        );

        maxStamina = Mathf.Max(
            Mathf.RoundToInt(stats.maxStamina.FinalValue),
            minMaxStamina
        );

        if (stats.staminaBar == null || stats.staminaBar.Length != baseStaminaBar.Length)
        {
            staminaBar = (int[])baseStaminaBar.Clone();
        }
        else
        {
            staminaBar = (int[])stats.staminaBar.Clone();

            for (int i = 0; i < staminaBar.Length; i++)
            {
                staminaBar[i] = Mathf.Max(staminaBar[i], minStaminaBarSectionValue);
            }
        }

        iframesDuration = Mathf.Clamp(
            stats.iframesDuration.FinalValue,
            minIframesDuration,
            maxIframesDuration
        );

        movementSpeed = Mathf.Clamp(
            stats.movementSpeed.FinalValue,
            minMovementSpeed,
            maxMovementSpeed
        );

        currentHealth = Mathf.Clamp(
            Mathf.RoundToInt(maxHealth * healthPercent),
            minMaxHealth,
            maxHealth
        );
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        //PlayerUI
        if(playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void HealPercent(float percent)
    {
        if (percent <= 0f)
            return;

        int amount = Mathf.RoundToInt(maxHealth * percent);
        Heal(amount);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;

        //PlayerUI - pozn. proc se neda pouzit Heal(maxHealth);????
        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void AddMaxHealth(int amount)
    {
        if (amount <= 0)
            return;
        maxHealth += amount;
        currentHealth += amount;
        //PlayerUI
        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;
        int effectiveDamage = Mathf.RoundToInt(amount * (1f - damageReduction));
        currentHealth = Mathf.Max(currentHealth - effectiveDamage, 0);
        //PlayerUI
        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public bool ConsumeStamina(int amount)
    {
        if (amount <= 0 || currentStamina < amount)
            return false;
        currentStamina -= amount;
        lastStaminaUseTime = Time.time;

        if (playerUI != null)
        {
            playerUI.UpdateStaminaBar(currentStamina, maxStamina);
        }
        return true;
    }

    public void RegenerateStamina(int amount)
    {
        if (amount <= 0)
            return;
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);

        if (playerUI != null)
        {
            playerUI.UpdateStaminaBar(currentStamina, maxStamina);
        }
    }

    public void AddMaxStamina(int amount)
    {
        if (amount <= 0)
            return;
        maxStamina += amount;
        currentStamina += amount;
        
        if (playerUI != null)
        {
            playerUI.UpdateStaminaBar(currentStamina, maxStamina);
        }
    }

    private IEnumerator RegenStaminaLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (true)
        {
            // 1. Check if the delay has passed
            if (Time.time - lastStaminaUseTime >= regenDelay)
            {
                if (currentStamina < maxStamina)
                {
                    RegenerateStamina((int)(regenRate * 0.1f));
                }
            }

            // 3. Wait for 0.1 seconds before running again
            yield return wait;
        }
    }


    public void DebugPrintStats()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== PLAYER STATS ===");
        sb.AppendLine($"HP: {currentHealth}/{maxHealth}");
        sb.AppendLine($"Damage: {damage}");
        sb.AppendLine($"Health Regen: {healthRegen}");
        sb.AppendLine($"Lives: {lives}");
        sb.AppendLine($"Damage Reduction: {damageReduction}");
        sb.AppendLine($"Attack Range: {attackRange}");
        sb.AppendLine($"Knockback Force: {knockbackForce}");
        sb.AppendLine($"Max Stamina: {maxStamina}");
        sb.AppendLine($"Iframes Duration: {iframesDuration}");
        sb.AppendLine($"Movement Speed: {movementSpeed}");

        if (staminaBar != null)
        {
            sb.Append("Stamina Bar: [");
            for (int i = 0; i < staminaBar.Length; i++)
            {
                sb.Append(staminaBar[i]);
                if (i < staminaBar.Length - 1)
                    sb.Append(", ");
            }
            sb.AppendLine("]");
        }

        Debug.Log(sb.ToString());
    }
}