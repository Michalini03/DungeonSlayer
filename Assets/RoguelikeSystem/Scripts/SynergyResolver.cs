using System.Collections.Generic;

public static class SynergyResolver
{
    public static List<AugmentSynergyDefinition> GetActiveSynergies(RunState run, IReadOnlyList<AugmentSynergyDefinition> allSynergies)
    {
        List<AugmentSynergyDefinition> active = new();

        foreach (var synergy in allSynergies)
        {
            if (IsSynergyActive(run, synergy))
            {
                active.Add(synergy);
            }
        }

        return active;
    }

    public static bool IsSynergyActive(RunState run, AugmentSynergyDefinition synergy)
    {
        if (synergy == null || synergy.requiredAugmentIds == null || synergy.requiredAugmentIds.Count == 0)
            return false;

        foreach (string requiredId in synergy.requiredAugmentIds)
        {
            if (!run.HasAugment(requiredId))
                return false;
        }

        return true;
    }
}