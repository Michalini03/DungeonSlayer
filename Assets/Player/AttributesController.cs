using System.Collections;
using System.Text;
using UnityEngine;
using System;

public class AttributesController : MonoBehaviour
{
    public event Action<int, int> OnHealthChange; 
    public event Action<int, int> OnStaminaChange;

    public int baseMaxHealth = 120;
    public int baseDamage = 50;
    public int baseHealthRegen = 0;
    public int baseLives = 0;
    public float baseDamageReduction = 0f;
    public float baseAttackRange = 0.45f;
    public float baseKnockbackForce = 1f;
    public int baseMaxStamina = 100;
    public int[] baseComboBar = new int[3] { 8, 8, 4 };
    public float baseIframesDuration = 0.5f;
    public float baseMovementSpeed = 10f;

    public const int minMaxHealth = 1;
    public const int minDamage = 0;
    public const int minHealthRegen = 0;
    public const int minLives = 0;
    public const float minAttackRange = 0.4f;
    public const float minKnockbackForce = 0f;
    public const int minMaxStamina = 50;
    public const int minComboBarSectionValue = 0;

    public const float minDamageReduction = -1f;
    public const float maxDamageReduction = 0.5f;

    public const float minIframesDuration = 0.1f;
    public const float maxIframesDuration = 2f;

    public const float minMovementSpeed = 1f;
    public const float maxMovementSpeed = 20f;

    private int _maxHealth;
    public int maxHealth
    {
        get => _maxHealth;
        set { _maxHealth = value; OnHealthChange?.Invoke(currentHealth, maxHealth); }
    }

    private int _currentHealth;
    public int currentHealth
    {
        get => _currentHealth; 
        set { _currentHealth = value; OnHealthChange?.Invoke(currentHealth,maxHealth);  }
    }
    public int damage;
    public int healthRegen;
    public int lives;
    public float damageReduction;

    public float attackRange;
    public float knockbackForce;

    private int _maxStamina;
    public int maxStamina
    {
        get => _maxStamina;
        set { _maxStamina = value; OnStaminaChange?.Invoke(currentStamina, maxStamina); }
    }
    private int _currentStamina;
    public int currentStamina
    {
        get => _currentStamina;
        set { _currentStamina = value; OnStaminaChange?.Invoke(currentStamina, maxStamina); }
    }
    public int[] comboBar;

    public float iframesDuration;
    public float movementSpeed;

    
    [Header("Abilities")]
    public int maxGroundCombos; //bude lehci jen zvedat cislo nez mit pro kazde odemcene kombo vlastni bool
    public int maxAirCombos; //stejne tady
    public int maxJumps;
    public bool berserker;
    public bool lifeSteal;
    public bool martyrsBlood;

    //marek: for ui updated, will be on more lines marked by comment //PlayerUI
    [Header("Player UI")]
    public PlayerUI playerUI;

    [Header("Stamina cost settings")]
    public int attackStaminaCost = 20;
    public int dashStaminaCost = 30;
    public int jumpStaminaCost = 15;

    [Header("Stamina Settings")]
    public float staminaRegenRate = 20f;        // Stamina per second
    public float staminaRegenDelay = 0.5f;      // Time to wait before starting regen
    private float lastStaminaUseTime;

    private void Awake()
    {
        ResetToBaseStats();
        StartCoroutine(RegenStaminaLoop());
        StartCoroutine(RegenHealthLoop());
    }

    private void RefreshUI()
    {
        if (playerUI == null)
        {
            return;
        }

        playerUI.UpdateHealthBar(currentHealth, maxHealth);
        playerUI.UpdateStaminaBar(currentStamina, maxStamina);
    }

