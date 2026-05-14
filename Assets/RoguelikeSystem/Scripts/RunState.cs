using System.Collections.Generic;

public class RunState
{
    private Dictionary<string, int> ownedStacks = new Dictionary<string, int>();

    public int Seed { get; set; }
    public string CurrentSceneName { get; set; }

    public int CurrentHealth { get; set; }
    public int CurrentStamina { get; set; }

    public IReadOnlyDictionary<string, int> OwnedStacks => ownedStacks;

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

    public void RemoveAugment(string id)
    {
        if (!ownedStacks.ContainsKey(id))
        {
            return;
        }

        ownedStacks[id]--;

        if (ownedStacks[id] <= 0)
        {
            ownedStacks.Remove(id);
        }
    }

    public RunSaveData ToSaveData()
    {
        RunSaveData data = new RunSaveData();
        data.runSeed = Seed;
        data.currentSceneName = CurrentSceneName;
        data.currentHealth = CurrentHealth;
        data.currentStamina = CurrentStamina;

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
        {
            return;
        }

        Seed = data.runSeed;
        CurrentSceneName = data.currentSceneName;
        CurrentHealth = data.currentHealth;
        CurrentStamina = data.currentStamina;

        for (int i = 0; i < data.ownedAugmentIds.Count; i++)
        {
            string id = data.ownedAugmentIds[i];
            int stacks = i < data.ownedAugmentStacks.Count ? data.ownedAugmentStacks[i] : 1;
            ownedStacks[id] = stacks;
        }
    }
}