using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AugmentDraftService
{
    private readonly System.Random rng;

    private const int CommonChance = 59;
    private const int UncommonChance = 40;
    private const int RareChance = 10;
    private const int LegendaryChance = 1;

    public AugmentDraftService(int seed)
    {
        rng = new System.Random(seed);
    }

    public List<AugmentDefinition> GenerateDraft(IReadOnlyList<AugmentDefinition> allAugments, RunState run, int count = 3)
    {
        List<AugmentDefinition> validAugments = GetValidAugments(allAugments, run);
        List<AugmentDefinition> result = new();

        while (result.Count < count && validAugments.Count > 0)
        {
            AugmentRarity rolledRarity = RollRarity();

            List<AugmentDefinition> rarityPool = validAugments.Where(a => a.rarity == rolledRarity).ToList();

            if (rarityPool.Count == 0)
            {
                rarityPool = validAugments;
            }

            AugmentDefinition picked = PickWeightedRandom(rarityPool);

            if (picked == null)
            {
                break;
            }

            result.Add(picked);
            validAugments.Remove(picked);
        }

        return result;
    }

    private List<AugmentDefinition> GetValidAugments(IReadOnlyList<AugmentDefinition> allAugments, RunState run)
    {
        List<AugmentDefinition> valid = new();

        foreach (AugmentDefinition augment in allAugments)
        {
            if (augment == null)
            {
                continue;
            }

            if (ConflictsWithOwnedAugments(augment, run))
            {
                continue;
            }

            if (run.GetStacks(augment.id) >= augment.maxStacks)
            {
                continue;
            }

            if (augment.baseWeight <= 0)
            {
                continue;
            }

            valid.Add(augment);
        }


        return valid;
    }

    private bool ConflictsWithOwnedAugments(AugmentDefinition augment, RunState run)
    {
        if (augment.excludes == null)
        {
            return false;
        }

        foreach (string excludedId in augment.excludes)
        {
            if (run.HasAugment(excludedId))
            {
                return true;
            }
        }

        return false;
    }

    private AugmentRarity RollRarity()
    {
        int total = CommonChance + UncommonChance + RareChance + LegendaryChance;
        int roll = rng.Next(0, total);

        if (roll < CommonChance)
        {
            return AugmentRarity.Common;
        }

        roll -= CommonChance;
        if (roll < UncommonChance)
        {
            return AugmentRarity.Rare;
        }

        roll -= UncommonChance;
        if (roll < RareChance)
        {
            return AugmentRarity.Rare;
        }

        return AugmentRarity.Legendary;
    }

    private AugmentDefinition PickWeightedRandom(List<AugmentDefinition> augments)
    {
        int totalWeight = 0;

        foreach (AugmentDefinition augment in augments)
        {
            totalWeight += Math.Max(0, augment.baseWeight);
        }

        if (totalWeight <= 0)
        {
            return null;
        }
            
        int roll = rng.Next(0, totalWeight);
        int current = 0;

        foreach (AugmentDefinition augment in augments)
        {
            current += Math.Max(0, augment.baseWeight);

            if (roll < current)
            {
                return augment;
            }
        }

        return augments[augments.Count - 1];
    }
}