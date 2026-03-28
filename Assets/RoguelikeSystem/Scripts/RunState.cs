using System.Collections.Generic;

public class RunState
{
    private Dictionary<string, int> ownedStacks = new();

    public int Seed { get; set; }

    public void AddAugment(string id)
    {
        if (!ownedStacks.ContainsKey(id))
        {
            ownedStacks[id] = 0;
        }

        ownedStacks[id]++;
    }

    public bool HasAugment(string id)
    {
        return ownedStacks.ContainsKey(id);
    }

    public int GetStacks(string id)
    {
        return ownedStacks.TryGetValue(id, out int value) ? value : 0;
    }

    public RunSaveData ToSaveData()
    {
        RunSaveData data = new RunSaveData();
        data.runSeed = Seed;

        foreach (var kv in ownedStacks)
        {
            data.ownedAugmentIds.Add(kv.Key);
            data.ownedAugmentStacks.Add(kv.Value);
        }

        return data;
    }

    public void LoadFromSaveData(RunSaveData data)
    {
        ownedStacks.Clear();

        if (data == null)
            return;

        Seed = data.runSeed;

        for (int i = 0; i < data.ownedAugmentIds.Count; i++)
        {
            string id = data.ownedAugmentIds[i];
            int stacks = i < data.ownedAugmentStacks.Count ? data.ownedAugmentStacks[i] : 1;
            ownedStacks[id] = stacks;
        }
    }
}