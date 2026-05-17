using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AugmentDraftService
{
    private readonly System.Random rng;

    private int[] forestChance = new int[4] { 49, 40, 10, 1 };
    private int[] caveChance = new int[4] { 45, 30, 20, 5 };
    private int[] castleChance = new int[4] { 30, 30, 20, 10 };

    public AugmentDraftService(int seed)
    {
        rng = new System.Random(seed);
    }

    public List<AugmentDefinition> GenerateDraft(IReadOnlyList<AugmentDefinition> allAugments, RunState run)
    {
        List<AugmentDefinition> validAugments = GetValidAugments(allAugments, run);
        List<AugmentDefinition> result = new();

        int otherSwitch = rng.Next(0, 5);
        switch (otherSwitch)
        {
            case 0:
                result.Add(PickAugment(AugmentType.Other, validAugments));
                result.Add(PickAugment(AugmentType.Defence, validAugments));
                result.Add(PickAugment(AugmentType.Mobility, validAugments));
                break;

            case 1:
                result.Add(PickAugment(AugmentType.Offence, validAugments));
                result.Add(PickAugment(AugmentType.Other, validAugments));
                result.Add(PickAugment(AugmentType.Mobility, validAugments));
                break;

            case 2:
                result.Add(PickAugment(AugmentType.Offence, validAugments));
                result.Add(PickAugment(AugmentType.Defence, validAugments));
                result.Add(PickAugment(AugmentType.Other, validAugments));
                break;

            default:
                result.Add(PickAugment(AugmentType.Offence, validAugments));
                result.Add(PickAugment(AugmentType.Defence, validAugments));
                result.Add(PickAugment(AugmentType.Mobility, validAugments));
                break;
        }

        return result;
    }

    private AugmentDefinition PickAugment(AugmentType type, List<AugmentDefinition> validAugments)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string[] parts = sceneName.Split('_');
        string levelType = parts.Length > 1 ? parts[1].ToLower() : sceneName.ToLower();

        AugmentRarity rolledRarity = RollRarity(levelType);

        List<AugmentDefinition> typePool = validAugments.Where(a => a.type == type).ToList();

        if (typePool.Count == 0)
        {
            typePool = validAugments;
        }

        List<AugmentDefinition> rarityPool = typePool.Where(a => a.rarity == rolledRarity).ToList();

        if (rarityPool.Count == 0)
        {
            rarityPool = typePool;
        }

        return PickWeightedRandom(rarityPool);
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

    private AugmentRarity RollRarity(string curSceneName)
    {
        int common, uncommon, rare, legendary;

        switch (curSceneName)
        {
            case "forest":
                common = forestChance[0];
                uncommon = forestChance[1];
                rare = forestChance[2];
                legendary = forestChance[3];
                break;

            case "cave":
                common = caveChance[0];
                uncommon = caveChance[1];
                rare = caveChance[2];
                legendary = caveChance[3];
                break;

            case "castle":
                common = castleChance[0];
                uncommon = castleChance[1];
                rare = castleChance[2];
                legendary = castleChance[3];
                break;

            default:
                common = 25;
                uncommon = 25;
                rare = 25;
                legendary = 25;
                break;
        }

        int total = common + uncommon + rare + legendary;
        int roll = rng.Next(0, total);

        if (roll < common)
        {
            return AugmentRarity.Common;
        }

        roll -= common;
        if (roll < uncommon)
        {
            return AugmentRarity.Uncommon;
        }

        roll -= uncommon;
        if (roll < rare)
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