    private void SetMaxHealthPreservePercent(int newMaxHealth)
    {
        float percent = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;

        maxHealth = Mathf.Max(newMaxHealth, minMaxHealth);
        currentHealth = Mathf.Clamp(Mathf.RoundToInt(maxHealth * percent), 0, maxHealth);
    }
    private void SetMaxStaminaPreservePercent(int newMaxStamina)
    {
        float percent = maxStamina > 0 ? (float)currentStamina / maxStamina : 1f;

        maxStamina = Mathf.Max(newMaxStamina, minMaxStamina);
        currentStamina = Mathf.Clamp(Mathf.RoundToInt(maxStamina * percent), 0, maxStamina);
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
        comboBar = (int[])baseComboBar.Clone();

        iframesDuration = baseIframesDuration;
        movementSpeed = baseMovementSpeed;

        maxGroundCombos = 2;
        maxAirCombos = 1;
        maxJumps = 1;
        berserker = false;
        lifeSteal = false;
        martyrsBlood = false;


        currentHealth = maxHealth;
        currentStamina = maxStamina;

        RefreshUI();
    }
    public void ApplyCalculatedStats(PlayerBuildStats stats)
    {
        int newMaxHealth = Mathf.Max(
            Mathf.RoundToInt(stats.maxHealth.FinalValue),
            minMaxHealth
        );

        int newDamage = Mathf.Max(
            Mathf.RoundToInt(stats.damage.FinalValue),
            minDamage
        );

        int newHealthRegen = Mathf.Max(
            Mathf.RoundToInt(stats.healthRegen.FinalValue),
            minHealthRegen
        );

        int newLives = Mathf.Max(
            Mathf.RoundToInt(stats.lives.FinalValue),
            minLives
        );

        float newDamageReduction = Mathf.Clamp(
            stats.damageReduction.FinalValue,
            minDamageReduction,
            maxDamageReduction
        );

        float newAttackRange = Mathf.Max(
            stats.attackRange.FinalValue,
            minAttackRange
        );

        float newKnockbackForce = Mathf.Max(
            stats.knockbackForce.FinalValue,
            minKnockbackForce
        );

        int newMaxStamina = Mathf.Max(
            Mathf.RoundToInt(stats.maxStamina.FinalValue),
            minMaxStamina
        );

        float newIframesDuration = Mathf.Clamp(
            stats.iframesDuration.FinalValue,
            minIframesDuration,
            maxIframesDuration
        );

        float newMovementSpeed = Mathf.Clamp(
            stats.movementSpeed.FinalValue,
            minMovementSpeed,
            maxMovementSpeed
        );

        SetMaxHealthPreservePercent(newMaxHealth);
        SetMaxStaminaPreservePercent(newMaxStamina);

        damage = newDamage;
        healthRegen = newHealthRegen;
        lives = newLives;
        damageReduction = newDamageReduction;
        attackRange = newAttackRange;
        knockbackForce = newKnockbackForce;
        iframesDuration = newIframesDuration;
        movementSpeed = newMovementSpeed;

        if (stats.comboBar == null || stats.comboBar.Length != baseComboBar.Length)
        {
            comboBar = (int[])baseComboBar.Clone();
        }
        else
        {
            comboBar = (int[])stats.comboBar.Clone();

            for (int i = 0; i < comboBar.Length; i++)
            {
                comboBar[i] = Mathf.Max(comboBar[i], minComboBarSectionValue);
            }
        }

        RefreshUI();
    }


    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        
    }

    public void HealPercent(float percent)
    {
        if (percent <= 0f)
        {
            return;
        }

        int amount = Mathf.RoundToInt(maxHealth * percent);
        Heal(amount);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;

        
    }

    public void AddMaxHealth(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        maxHealth += amount;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int effectiveDamage = Mathf.RoundToInt(amount * (1f - damageReduction));
        currentHealth = Mathf.Max(currentHealth - effectiveDamage, 0);

        
    }
    public void RegenerateHealth(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // PlayerUI
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
    }

    private IEnumerator RegenHealthLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);

        while (true)
        {
            if (martyrsBlood)
            {
                if (currentHealth < maxHealth)
                {
                    RegenerateHealth((int)(maxHealth * healthRegen * 0.01f));
                }
            }
            else
            {
                if (currentHealth < (maxHealth / 2))
                {
                    RegenerateHealth((int)(maxHealth * healthRegen * 0.01f));
                }
            }

            yield return wait;
        }
    }

    public bool ConsumeStamina(int amount)
    {
        if (amount <= 0 || currentStamina < amount)
        {
            return false;
        }

        currentStamina -= amount;
        lastStaminaUseTime = Time.time;

        
        return true;
    }

    public void RegenerateStamina(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // PlayerUI
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        
    }

    public void AddMaxStamina(int amount)
    {
        if (amount <= 0)
            return;

        maxStamina += amount;
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        
    }

    private IEnumerator RegenStaminaLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (true)
        {
            // 1. Check if the delay has passed
            if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
            {
                if (currentStamina < maxStamina)
                {
                    RegenerateStamina((int)(staminaRegenRate * 0.1f));
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
        sb.AppendLine($"Current Stamina: {currentStamina}");
        sb.AppendLine($"Iframes Duration: {iframesDuration}");
        sb.AppendLine($"Movement Speed: {movementSpeed}");

        if (comboBar != null)
        {
            sb.Append("Stamina Bar: [");
            for (int i = 0; i < comboBar.Length; i++)
            {
                sb.Append(comboBar[i]);
                if (i < comboBar.Length - 1)
                    sb.Append(", ");
            }
            sb.AppendLine("]");
        }

        Debug.Log(sb.ToString());
    }
}