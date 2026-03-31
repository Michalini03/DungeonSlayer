using System.Collections.Generic;

public class BuildContext
{
    public RunState RunState { get; }
    public HashSet<string> ActiveSynergyIds { get; }

    public BuildContext(RunState runState, IEnumerable<AugmentSynergyDefinition> activeSynergies)
    {
        RunState = runState;
        ActiveSynergyIds = new HashSet<string>();

        foreach (var synergy in activeSynergies)
        {
            ActiveSynergyIds.Add(synergy.id);
        }
    }

    public bool HasAugment(string id)
    {
        return RunState.HasAugment(id);
    }

    public int GetStacks(string id)
    {
        return RunState.GetStacks(id);
    }

    public bool HasSynergy(string id)
    {
        return ActiveSynergyIds.Contains(id);
    }
}