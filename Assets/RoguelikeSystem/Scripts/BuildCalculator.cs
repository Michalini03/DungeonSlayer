using JetBrains.Annotations;
using System.Collections.Generic;

public static class BuildCalculator
{
    public static PlayerBuildStats BuildStats(AttributesController attributes, RunState run, IReadOnlyList<AugmentSynergyDefinition> allSynergies)
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
        stats.comboBar = (int[])attributes.baseComboBar.Clone();

        stats.iframesDuration.BaseValue = attributes.baseIframesDuration;
        stats.movementSpeed.BaseValue = attributes.baseMovementSpeed;

        List<AugmentSynergyDefinition> activeSynergies = SynergyResolver.GetActiveSynergies(run, allSynergies);

        BuildContext context = new BuildContext(run, activeSynergies);

        foreach (var kv in run.OwnedStacks)
        {
            ApplyAugmentEffect(kv.Key, kv.Value, stats, context, attributes);
        }

        foreach (var synergy in activeSynergies)
        {
            ApplySynergyEffect(synergy.id, stats, context);
        }

        return stats;
    }

    private static void ApplyAugmentEffect(string augmentId, int stacks, PlayerBuildStats stats, BuildContext context, AttributesController attributes)
    {
        switch (augmentId)
        {
            // Common
            case "attack_size_up":
                stats.attackRange.FlatBonus += 0.05f * stacks;
                break;

            case "combo_bar_size_up":
                ApplyComboUp(stats, stacks);
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
            case "invincibility_up":
                stats.iframesDuration.FlatBonus += 0.1f * stacks;
                break;

            case "oaken_armor":
                stats.damageReduction.FlatBonus += 0.25f;

                stats.movementSpeed.ApplyMinMultiplier(0.75f);
                break;

            case "oaken_shield":
                stats.damageReduction.FlatBonus += 0.1f;
                break;

            case "weight_of_sins":
                stats.knockbackForce.FlatBonus += 10f;

                stats.movementSpeed.ApplyMinMultiplier(0.8f);
                break;

            // Rare
            case "berserker":
                attributes.berserker = true;
                break;

            case "crown_of_thorns":
                stats.damage.Multiplier *= 2f;

                stats.damageReduction.FlatBonus -= 1f;
                break;

            case "life_steal":
                attributes.lifeSteal = true;
                break;

            case "life_up":
                stats.lives.FlatBonus += 1;
                break;

            case "regeneration":
                stats.healthRegen.FlatBonus += 5;
                break;

            // Legendary
            case "griffith_attack":
                ApplyComboMax(stats);
                stats.knockbackForce.FlatBonus -= 5f;

                stats.damage.ApplyMinMultiplier(0.5f);
                break;

            case "guts_attack":
                stats.damage.Multiplier *= 2f;
                stats.knockbackForce.FlatBonus += 10f;
                stats.attackRange.FlatBonus += 0.5f;

                ApplyComboMin(stats);
                break;

            case "martyrs_blood":
                attributes.martyrsBlood = true;
                stats.healthRegen.FlatBonus += 10;

                stats.maxHealth.Multiplier *= 0.5f;
                break;
        }
    }

    private static void ApplySynergyEffect(string synergyId, PlayerBuildStats stats, BuildContext context)
    {
        switch (synergyId)
        {
            case "the_holy_sword":
                ApplyComboUp(stats, 1);
                stats.damage.FlatBonus += 50;
                stats.knockbackForce.FlatBonus += 10f;
                stats.movementSpeed.FlatBonus += 10f;
                stats.attackRange.FlatBonus += 0.5f;
                break;

            default:
                break;
        }
    }

    private static void ApplyComboUp(PlayerBuildStats stats, int stacks)
    {
        if (stats.comboBar == null || stats.comboBar.Length < 3)
        {
            return;
        }

        for (int i = 0; i < stacks; i++)
        {
            stats.comboBar[2] += 1;

            if (stats.comboBar[0] > 0)
            {
                stats.comboBar[0] -= 1;
            }
            else if (stats.comboBar[1] > 0)
            {
                stats.comboBar[1] -= 1;
            }
        }
    }

    private static void ApplyComboMax(PlayerBuildStats stats)
    {
        if (stats.comboBar == null || stats.comboBar.Length < 3)
        {
            return;
        }

        stats.comboBar[2] += stats.comboBar[0] + stats.comboBar[1];

        stats.comboBar[0] = 0;
        stats.comboBar[1] = 0;
    }

    private static void ApplyComboMin(PlayerBuildStats stats)
    {
        if (stats.comboBar == null || stats.comboBar.Length < 3)
        {
            return;
        }

        stats.comboBar[2] = 1;
        stats.comboBar[0] = 9;
        stats.comboBar[1] = 0;
    }
}