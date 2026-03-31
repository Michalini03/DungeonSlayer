using System.Collections.Generic;

public static class BuildCalculator
{
    public static PlayerBuildStats BuildStats(
        AttributesController attributes,
        RunState run,
        IReadOnlyList<AugmentSynergyDefinition> allSynergies)
    {
        PlayerBuildStats stats = new PlayerBuildStats();

        stats.maxHealth.BaseValue = attributes.baseMaxHealth;
        stats.damage.BaseValue = attributes.baseDamage;
        stats.healthRegen.BaseValue = attributes.baseHealthRegen;
        stats.lives.BaseValue = attributes.baseLives;
        stats.damageReduction.BaseValue = attributes.baseDamageReduction;

        stats.attackRange.BaseValue = attributes.baseAttackRange;
        stats.knockbackForce.BaseValue = attributes.baseKnockbackForce;

        stats.maxStamina.BaseValue = attributes.baseMaxStamina;
        stats.staminaBar = (int[])attributes.baseStaminaBar.Clone();

        stats.iframesDuration.BaseValue = attributes.baseIframesDuration;
        stats.movementSpeed.BaseValue = attributes.baseMovementSpeed;

        List<AugmentSynergyDefinition> activeSynergies =
            SynergyResolver.GetActiveSynergies(run, allSynergies);

        BuildContext context = new BuildContext(run, activeSynergies);

        foreach (var kv in run.OwnedStacks)
        {
            ApplyAugmentEffect(kv.Key, kv.Value, stats, context);
        }

        foreach (var synergy in activeSynergies)
        {
            ApplySynergyEffect(synergy.id, stats, context);
        }

        return stats;
    }

    private static void ApplyAugmentEffect(
        string augmentId,
        int stacks,
        PlayerBuildStats stats,
        BuildContext context)
    {
        switch (augmentId)
        {
            // Common
            case "attack_size_up":
                stats.damageReduction.FlatBonus += 0.10f * stacks;
                break;

            case "combo_bar_size_up":
                ApplyStaminaUp(stats, stacks);
                break;

            case "damage_up":
                stats.damage.FlatBonus += 15 * stacks;
                break;

            case "health_up":
                stats.maxHealth.FlatBonus += 40 * stacks;
                break;

            case "knockback_up":
                stats.knockbackForce.FlatBonus += 2 * stacks;
                break;

            case "movement_up":
                stats.movementSpeed.FlatBonus += 0.1f * stacks;
                break;

            case "stamina_up":
                stats.maxStamina.FlatBonus += 20 * stacks;
                break;

            // Uncommon

            // Rare

            // Legendary
        }
    }

    private static void ApplySynergyEffect(
        string synergyId,
        PlayerBuildStats stats,
        BuildContext context)
    {
        switch (synergyId)
        {

        }
    }

    private static void ApplyStaminaUp(PlayerBuildStats stats, int stacks)
    {
        if (stats.staminaBar == null || stats.staminaBar.Length < 3)
            return;

        for (int i = 0; i < stacks; i++)
        {
            stats.staminaBar[2] += 1;

            if (stats.staminaBar[1] > 0)
            {
                stats.staminaBar[1] -= 1;
            }
            else if (stats.staminaBar[0] > 0)
            {
                stats.staminaBar[0] -= 1;
            }
        }
    }
}